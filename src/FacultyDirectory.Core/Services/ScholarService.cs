using System.Net.Http;
using FacultyDirectory.Core.Data;

namespace FacultyDirectory.Core.Services
{
    public class ScholarService {
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient httpClient;

        public ScholarService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClient;
        }

        
    }
}