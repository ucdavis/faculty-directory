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

namespace FacultyDirectory.Jobs.SyncSites
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
            var dbContext = provider.GetService<ApplicationDbContext>();
            var siteFarmService = provider.GetService<ISiteFarmService>();

            // Process the sync
            ProcessSitePeople(dbContext, siteFarmService).GetAwaiter().GetResult();

            _log.Information("Sync Sites Job Finished");
        }

        private static async Task ProcessSitePeople(ApplicationDbContext dbContext, ISiteFarmService siteFarmService)
        {
            // grab N people who haven't been sync'd in a while
            var sitePeople = await dbContext.SitePeople.Where(sp => sp.ShouldSync)
                .OrderBy(sp => sp.LastSync).Include(sp => sp.Person).Take(50).ToArrayAsync();

            _log.Information("Syncing {num} people to SiteFarm", sitePeople.Length);

            foreach (var sitePerson in sitePeople)
            {
                try
                {
                    _log.Information("Processing {name} ({id})", sitePerson.Person.FullName, sitePerson.Person.Id);

                    await siteFarmService.PublishPerson(sitePerson);
                }
                catch
                {
                    _log.Warning("Could not process user {id}", sitePerson.Person.Id);
                }
            }
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddDbContextPool<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.Configure<SiteFarmConfiguration>(Configuration.GetSection("SiteFarm"));

            services.AddTransient<IBiographyGenerationService, BiographyGenerationService>();

            services.AddHttpClient<ISiteFarmService, SiteFarmService>();

            return services.BuildServiceProvider();
        }
    }
}
