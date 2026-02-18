namespace FacultyDirectory.Core.Models
{
    public class SerilogSettings
    {
        public string AppName { get; set; }
        public string Environment { get; set; }
        public OtelSettings OTEL { get; set; }
    }

    public class OtelSettings
    {
        public string Endpoint { get; set; }
        public string Headers { get; set; }
        public string ServiceName { get; set; }
        public string DeploymentEnvironment { get; set; }
    }
}
