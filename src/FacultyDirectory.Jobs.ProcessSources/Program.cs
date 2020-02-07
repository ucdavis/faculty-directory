using System;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Services;
using FacultyDirectory.Jobs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FacultyDirectory.Jobs.ProcessSources
{
    public class Program : JobBase
    {
        private static ILogger _log;
        private static Random _random;

        static void Main(string[] args)
        {
            // base config
            Configure();

            var assembyName = typeof(Program).Assembly.GetName();

            _log = Log.Logger
                .ForContext("jobname", assembyName.Name)
                .ForContext("jobid", Guid.NewGuid());

            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            var provider = ConfigureServices();
            var scholarService = provider.GetService<IScholarService>();
            var dbContext = provider.GetService<ApplicationDbContext>();

            // Setup random number generator
            _random = new Random();

            // First, get anyone who hasn't had any scholar information added yet
            ProcessFirstTimers(dbContext, scholarService).GetAwaiter().GetResult();

            // Now look for updates for people who haven't been updated recently
            // ProcessExisting(dbContext, scholarService).GetAwaiter().GetResult();

            _log.Information("Process Sources Job Finished");
        }

        private static async Task ProcessFirstTimers(ApplicationDbContext dbContext, IScholarService scholarService)
        {
            // grab N random people who need sources setup first time
            var firstTimePeopleWithoutScholarSource = await dbContext.People.Where(p => !p.Sources.Any(s => s.Source == "scholar")).Take(100).ToArrayAsync();

            _log.Information("Processing {num} people without scholar sources", firstTimePeopleWithoutScholarSource.Length);

            foreach (var person in firstTimePeopleWithoutScholarSource)
            {
                try
                {
                    _log.Information("Processing {name} ({id})", person.FullName, person.Id);

                    await scholarService.SyncForPerson(person.Id);
                }
                catch (ApplicationException)
                {
                    _log.Warning("Could not process user {id}", person.Id);
                }
                finally
                {
                    // wait a little before trying the next one to make sure our data source is happy
                    await Task.Delay(_random.Next(500, 1500));
                }
            }
        }

        private static async Task ProcessExisting(ApplicationDbContext dbContext, IScholarService scholarService)
        {
            // grab N people with scholar who haven't been updated recently
            var oldestUpdatedScholarPeopleIds = await dbContext.PeopleSources
                    .Where(s => s.Source == "scholar")
                    .OrderBy(s => s.LastUpdate)
                    .Select(s => s.PersonId).Take(25).ToArrayAsync();

            _log.Information("Updating scholar information for {num} people ", oldestUpdatedScholarPeopleIds.Length);

            foreach (var personId in oldestUpdatedScholarPeopleIds)
            {
                try
                {
                    _log.Information("Processing {id}", personId);

                    await scholarService.SyncForPerson(personId);
                }
                catch (ApplicationException)
                {
                    _log.Warning("Could not process user {id}", personId);
                }
                finally
                {
                    // wait a little before trying the next one to make sure our data source is happy
                    await Task.Delay(_random.Next(500, 1500));
                }
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddDbContextPool<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient<IScholarService, ScholarService>();

            return services.BuildServiceProvider();
        }
    }
}
