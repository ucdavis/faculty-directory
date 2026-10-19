using System.Net;
using System.Net.Http;

namespace FacultyDirectory.Core.Services
{
    // Contains only application-defined diagnostics. Never include response bodies,
    // request URLs, credentials, or upstream exception messages.
    public sealed class SiteFarmRequestException : HttpRequestException
    {
        public string Operation { get; }
        public string Failure { get; }

        public SiteFarmRequestException(string operation, HttpStatusCode? statusCode, string failure)
            : base($"SiteFarm {operation} failed: {failure} (HTTP {(int?)statusCode}).", null, statusCode)
        {
            Operation = operation;
            Failure = failure;
        }
    }
}
