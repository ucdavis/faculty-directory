using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FacultyDirectory.Core.Services
{
    public interface IScholarService
    {
        Task<string[]> FindScholarIds(string name);
        Task<SourceData> GetTagsAndPublicationsById(string id);
        Task SyncForPerson(int personId);
    }

    public class ScholarService : IScholarService
    {
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient httpClient;

        public ScholarService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClient;
        }

        public async Task SyncForPerson(int personId) {
            // see if we already have a scholar record for this person
            // TODO: query source values out of enum or resources
            var source = await this.dbContext.PeopleSources.SingleOrDefaultAsync(s => s.Source == "scholar" && s.PersonId == personId);

            var sourceId = string.Empty;

            if (source != null) {
                sourceId = source.SourceKey;
            } else {
                // TODO: here we should attempt to find source ID, either through search or maybe based on manual sitePerson attributes
                var personName = await this.dbContext.People.Where(p => p.Id == personId).Select(p => p.FullName).SingleOrDefaultAsync();
                var matchingIds = await FindScholarIds(personName);

                if (matchingIds.Length == 1) {
                    // if we have exactly one match, use that
                    sourceId = matchingIds[0];
                    source = new PersonSource { Source = "scholar", SourceKey = sourceId };
                    source.PersonId = personId;
                } else {
                    // else we have no match, just add an empty record until it can be manually updated
                    source = new PersonSource { Source = "scholar" };

                    this.dbContext.PeopleSources.Add(source);
                    await this.dbContext.SaveChangesAsync();

                    return;
                }
            }

            var sourceData = await GetTagsAndPublicationsById(sourceId);

            source.Data = JsonConvert.SerializeObject(sourceData);
            source.HasKeywords = sourceData.Tags.Any();
            source.HasPubs = sourceData.Publications.Any();
            source.LastUpdate = DateTime.UtcNow;

            // save changes to source object
            this.dbContext.PeopleSources.Add(source);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task<SourceData> GetTagsAndPublicationsById(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) {
                throw new ArgumentNullException();
            }

            var siteUrl = $"https://scholar.google.com/citations?hl=en&user={id}";

            var request = await this.httpClient.GetAsync(siteUrl);

            IHtmlDocument document;

            using (var responseStream = await request.Content.ReadAsStreamAsync())
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(responseStream);
            }

            var tags = document.All.Where(m => m.LocalName == "a" &&
                m.ParentElement.Id == "gsc_prf_int");

            var publications = document.All.Where(m => m.LocalName == "a" &&
                m.ParentElement.ClassName == "gsc_a_t");

            var data = new SourceData
            {
                Tags = tags.Select(t => t.TextContent).ToArray(),
                Publications = publications.Select(p => new SourcePublication { Title = p.TextContent, Url = p.GetAttribute("data-href") }).ToArray()
            };

            return data;
        }

        public async Task<string[]> FindScholarIds(string name) {
            var siteUrl = "https://scholar.google.com/citations?hl=en&view_op=search_authors&mauthors=" + name;

            var request = await this.httpClient.GetAsync(siteUrl);

            IHtmlDocument document;

            using (var responseStream = await request.Content.ReadAsStreamAsync())
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(responseStream);
            }

            //  Query Profiles
            var profiles = document.All.Where(m => m.ClassName == "gs_ai gs_scl gs_ai_chpr" &&
                m.ParentElement.ClassName == "gsc_1usr");

            var foundScholarIds = new List<string>();

            foreach (var item in profiles)
            {
                var university = item.GetElementsByClassName("gs_ai_aff");
                foreach (var k in university)
                {
                    if (k.TextContent == "University of California, Davis")
                    {
                        // TODO: maybe use regex as more reliable method of getting out id string?
                        var user = item.GetElementsByClassName("gs_ai_pho");
                        var userId = user.Single().GetAttribute("href");
                        var startInd = userId.LastIndexOf("user") + 5;
                        var lastInd = userId.Length - startInd;
                        var id = userId.Substring(startInd, lastInd);
                        foundScholarIds.ToList();
                    }
                }
            }

            return foundScholarIds.ToArray();
        }
    }

    public class SourceData
    {
        public string[] Tags { get; set; }

        public SourcePublication[] Publications { get; set; }
    }

    public class SourcePublication
    {
        public string Title { get; set; }
        public string Url { get; set; }
    }
}