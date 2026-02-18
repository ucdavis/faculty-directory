using System.Collections.Generic;
using Serilog;
using Serilog.Sinks.OpenTelemetry;
using FacultyDirectory.Core.Models;

namespace FacultyDirectory.Core.Extensions
{
    public static class OtelSinkConfigurationExtensions
    {
        /// <summary>
        /// Configures the OpenTelemetry sink for Serilog if OTEL settings are provided
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration to extend</param>
        /// <param name="otelSettings">The OTEL settings from configuration</param>
        /// <returns>The logger configuration for chaining</returns>
        public static LoggerConfiguration AddOpenTelemetrySinkIfConfigured(
            this LoggerConfiguration loggerConfiguration, 
            OtelSettings otelSettings)
        {
            if (otelSettings == null || string.IsNullOrWhiteSpace(otelSettings.Endpoint))
            {
                return loggerConfiguration;
            }

            return loggerConfiguration.WriteTo.OpenTelemetry(options =>
            {
                options.Endpoint = otelSettings.Endpoint;
                
                // Build resource attributes from flattened properties
                // This allows service.name to come from appsettings.json
                // while deployment.environment can be overridden via Azure env vars
                var resourceAttributes = new Dictionary<string, object>();
                
                if (!string.IsNullOrWhiteSpace(otelSettings.ServiceName))
                {
                    resourceAttributes["service.name"] = otelSettings.ServiceName;
                }
                
                if (!string.IsNullOrWhiteSpace(otelSettings.DeploymentEnvironment))
                {
                    resourceAttributes["deployment.environment"] = otelSettings.DeploymentEnvironment;
                }
                
                if (resourceAttributes.Count > 0)
                {
                    options.ResourceAttributes = resourceAttributes;
                }
                
                // Configure headers if provided
                if (!string.IsNullOrWhiteSpace(otelSettings.Headers))
                {
                    try
                    {
                        // Try to parse as JSON dictionary
                        options.Headers = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(otelSettings.Headers);
                    }
                    catch
                    {
                        // If not JSON, treat as comma-separated key=value pairs
                        var headerDict = new Dictionary<string, string>();
                        var pairs = otelSettings.Headers.Split(',');
                        foreach (var pair in pairs)
                        {
                            var parts = pair.Split('=', 2);
                            if (parts.Length == 2)
                            {
                                headerDict[parts[0].Trim()] = parts[1].Trim();
                            }
                        }
                        if (headerDict.Count > 0)
                        {
                            options.Headers = headerDict;
                        }
                    }
                }
            });
        }
    }
}
