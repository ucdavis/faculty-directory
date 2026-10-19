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

        static int Main(string[] args)
        {
            // base config
            Configure();

            var assembyName = typeof(Program).Assembly.GetName();

            _log = Log.Logger
                .ForContext("jobname", assembyName.Name)
                .ForContext("jobid", Guid.NewGuid());

            _log.Information("Running {job} build {build}", assembyName.Name, assembyName.Version);

            // setup di
            using var provider = ConfigureServices();
            var dbContext = provider.GetService<ApplicationDbContext>();
            var siteFarmService = provider.GetService<ISiteFarmService>();

            // Process the sync
            return RunAsync(dbContext, siteFarmService, _log).GetAwaiter().GetResult();
        }

        public static async Task<int> RunAsync(ApplicationDbContext dbContext, ISiteFarmService siteFarmService, ILogger log)
        {
            // grab N people who haven't been sync'd in a while
            var sitePeople = await dbContext.SitePeople.Where(sp => sp.ShouldSync)
                .OrderBy(sp => sp.LastSync).Include(sp => sp.Person).Take(50).ToArrayAsync();

            log.Information("Syncing {num} people to SiteFarm", sitePeople.Length);

            var succeeded = 0;
            var failed = 0;
            foreach (var sitePerson in sitePeople)
            {
                try
                {
                    await siteFarmService.PublishPerson(sitePerson);
                    succeeded++;
                }
                catch (Exception exception)
                {
                    failed++;
                    // Upstream error bodies and exception messages may contain personal
                    // data or credentials. Log only controlled diagnostics and the local record key.
                    if (exception is SiteFarmRequestException siteFarmError)
                    {
                        log.Error("Site person {SitePersonId} failed during {Operation}: {Failure}; HTTP {StatusCode}",
                            sitePerson.Id, siteFarmError.Operation, siteFarmError.Failure, (int?)siteFarmError.StatusCode);
                    }
                    else
                    {
                        log.Error("Site person {SitePersonId} failed with {ExceptionType}",
                            sitePerson.Id, exception.GetType().Name);
                    }
                }
            }

            log.Information("Sync Sites batch finished: {Attempted} attempted, {Succeeded} succeeded, {Failed} failed",
                sitePeople.Length, succeeded, failed);
            return failed == 0 ? 0 : 1;
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
