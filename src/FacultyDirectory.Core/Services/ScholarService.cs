using System;
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