using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using FacultyDirectory.Core.Models;

namespace FacultyDirectory.Core.Extensions
{
    public static class OtelSinkConfigurationExtensions
    {
        public static LoggerConfiguration AddOpenTelemetrySinkIfConfigured(
            this LoggerConfiguration loggerConfiguration,
            SerilogSettings serilogSettings)
        {
            if (serilogSettings == null
                || string.IsNullOrWhiteSpace(serilogSettings.OtelEndpoint)
                || string.IsNullOrWhiteSpace(serilogSettings.OtelAuthHeader))
            {
                return loggerConfiguration;
            }

            var parts = serilogSettings.OtelAuthHeader.Split('=', 2);
            if (parts.Length != 2)
            {
                Log.Error("Invalid OpenTelemetry auth header format. Expected 'Authorization=Bearer <token>'.", serilogSettings.OtelAuthHeader);
                return loggerConfiguration;
            }

            return loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = serilogSettings.OtelEndpoint;
                
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = serilogSettings.AppName,
                    ["deployment.environment"] = serilogSettings.Environment
                };
                
                options.Headers = new Dictionary<string, string>
                {
                    [parts[0].Trim()] = parts[1].Trim()
                };
            });
        }
    }
}
