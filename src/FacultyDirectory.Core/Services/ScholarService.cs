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

        public async Task SyncForPerson(int personId)
        {
            // see if we already have a scholar record for this person
            // TODO: query source values out of enum or resources
            var source = await this.dbContext.PeopleSources.SingleOrDefaultAsync(s => s.Source == "scholar" && s.PersonId == personId);

            if (source == null || string.IsNullOrWhiteSpace(source.SourceKey))
            {
                // no source with key found
                if (source == null)
                {
                    // use the existing source if available, otherwise create a new one
                    source = new PersonSource { Source = "scholar", PersonId = personId };
                    this.dbContext.PeopleSources.Add(source);
                }

                // here we attempt to find source ID through search
                var personName = await this.dbContext.People.Where(p => p.Id == personId).Select(p => p.FullName).SingleOrDefaultAsync();
                var matchingIds = await FindScholarIds(personName);

                if (matchingIds.Length == 1)
                {
                    // if we have exactly one match, use that
                    source.SourceKey = matchingIds[0];
                }
                else
                {
                    // else we have no match, just use the empty record until it can be manually updated
                    if (source.Id == default(int))
                    {
                        this.dbContext.Add(source);
                    }

                    source.LastUpdate = DateTime.UtcNow;

                    await this.dbContext.SaveChangesAsync();

                    return;
                }
            }

            // go grab updated info
            var sourceData = await GetTagsAndPublicationsById(source.SourceKey);

            source.Data = JsonConvert.SerializeObject(sourceData);
            source.HasKeywords = sourceData.Tags.Any();
            source.HasPubs = sourceData.Publications.Any();
            source.LastUpdate = DateTime.UtcNow;

            // save changes to source object
            await this.dbContext.SaveChangesAsync();
        }

        private HttpRequestMessage ConfigureRequest(HttpMethod method, string url) {
            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("user-agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_2) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.130 Safari/537.36");
            request.Headers.Add("referer","https://scholar.google.com/");
            request.Headers.Add("accept-language", "en-US,en;q=0.9");
            request.Headers.Add("accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
            return request;
        }

        public async Task<SourceData> GetTagsAndPublicationsById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentNullException();
            }

            var siteUrl = $"https://scholar.google.com/citations?hl=en&user={id}";

            var request = await this.httpClient.SendAsync(ConfigureRequest(HttpMethod.Get, siteUrl));

            IHtmlDocument document;

            using (var responseStream = await request.Content.ReadAsStreamAsync())
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(responseStream);
            }

            EnsureValidResponse(document, siteUrl);

            var tags = document.All.Where(m => m.LocalName == "a" &&
                m.ParentElement.Id == "gsc_prf_int");

            var publications = document.All.Where(m => m.LocalName == "a" &&
                m.ParentElement.ClassName == "gsc_a_t");

            // pubs are rows of the gsc_a_t table
            var pubs = document.All.Where(m => m.ClassName == "gsc_a_t" && m.ParentElement.ClassName == "gsc_a_tr");

            var sourcePublications = new List<SourcePublication>();

            if (pubs.Any()) {
                var firstPub = pubs.First();
                var greyEls = firstPub.Children.Where(c => c.LocalName == "div" && c.ClassName == "gs_gray");
                var firstSub = greyEls.First().TextContent;

                foreach (var pub in pubs)
                {
                    var header = pub.Children.Where(c => c.LocalName == "a").First();
                    var subInfo = pub.Children.Where(c => c.LocalName == "div" && c.ClassName == "gs_gray").ToArray();

                    sourcePublications.Add(new SourcePublication
                    {
                        Title = header.TextContent,
                        Url = header.GetAttribute("data-href"),
                        Authors = subInfo[0].TextContent,
                        ShortDetail = subInfo[1].TextContent
                    });
                }
            }

            var data = new SourceData
            {
                Tags = tags.Select(t => t.TextContent).ToArray(),
                Publications = sourcePublications.ToArray()
            };

            return data;
        }

        public async Task<string[]> FindScholarIds(string name)
        {
            var siteUrl = "https://scholar.google.com/citations?hl=en&view_op=search_authors&mauthors=" + name;

            var request = await this.httpClient.SendAsync(ConfigureRequest(HttpMethod.Get, siteUrl));

            IHtmlDocument document;

            using (var responseStream = await request.Content.ReadAsStreamAsync())
            {
                var parser = new HtmlParser();
                document = parser.ParseDocument(responseStream);
            }

            EnsureValidResponse(document, siteUrl);

            //  Query Profiles
            var profiles = document.All.Where(m => m.ClassName == "gs_ai gs_scl gs_ai_chpr" &&
                m.ParentElement.ClassName == "gsc_1usr");

            var foundScholarIds = new List<string>();

            foreach (var item in profiles)
            {
                // for each found profile, determine if they are a ucd affiliate by looking at school and email
                var isUcdAffiliate = false;

                var university = item.GetElementsByClassName("gs_ai_aff");

                var email = item.GetElementsByClassName("gs_ai_eml");

                foreach (var k in university)
                {
                    if (string.Equals(k.TextContent, "University of California, Davis", StringComparison.OrdinalIgnoreCase))
                    {
                        isUcdAffiliate = true;
                    }
                }

                foreach (var e in email)
                {
                    if (string.Equals(e.TextContent, "Verified email at ucdavis.edu", StringComparison.OrdinalIgnoreCase))
                    {
                        isUcdAffiliate = true;
                    }
                }

                // if we found a ucd school or email, then extract their scholar id and add to the list
                if (isUcdAffiliate)
                {
                    // TODO: maybe use regex as more reliable method of getting out id string?
                    var user = item.GetElementsByClassName("gs_ai_pho");
                    var userId = user.Single().GetAttribute("href");
                    var startInd = userId.LastIndexOf("user") + 5;
                    var lastInd = userId.Length - startInd;
                    var id = userId.Substring(startInd, lastInd);
                    foundScholarIds.Add(id);
                }
            }

            return foundScholarIds.ToArray();
        }

        private void EnsureValidResponse(IHtmlDocument document, string siteUrl) {
            // Get the main body to determine if we've properly loaded the document
            var body = document.GetElementById("gs_bdy_ccl");

            if (body == null)
            {
                throw new ApplicationException($"{siteUrl} could not be loaded");
            }
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
        public string Authors { get; set; }
        public string ShortDetail { get; set; }
    }
}