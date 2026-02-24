namespace FacultyDirectory.Core.Models
{
    public class SerilogSettings
    {
        public string AppName { get; set; } = "";
        public string Environment { get; set; } = "";
        public string OtelEndpoint { get; set; } = "";
        public string OtelAuthHeader { get; set; } = "";
        public bool SelfLog { get; set; } = false;
    }
}
