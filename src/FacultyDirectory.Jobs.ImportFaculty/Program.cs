using System;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Models;
using FacultyDirectory.Core.Services;
using FacultyDirectory.Jobs.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace FacultyDirectory.Jobs.ImportFaculty
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

            //// setup di
            var provider = ConfigureServices();
            var dbContext = provider.GetService<ApplicationDbContext>();
            // var emailService = provider.GetService<IEmailService>();

            _log.Information("Import Faculty Job Finished");
        }

        private static ServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddOptions();
            services.AddDbContextPool<ApplicationDbContext>(o => o.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddHttpClient<IDirectoryPopulationService, DirectoryPopulationService>();
            // services.AddTransient<IEmailService, EmailService>();
            //services.Configure<DirectoryConfiguration>(Configuration.GetSection("Directory"));

            return services.BuildServiceProvider();
        }
    }
}
