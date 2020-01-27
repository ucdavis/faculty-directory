using System;

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
            // var directoryPopulationService = provider.GetService<IDirectoryPopulationService>();

            _log.Information("Process Sources Job Finished");
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddDbContextPool<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient<IDirectoryPopulationService, DirectoryPopulationService>();
            services.Configure<DirectoryConfiguration>(Configuration.GetSection("Directory"));

            return services.BuildServiceProvider();
        }
    }
}
