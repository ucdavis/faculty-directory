﻿using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FacultyDirectory.Jobs.Core
{
    public abstract class JobBase
    {
        public static IConfigurationRoot Configuration { get; set; }

        protected static void Configure()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (string.Equals(environmentName, "development", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddUserSecrets<JobBase>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            LogConfiguration.Setup(Configuration);
        }
    }
}
