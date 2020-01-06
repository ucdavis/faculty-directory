using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;

namespace FacultyDirectory.Core.Services
{
    public interface ISiteFarmService
    {
        Task<string> PublishPerson(SitePerson person);
    }

    public class SiteFarmService : ISiteFarmService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient httpClient;

        public SiteFarmService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClient;

            // TODO: determine if we want to set auth at the service level
            this.httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", "apiuser", "pass"))));
        }

        // TODO: handle multiple sites?
        public async Task<string> PublishPerson(SitePerson person)
        {
            var baseUrl = "https://playground.sf.ucdavis.edu/jsonapi/node/sf_person";

            var personData = new
            {
                data = new
                {
                    type = "node--sf_person",
                    attributes = new
                    {
                        title = "Api Person",
                        field_sf_first_name = "Api",
                        field_sf_last_name = "Person"
                    }
                }
            };

            var serialized = JsonSerializer.Serialize(personData);
            var serialized2 = Newtonsoft.Json.JsonConvert.SerializeObject(personData);

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);

            request.Content = new StringContent(serialized2, Encoding.UTF8, "application/vnd.api+json");

            var response = await this.httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return content;
        }
    }
}