using System;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Models;
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

            // First, get anyone who hasn't had any scholar information added yet
            ProcessFirstTimers(dbContext, scholarService).GetAwaiter().GetResult(); 

            _log.Information("Process Sources Job Finished");
        }

        private static async Task ProcessFirstTimers(ApplicationDbContext dbContext, IScholarService scholarService) {
            // grab N random people who need sources setup first time
            var firstTimePeopleWithoutScholarSource = await dbContext.People.Where(p => !p.Sources.Any(s => s.Source == "scholar")).Take(25).ToListAsync();

            foreach (var person in firstTimePeopleWithoutScholarSource)
            {
                await scholarService.SyncForPerson(person.Id);

                // wait a little before trying the next one to make sure our data source is happy
                await Task.Delay(500);
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
