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
            OtelSettings otelSettings)
        {
            if (otelSettings == null
                || string.IsNullOrWhiteSpace(otelSettings.Endpoint)
                || string.IsNullOrWhiteSpace(otelSettings.ServiceName)
                || string.IsNullOrWhiteSpace(otelSettings.DeploymentEnvironment)
                || string.IsNullOrWhiteSpace(otelSettings.AuthHeader))
            {
                return loggerConfiguration;
            }

            return loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otelSettings.Endpoint;
                
                options.ResourceAttributes = new Dictionary<string, object>
                {
                    ["service.name"] = otelSettings.ServiceName,
                    ["deployment.environment"] = otelSettings.DeploymentEnvironment
                };
                
                var parts = otelSettings.AuthHeader.Split('=', 2);
                options.Headers = new Dictionary<string, string>
                {
                    [parts[0].Trim()] = parts[1].Trim()
                };
            });
        }
    }
}
