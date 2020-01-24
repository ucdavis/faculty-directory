﻿using System;
using FacultyDirectory.Jobs.Core;
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
            //// TODO

            _log.Information("Import Faculty Job Finished");
        }
    }
}