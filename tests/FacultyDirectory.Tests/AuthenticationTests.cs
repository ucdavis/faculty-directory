using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Serilog;
using Xunit;

namespace FacultyDirectory.Tests;

public sealed class AuthenticationTests : IAsyncLifetime
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private IHost host = null!;
    private HttpClient client = null!;

    public async Task InitializeAsync()
    {
        await connection.OpenAsync();
        // Use the application's Startup and real cookie/OIDC handlers, with only
        // local fixtures. No user secrets, discovery requests, or campus APIs.
        host = await new HostBuilder()
            .UseSerilog((_, logger) => logger.MinimumLevel.Warning())
            .ConfigureWebHost(web => web
                .UseEnvironment("Testing")
                .UseTestServer()
                .ConfigureAppConfiguration(config => config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Authentication:ClientId"] = "local-tests",
                    ["Authentication:ClientSecret"] = "local-test-secret",
                    ["Authentication:Authority"] = "https://identity.example.test",
                }))
                .UseStartup<Startup>()
                .ConfigureTestServices(services =>
                {
                    services.AddControllers().AddApplicationPart(typeof(Startup).Assembly);
                    services.AddDataProtection().UseEphemeralDataProtectionProvider();
                    services.RemoveAll<ApplicationDbContext>();
                    services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                    services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
                    services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                    {
                        options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(
                            new OpenIdConnectConfiguration
                            {
                                Issuer = "https://identity.example.test",
                                AuthorizationEndpoint = "https://identity.example.test/authorize",
                                TokenEndpoint = "https://identity.example.test/token",
                            });
                    });
                }))
            .StartAsync();

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
        db.Users.Add(new User { Username = "admin" });
        await db.SaveChangesAsync();
        client = host.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");
    }

    [Fact]
    public async Task AnonymousApiRequestStillChallengesExistingOidcProvider()
    {
        var response = await client.GetAsync("/api/faculty/name");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var redirect = Assert.IsType<Uri>(response.Headers.Location);
        Assert.Equal("https://identity.example.test/authorize", redirect.GetLeftPart(UriPartial.Path));
        var query = QueryHelpers.ParseQuery(redirect.Query);
        Assert.Equal("code", query["response_type"].ToString());
        Assert.Equal("local-tests", query["client_id"].ToString());
        Assert.Equal("https://localhost/signin-oidc", query["redirect_uri"].ToString());
        Assert.Contains("ucdProfile", query["scope"].ToString().Split(' '));
        Assert.Contains("eduPerson", query["scope"].ToString().Split(' '));
    }

    [Fact]
    public async Task SignedInNonAdminGets403InsteadOf302()
    {
        SignIn("faculty");

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SignedInFacultyCanReadName()
    {
        SignIn("faculty");

        var response = await client.GetAsync("/api/faculty/name");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.Equal("Local Faculty", result!["name"]);
    }

    [Fact]
    public async Task AdminCanCreateReadAndDeleteUser()
    {
        SignIn("admin");

        var created = await client.PostAsJsonAsync("/api/users", new { Username = "new-admin" });
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var user = await created.Content.ReadFromJsonAsync<User>();
        Assert.True(user!.Id > 0);
        Assert.Equal("new-admin", user.Username);

        var duplicate = await client.PostAsJsonAsync("/api/users", new { Username = "new-admin" });
        Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        var users = await client.GetFromJsonAsync<User[]>("/api/users");
        Assert.Contains(users!, entry => entry.Id == user.Id);

        var deleted = await client.DeleteAsync($"/api/users/{user.Id}");
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        users = await client.GetFromJsonAsync<User[]>("/api/users");
        Assert.DoesNotContain(users!, entry => entry.Id == user.Id);
    }

    private void SignIn(string username)
    {
        var options = host.Services.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.NameIdentifier, username),
            new Claim(ClaimTypes.GivenName, "Local"),
            new Claim(ClaimTypes.Surname, "Faculty"),
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), new AuthenticationProperties(),
            CookieAuthenticationDefaults.AuthenticationScheme);
        client.DefaultRequestHeaders.Add("Cookie", $"{options.Cookie.Name}={options.TicketDataFormat.Protect(ticket)}");
    }

    public async Task DisposeAsync()
    {
        client?.Dispose();
        if (host != null)
        {
            await host.StopAsync();
            host.Dispose();
        }
        await connection.DisposeAsync();
    }
}
