using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Microsoft.EntityFrameworkCore;

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
        public async Task<string> PublishPerson(SitePerson sitePerson)
        {
            // Get the related information for this person
            var tags = await this.dbContext.SitePeopleTags.Include(s => s.SiteTag).Where(s => s.SitePersonId == sitePerson.Id).ToArrayAsync();

            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).ToArrayAsync();

            var baseUrl = "https://playground.sf.ucdavis.edu/jsonapi/node/sf_person";
            var method = HttpMethod.Post;

            if (sitePerson.PageUid.HasValue) {
                baseUrl = baseUrl + "/" + sitePerson.PageUid.Value;
                method = System.Net.Http.HttpMethod.Put;
            }

            var personData = new
            {
                data = new
                {
                    type = "node--sf_person",
                    attributes = new
                    {
                        title = sitePerson.Name,  // what goes here?
                        field_sf_first_name = sitePerson.Person.FirstName,
                        field_sf_last_name = sitePerson.Person.LastName
                    }
                }
            };

            var serialized = JsonSerializer.Serialize(personData);

            var request = new HttpRequestMessage(HttpMethod.Post, baseUrl);

            request.Content = new StringContent(serialized, Encoding.UTF8, "application/vnd.api+json");

            var response = await this.httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // TODO: make models and deserialze properly
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

            var id = json.data.id;

            if (sitePerson.PageUid.HasValue == false) {
                // if this is a new site entity, save the page uid for future updates
                sitePerson.PageUid = id;

                await this.dbContext.SaveChangesAsync();
            }

            return content;
        }
    }
}