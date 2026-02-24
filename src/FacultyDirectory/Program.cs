using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using FacultyDirectory.Core.Models;
using FacultyDirectory.Core.Extensions;
namespace FacultyDirectory
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var isDevelopment = string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"), "development", StringComparison.OrdinalIgnoreCase);
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();

            //only add secrets in development
            if (isDevelopment)
            {
                builder.AddUserSecrets<Program>();
            }
            var configuration = builder.Build();
            
            // Bind Serilog settings to model
            var settings = new SerilogSettings();
            configuration.GetSection("Serilog").Bind(settings);

            // Enable Serilog internal logging to diagnose sink failures
            var homeDir = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrEmpty(homeDir) && settings.SelfLog)
            {
                var selfLogPath = System.IO.Path.Combine(homeDir, "LogFiles", "serilog-selflog.txt");
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(selfLogPath));
                var selfLogWriter = System.IO.File.AppendText(selfLogPath);
                selfLogWriter.AutoFlush = true;
                Serilog.Debugging.SelfLog.Enable(msg => selfLogWriter.WriteLine($"{DateTime.UtcNow:o} {msg}"));
            }


            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", settings.AppName)
                .Enrich.WithProperty("AppEnvironment", settings.Environment)
                // .Enrich.WithExceptionDetails()
                .WriteTo.Console();

            // add OpenTelemetry sink if endpoint is configured
            loggerConfig = loggerConfig.AddOpenTelemetrySinkIfConfigured(settings);

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                }).UseSerilog();
    }
}
