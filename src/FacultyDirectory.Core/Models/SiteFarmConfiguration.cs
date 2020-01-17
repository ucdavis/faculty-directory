namespace FacultyDirectory.Core.Models
{
    // TODO: need to store elsewhere for multi-site support
    public class SiteFarmConfiguration {
        public string ApiBase { get; set; }
        public string ApiUsername { get; set; }
        public string ApiPassword { get; set; }
    }
}