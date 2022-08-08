using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace FacultyDirectory.Core.Services
{
    public interface ISiteFarmService
    {
        Task<string> PublishPerson(SitePerson person);
        IAsyncEnumerable<string> SyncTags(string[] tags);
        Task<string> PublishAudio(Stream audioContent, string fileName);
        Task<Stream> GetAudio(string audioId);
    }

    public class SiteFarmService : ISiteFarmService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IBiographyGenerationService biographyGenerationService;
        private readonly HttpClient httpClient;
        private readonly SiteFarmConfiguration config;

        public SiteFarmService(ApplicationDbContext dbContext, IBiographyGenerationService biographyGenerationService, HttpClient httpClient, IOptions<SiteFarmConfiguration> config)
        {
            this.dbContext = dbContext;
            this.biographyGenerationService = biographyGenerationService;
            this.httpClient = httpClient;
            this.config = config.Value;

            // TODO: determine if we want to set auth at the service level
            this.httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(String.Format("{0}:{1}", this.config.ApiUsername, this.config.ApiPassword))));
        }

        public async IAsyncEnumerable<string> SyncTags(string[] tags)
        {
            var baseUrl = $"{this.config.ApiBase}/taxonomy_term/sf_tags";

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

        public async Task<Stream> GetAudio(string audioId)
        {
            var mediaFileUrl = $"{this.config.ApiBase}/media/sf_audio_media_type/{audioId}/field_media_audio_file";

            dynamic mediaResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(await this.httpClient.GetStringAsync(mediaFileUrl));

            string fileRelativeUrl = mediaResponse.data.attributes.uri.url.ToString();

            // uri is a relative url, so we need to prepend the base url
            var fileUri = new UriBuilder(this.config.ApiBase);
            fileUri.Path = fileRelativeUrl;

            var fileResponse = await this.httpClient.GetStreamAsync(fileUri.Uri);
            
            return fileResponse;
        }

        /// <summary>
        /// Publishes the audio file to site farm.
        /// First, the blob is uploaded to the file section, a new audio media item is created and associated
        /// </summary>
        public async Task<string> PublishAudio(Stream audioContent, string fileName) {
            var mediaUrl = $"{this.config.ApiBase}/media/sf_audio_media_type/field_media_audio_file";

            var request = new HttpRequestMessage(HttpMethod.Post, mediaUrl);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.api+json"));

            request.Content = new StreamContent(audioContent);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            request.Content.Headers.Add("Content-Disposition", "file; filename=\"" + fileName + "\"");

            var response = await this.httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Error uploading audio file");
            }

            var content = await response.Content.ReadAsStringAsync();
            dynamic mediaJson = Newtonsoft.Json.JsonConvert.DeserializeObject(content);
            var mediaId = mediaJson.data.id.ToString();

            // create the audio media item
            var audioMediaItemUrl = $"{this.config.ApiBase}/media/sf_audio_media_type";

            var audioMediaItemData = new
            {
                data = new
                {
                    type = "media--sf_audio_media_type",
                    attributes = new
                    {
                        status = true,
                        name = fileName
                    },
                    relationships = new
                    {
                        field_media_audio_file = new
                        {
                            data = new
                            {
                                type = "file--file",
                                id = mediaId
                            }
                        }
                    }
                }
            };

            var serialized = JsonSerializer.Serialize(audioMediaItemData);
            var audioMediaItemResponse = await this.httpClient.PostAsync(audioMediaItemUrl, new StringContent(serialized, Encoding.UTF8, "application/vnd.api+json"));

            if (!audioMediaItemResponse.IsSuccessStatusCode)
            {
                throw new Exception("Error creating audio media item");
            }

            var audioMediaItemContent = await audioMediaItemResponse.Content.ReadAsStringAsync();
            dynamic audioMediaItemJson = Newtonsoft.Json.JsonConvert.DeserializeObject(audioMediaItemContent);

            return audioMediaItemJson.data.id;
        }

        // TODO: handle multiple sites?
        public async Task<string> PublishPerson(SitePerson sitePerson)
        {
            // TODO: get from site settings or query dynamically from site settings?
            var personTypes = new Dictionary<string, string> {
                { "faculty", "8338d79c-1105-4845-ae56-1919c8738249" },
                { "emeriti", "14f01b6f-09af-422a-9b57-739a4b306d17" },
                { "leadership", "eca6b30c-6c72-442b-af9a-dce57a0c8358" } 
            };

            // TODO: determine which site values to use for each property
            var drupalPerson = await this.biographyGenerationService.Generate(sitePerson);

            // Step 1: Sync tag taxonomy
            var tags = drupalPerson.Tags;

            var tagNodes = new List<object>();

            await foreach (var tagId in this.SyncTags(tags))
            {
                tagNodes.Add(new
                {
                    type = "taxonomy_term--sf_tags",
                    id = tagId
                });
            }

            // now we have our tag relationship
            var field_sf_tags = new
            {
                data = tagNodes
            };

            // pull in person type
            var field_sf_person_type = new
            {
                data = new
                {
                    type = "taxonomy_term--sf_person_type",
                    id = personTypes[sitePerson.Person.Classification]
                }
            };

            // relationships are different if the person is newly created or existing
            dynamic relationships;

            if (sitePerson.PageUid.HasValue) {
                // existing users get to keep their images
                relationships = new
                {
                    field_sf_person_type,
                    field_sf_tags
                };
            } else {
                // set to default image for people without an existing page
                relationships = new
                {
                    field_sf_person_type,
                    field_sf_primary_image = new {
                        data = new
                        {
                            type = "file--file",
                            id = "b1327c77-a018-4ede-b322-0bfd9f4ee79e", // TODO: get from site settings
                            meta = new {
                                title = "Profile Image"
                            }
                        }
                    },
                    field_sf_tags
                };
            }
    
            // Step 2: Compile and generate user page info
            var personData = new
            {
                data = new
                {
                    type = "node--sf_person",
                    id = sitePerson.PageUid,
                    attributes = new
                    {
                        title = drupalPerson.Title,
                        field_sf_first_name = drupalPerson.FirstName,
                        field_sf_last_name = drupalPerson.LastName,
                        field_sf_position_title = drupalPerson.Title,
                        field_sf_emails = drupalPerson.Emails,
                        field_sf_phone_numbers = drupalPerson.Phones,
                        field_sf_unit = drupalPerson.Departments,
                        field_sf_websites = drupalPerson.Websites.Select(w => new
                        {
                            uri = w.Uri,
                            title = w.Title
                        }),
                        body = new
                        {
                            value = drupalPerson.Bio,
                            format = "basic_html",
                            summary = "" // TODO: do we need summary?
                        }
                    },
                    relationships
                },
            };

            var serialized = JsonSerializer.Serialize(personData);

            // Console.WriteLine(serialized);

            // Step 3: form POST/PATCH depending on prior existance of user page
            var resourceUrl = $"{this.config.ApiBase}/node/sf_person";
            var method = HttpMethod.Post;

            if (sitePerson.PageUid.HasValue)
            {
                resourceUrl = resourceUrl + "/" + sitePerson.PageUid.Value;
                method = System.Net.Http.HttpMethod.Patch;
            }

            var request = new HttpRequestMessage(method, resourceUrl);

            request.Content = new StringContent(serialized, Encoding.UTF8, "application/vnd.api+json");

            var response = await this.httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            // Console.WriteLine(content);

            // TODO: make models and deserialze properly
            dynamic json = Newtonsoft.Json.JsonConvert.DeserializeObject(content);

            var id = json.data.id;

            if (sitePerson.PageUid.HasValue == false)
            {
                // if this is a new site entity, save the page uid for future updates
                sitePerson.PageUid = id;
            }

            sitePerson.LastSync = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            return content;
        }
    }
}
