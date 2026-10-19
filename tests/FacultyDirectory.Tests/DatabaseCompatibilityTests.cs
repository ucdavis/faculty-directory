using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Models;
using FacultyDirectory.Core.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace FacultyDirectory.Tests;

public sealed class DatabaseCompatibilityTests
{
    [Fact]
    public void SqlServerModelDoesNotRequireSchemaChanges()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer("Server=localhost;Database=ModelOnly;Integrated Security=true").Options);

        // EF compares the current relational model to the existing migration
        // snapshot; it does not open this connection.
        Assert.False(db.Database.HasPendingModelChanges());
    }

    [Fact]
    public async Task FacultyMergeUpdatesExistingAndAddsNewPeople()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection).Options);
        await db.Database.EnsureCreatedAsync();
        await VerifyMerge(db, 10);
    }

    [SqlServerTheory]
    [InlineData(10)]
    [InlineData(2500)]
    public async Task ExistingSqlServerMigrationsAndFacultyMergeWork(int count)
    {
        var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("FACULTY_DIRECTORY_TEST_SQL"));
        // Each test owns a fresh local database, never the supplied catalog.
        Assert.Contains(connection.DataSource.Split(',')[0], new[] { "localhost", "127.0.0.1" });
        connection.InitialCatalog = $"FacultyDirectoryTests_{Guid.NewGuid():N}";
        await using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connection.ConnectionString).Options);
        try
        {
            await db.Database.MigrateAsync();
            await VerifyMerge(db, count);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static async Task VerifyMerge(ApplicationDbContext db, int count)
    {
        var existing = new Person { IamId = "000000", FirstName = "Old", Classification = "faculty" };
        var untouched = new Person { IamId = "outside-import", FirstName = "Unchanged" };
        db.People.AddRange(existing, untouched);
        await db.SaveChangesAsync();
        var originalId = existing.Id;
        db.ChangeTracker.Clear();

        var people = Enumerable.Range(0, count).Select(i => new Person
        {
            IamId = i.ToString("D6"),
            FirstName = "Updated",
            LastName = $"Faculty{i}",
            Email = $"faculty{i}@example.test",
            Departments = "Local department",
        }).ToArray();
        using var http = new HttpClient(new RejectExternalRequests());
        var service = new DirectoryPopulationService(db, http, Options.Create(new DirectoryConfiguration()));
        await service.MergeFaculty(people);
        db.ChangeTracker.Clear();

        Assert.Equal(count + 1, await db.People.CountAsync());
        var updated = await db.People.SingleAsync(person => person.IamId == "000000");
        Assert.Equal(originalId, updated.Id);
        Assert.Equal("Updated", updated.FirstName);
        Assert.Equal("faculty0@example.test", updated.Email);
        Assert.Equal("faculty", updated.Classification);
        Assert.Equal("Unchanged", (await db.People.SingleAsync(person => person.IamId == "outside-import")).FirstName);
    }

    private sealed class RejectExternalRequests : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("These tests must not contact external services.");
    }
}

public sealed class SqlServerTheoryAttribute : TheoryAttribute
{
    public SqlServerTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("FACULTY_DIRECTORY_TEST_SQL")))
            Skip = "Set FACULTY_DIRECTORY_TEST_SQL to a disposable local SQL Server instance to run SQL Server tests.";
    }
}
