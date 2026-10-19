using System.Net;
using System.Text.Json;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Models;
using FacultyDirectory.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace FacultyDirectory.Tests;

public sealed class SiteFarmSyncTests
{
    [Fact]
    public async Task RejectedPublishReportsHttpStatusAndDoesNotAdvanceSync()
    {
        await using var fixture = await SyncFixture.Create();
        var before = fixture.Person.LastSync;
        using var http = new HttpClient(new ResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"errors":[{"status":"403","detail":"private biography and credentials"}]}""")
        }));
        var service = fixture.Service(http);

        var error = await Assert.ThrowsAnyAsync<HttpRequestException>(() => service.PublishPerson(fixture.Person));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Contains("publish person", error.Message);
        Assert.DoesNotContain("private", error.ToString());
        await fixture.Db.Entry(fixture.Person).ReloadAsync();
        Assert.Equal(before, fixture.Person.LastSync);
    }

    [Fact]
    public async Task RejectedTagCreationReportsStatusAndNeverPublishesPerson()
    {
        await using var fixture = await SyncFixture.Create();
        fixture.Person.Tags = "fixture tag";
        await fixture.Db.SaveChangesAsync();
        var before = fixture.Person.LastSync;
        var requests = new List<string>();
        using var http = new HttpClient(new ResponseHandler(request =>
        {
            requests.Add(request.Method + " " + request.RequestUri!.AbsolutePath);
            return new HttpResponseMessage(request.Method == HttpMethod.Get ? HttpStatusCode.OK : HttpStatusCode.Forbidden)
            {
                Content = new StringContent(request.Method == HttpMethod.Get ? "{\"data\":[]}" : "{\"errors\":[{\"detail\":\"private tag\"}]}")
            };
        }));

        var error = await Assert.ThrowsAnyAsync<HttpRequestException>(() => fixture.Service(http).PublishPerson(fixture.Person));

        Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
        Assert.Contains("create tag", error.Message);
        Assert.DoesNotContain("private", error.ToString());
        Assert.Equal(new[] { "GET /jsonapi/taxonomy_term/sf_tags", "POST /jsonapi/taxonomy_term/sf_tags" }, requests);
        await fixture.Db.Entry(fixture.Person).ReloadAsync();
        Assert.Equal(before, fixture.Person.LastSync);
    }

    [Fact]
    public async Task BatchContinuesAfterRejectedPersonAndReturnsFailure()
    {
        await using var fixture = await SyncFixture.Create();
        var successful = new SitePerson
        {
            Person = new Person { IamId = "second", FirstName = "Second", Classification = "faculty" },
            Site = fixture.Person.Site,
            PageUid = Guid.Parse("22222222-2222-4222-8222-222222222222"),
            ShouldSync = true,
            LastSync = fixture.Person.LastSync!.Value.AddDays(1),
        };
        fixture.Db.SitePeople.Add(successful);
        await fixture.Db.SaveChangesAsync();
        var before = fixture.Person.LastSync;
        using var http = new HttpClient(new ResponseHandler(request => request.RequestUri!.AbsolutePath.EndsWith(fixture.Person.PageUid.ToString()!)
            ? new HttpResponseMessage(HttpStatusCode.Forbidden) { Content = new StringContent("private error") }
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"data\":{\"id\":\"22222222-2222-4222-8222-222222222222\"}}") }));
        var sink = new LogSink();
        using var log = new Serilog.LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();

        var exitCode = await FacultyDirectory.Jobs.SyncSites.Program.RunAsync(fixture.Db, fixture.Service(http), log);

        Assert.Equal(1, exitCode);
        await fixture.Db.Entry(fixture.Person).ReloadAsync();
        await fixture.Db.Entry(successful).ReloadAsync();
        Assert.Equal(before, fixture.Person.LastSync);
        Assert.True(successful.LastSync > DateTime.UtcNow.AddMinutes(-1));
        Assert.Contains(sink.Events, e => e.Properties.TryGetValue("Failed", out var failed) && failed.ToString() == "1"
            && e.Properties["Succeeded"].ToString() == "1");
        Assert.DoesNotContain(sink.Events, e => e.RenderMessage().Contains("private") || e.Exception != null);
    }

    [Theory]
    [InlineData("{\"data\":{}}")]
    [InlineData("{\"data\":{\"id\":\"not-a-uuid\"}}")]
    [InlineData("{\"data\":{\"id\":\"22222222-2222-4222-8222-222222222222\"}}")]
    [InlineData("null")]
    [InlineData("<html>private gateway response</html>")]
    public async Task InvalidSuccessResponseDoesNotAdvanceSync(string responseBody)
    {
        await using var fixture = await SyncFixture.Create();
        var before = fixture.Person.LastSync;
        var pageUid = fixture.Person.PageUid;
        using var http = new HttpClient(new ResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody)
        }));

        var error = await Assert.ThrowsAsync<SiteFarmRequestException>(() => fixture.Service(http).PublishPerson(fixture.Person));

        Assert.Equal("invalid response", error.Failure);
        Assert.DoesNotContain("private", error.ToString());
        await fixture.Db.Entry(fixture.Person).ReloadAsync();
        Assert.Equal(before, fixture.Person.LastSync);
        Assert.Equal(pageUid, fixture.Person.PageUid);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SuccessfulPublishPersistsPageAndSyncTime(bool existing)
    {
        await using var fixture = await SyncFixture.Create(existing);
        fixture.Person.Tags = "water & soil#1";
        var requests = new List<string>();
        using var http = new HttpClient(new ResponseHandler(request =>
        {
            requests.Add(request.Method.Method);
            if (request.Method == HttpMethod.Get)
            {
                Assert.Equal("?filter[name]=water%20%26%20soil%231", request.RequestUri!.Query);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"data\":[]}") };
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("sf_tags"))
                return new HttpResponseMessage(HttpStatusCode.Created) { Content = new StringContent("{\"data\":{\"id\":\"33333333-3333-4333-8333-333333333333\"}}") };
            using var payload = JsonDocument.Parse(request.Content!.ReadAsStringAsync().GetAwaiter().GetResult());
            var relationships = payload.RootElement.GetProperty("data").GetProperty("relationships");
            Assert.Equal(!existing, relationships.TryGetProperty("field_sf_primary_image", out _));
            Assert.Equal("33333333-3333-4333-8333-333333333333", relationships.GetProperty("field_sf_tags").GetProperty("data")[0].GetProperty("id").GetString());
            return new HttpResponseMessage(existing ? HttpStatusCode.OK : HttpStatusCode.Created)
            {
                Content = new StringContent("{\"data\":{\"id\":\"11111111-1111-4111-8111-111111111111\"}}")
            };
        }));
        var started = DateTime.UtcNow;

        var result = await fixture.Service(http).PublishPerson(fixture.Person);

        await fixture.Db.Entry(fixture.Person).ReloadAsync();
        Assert.Equal(Guid.Parse("11111111-1111-4111-8111-111111111111"), fixture.Person.PageUid);
        Assert.True(fixture.Person.LastSync >= started);
        Assert.Contains("11111111-1111-4111-8111-111111111111", result);
        Assert.Equal(new[] { "GET", "POST", existing ? "PATCH" : "POST" }, requests);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BatchReturnsSuccessForEmptyOrSuccessfulSelection(bool enabled)
    {
        await using var fixture = await SyncFixture.Create();
        fixture.Person.ShouldSync = enabled;
        await fixture.Db.SaveChangesAsync();
        using var http = new HttpClient(new ResponseHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"data\":{\"id\":\"11111111-1111-4111-8111-111111111111\"}}")
        }));
        var sink = new LogSink();
        using var log = new Serilog.LoggerConfiguration().WriteTo.Sink(sink).CreateLogger();

        var exitCode = await FacultyDirectory.Jobs.SyncSites.Program.RunAsync(fixture.Db, fixture.Service(http), log);

        Assert.Equal(0, exitCode);
        Assert.Contains(sink.Events, e => e.Properties.TryGetValue("Failed", out var failed) && failed.ToString() == "0"
            && e.Properties["Succeeded"].ToString() == (enabled ? "1" : "0"));
    }

    private sealed class LogSink : Serilog.Core.ILogEventSink
    {
        public List<Serilog.Events.LogEvent> Events { get; } = [];
        public void Emit(Serilog.Events.LogEvent logEvent) => Events.Add(logEvent);
    }

    private sealed class ResponseHandler(Func<HttpRequestMessage, HttpResponseMessage> respond) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(respond(request));
    }

    private sealed class SyncFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public ApplicationDbContext Db { get; }
        public SitePerson Person { get; }
        private SyncFixture(SqliteConnection connection, ApplicationDbContext db, SitePerson person)
            => (this.connection, Db, Person) = (connection, db, person);

        public static async Task<SyncFixture> Create(bool existing = true)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);
            await db.Database.EnsureCreatedAsync();
            var person = new SitePerson
            {
                Person = new Person { IamId = "fixture", FirstName = "Test", LastName = "Faculty", Classification = "faculty" },
                Site = new Site { Name = "Fixture" },
                PageUid = existing ? Guid.Parse("11111111-1111-4111-8111-111111111111") : null,
                LastSync = new DateTime(2023, 12, 6, 11, 30, 0, DateTimeKind.Utc),
                ShouldSync = true,
            };
            db.SitePeople.Add(person);
            await db.SaveChangesAsync();
            return new SyncFixture(connection, db, person);
        }

        public SiteFarmService Service(HttpClient http) => new(Db, new BiographyGenerationService(Db), http,
            Options.Create(new SiteFarmConfiguration { ApiBase = "https://sitefarm.example.test/jsonapi", ApiUsername = "fixture", ApiPassword = "secret-fixture" }));

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
