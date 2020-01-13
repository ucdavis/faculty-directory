using System;
using System.Collections.Generic;
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
        IAsyncEnumerable<string> SyncTags(string[] tags);
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
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", "apiuser2", "pass"))));
        }

        public async IAsyncEnumerable<string> SyncTags(string[] tags)
        {
            var baseUrl = "https://playground.sf.ucdavis.edu/jsonapi/taxonomy_term/sf_tags";

            // for each tag where we don't have an existing UID, we need to create and save it
            foreach (var tag in tags)
            {
                // TODO: make actual object so we don't have to deserialize anon
                // query for tag 
                dynamic existingTagResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(await this.httpClient.GetStringAsync(baseUrl + "?filter[name]=" + tag));

                if (existingTagResponse.data.Count > 0)
                {
                    yield return existingTagResponse.data[0].id;
                }
                else
                {
                    // No existing tag record found, make a new one

                    var tagData = new
                    {
                        data = new
                        {
                            type = "taxonomy_term--sf_tags",
                            attributes = new
                            {
                                name = tag,
                            }
                        }
                    };

                    var serialized = JsonSerializer.Serialize(tagData);

                    var createResponse = await this.httpClient.PostAsync(baseUrl, new StringContent(serialized, Encoding.UTF8, "application/vnd.api+json"));
                    var content = await createResponse.Content.ReadAsStringAsync();

                    // TODO: make models and deserialze properly
                    dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

                    var id = json.data.id;

                    yield return id;
                }
            }
        }

        // TODO: handle multiple sites?
        public async Task<string> PublishPerson(SitePerson sitePerson)
        {
            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).ToArrayAsync();

            // TODO: determine which site values to use for each property

            // Step 1: Sync tag taxonomy

            // TODO: get from sources and site person
            var tags = new[] { "code", "javascript", "html" };

            var tagNodes = new List<object>();

            await foreach (var tagId in this.SyncTags(tags))
            {
                tagNodes.Add(new {
                    type = "taxonomy_term--sf_tags",
                    id = tagId
                });
            }

            // now we have our tag relationship
            var field_sf_tags = new {
                    data = tagNodes
            };

            // Step 2: form POST/PATCH depending on prior existance of user page
            var resourceUrl = "https://playground.sf.ucdavis.edu/jsonapi/node/sf_person";
            var method = HttpMethod.Post;

            if (sitePerson.PageUid.HasValue)
            {
                resourceUrl = resourceUrl + "/" + sitePerson.PageUid.Value;
                method = System.Net.Http.HttpMethod.Patch;
            }

            // Step 3: Compile and generate user page info
            var personData = new
            {
                data = new
                {
                    type = "node--sf_person",
                    id = sitePerson.PageUid,
                    attributes = new
                    {
                        title = sitePerson.Name,  // what goes here?
                        field_sf_first_name = sitePerson.Person.FirstName,
                        field_sf_last_name = sitePerson.Person.LastName,
                        field_sf_position_title = sitePerson.Title ?? sitePerson.Name,
                        field_sf_emails = new [] {
                            sitePerson.Email // TODO: extend for multiple
                        },
                        field_sf_phone_numbers = new [] {
                            sitePerson.Phone // TODO: extend for multiple
                        },
                        field_sf_unit = new [] {
                            sitePerson.Departments // TODO: parse and make multiple
                        },
                        field_sf_websites = new [] { // TODO: collect websites
                            new {
                                uri = "https://ucdavis.edu",
                                title = "UC Davis"
                            },
                            new {
                                uri = "https://research.ucdavis.edu",
                                title = "Research at UCD"
                            }
                        }
                    },
                    relationships = new
                    {
                        field_sf_person_type = new
                        {
                            data = new
                            {
                                type = "taxonomy_term--sf_person_type",
                                id = "8238d79c-1105-4845-ae56-1919c8738249" // Staff type.  TODO: get from site settings or query dynamically?
                            }
                        },
                        field_sf_tags
                    }
                },
            };

            var serialized = JsonSerializer.Serialize(personData);

            var request = new HttpRequestMessage(method, resourceUrl);

            request.Content = new StringContent(serialized, Encoding.UTF8, "application/vnd.api+json");

            var response = await this.httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine(content);

            // TODO: make models and deserialze properly
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

            var id = json.data.id;

            if (sitePerson.PageUid.HasValue == false)
            {
                // if this is a new site entity, save the page uid for future updates
                sitePerson.PageUid = id;

                await this.dbContext.SaveChangesAsync();
            }

            return content;
        }
    }
}