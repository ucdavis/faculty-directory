using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using FacultyDirectory.Core.Data;

namespace FacultyDirectory.Core.Services
{
    public interface IScholarService
    {
        Task<string[]> FindScholarIds(string name);
        Task<SourceData> GetTagsAndPublicationsById(string id);
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