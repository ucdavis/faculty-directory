using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FacultyDirectory.Core.Services
{
    public interface IBiographyGenerationService
    {
        Task<DrupalPerson> Generate(SitePerson sitePerson);

        DrupalPerson Generate(SitePerson sitePerson, PersonSource[] sources);
    }

    public class BiographyGenerationService : IBiographyGenerationService
    {
        private readonly ApplicationDbContext dbContext;

        public BiographyGenerationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public DrupalPerson Generate(SitePerson sitePerson, PersonSource[] sources) {
            var departmentValues = sitePerson.Departments ?? sitePerson.Person.Departments;

            var publications = string.IsNullOrWhiteSpace(sitePerson.Bio) ? GetPublications(sources) : sitePerson.Bio;

            var person = new DrupalPerson
            {
                FirstName = sitePerson.FirstName ?? sitePerson.Person.FirstName,
                LastName = sitePerson.LastName ?? sitePerson.Person.LastName,
                Title = sitePerson.Title ?? sitePerson.Person.Title,
                Emails = GetEmails(sitePerson, sources),
                Phones = GetPhones(sitePerson, sources),
                Departments = departmentValues?.Split("|").ToArray(), // TODO: should it be null or empty array if we don't have any?
                Tags = GetTags(sitePerson, sources),
                Websites = GetWebsites(sitePerson, sources),
                Pronunciation = GetPronunciation(sitePerson),
                Publications = publications
            };

            return person;
        }


        public async Task<DrupalPerson> Generate(SitePerson sitePerson)
        {
            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).AsNoTracking().ToArrayAsync();

            return Generate(sitePerson, sources);
        }

        private string GetPronunciation(SitePerson sitePerson) {
            if (sitePerson.PronunciationUid != null) {
                return $"<h3>Name Pronunciation:</h3><drupal-media data-align=\"\" data-entity-type=\"media\" data-entity-uuid=\"{sitePerson.PronunciationUid.ToString()}\"></drupal-media>";
            }

            return string.Empty;
        }

        private DrupalWebsite[] GetWebsites(SitePerson sitePerson, PersonSource[] sources) {
            if (!string.IsNullOrWhiteSpace(sitePerson.Websites))
            {
                // site person entry overrides all
                var websites = sitePerson.Websites.Split('|');

                var websiteList = new List<DrupalWebsite>();

                for (int i = 0; i < websites.Length; i += 2)
                {
                    websiteList.Add(new DrupalWebsite { Uri = websites[i], Title = websites[i+1]});
                }
                
                return websiteList.ToArray();
            }

            // TODO: once we have websites from a person source, check and return here
            
            return new DrupalWebsite[0];
        }

        private string[] GetEmails(SitePerson sitePerson, PersonSource[] sources) {
            if (!string.IsNullOrWhiteSpace(sitePerson.Emails)) {
                // site person entry overrides all
                return sitePerson.Emails.Split('|');
            } else if (!string.IsNullOrWhiteSpace(sitePerson.Person.Email)) {
                return new[] { sitePerson.Person.Email };
            }

            // TODO: pull from sources
            return new string[0];
        }

        private string[] GetPhones(SitePerson sitePerson, PersonSource[] sources)
        {
            if (!string.IsNullOrWhiteSpace(sitePerson.Phones))
            {
                // site person entry overrides all
                return sitePerson.Phones.Split('|');
            }
            else if (!string.IsNullOrWhiteSpace(sitePerson.Person.Phone))
            {
                return new[] { sitePerson.Person.Phone };
            }

            // TODO: pull from sources
            return new string[0];
        }

        private string[] GetTags(SitePerson sitePerson, PersonSource[] sources) {
            if (!string.IsNullOrWhiteSpace(sitePerson.Tags)) {
                // site person entry overrides all
                return sitePerson.Tags.Split('|');
            }

            return GetSourceTags(sources);
        }

        private string[] GetSourceTags(PersonSource[] sources)
        {
            foreach (var source in sources)
            {
                if (source.Data == null) {
                    // skip to the next source if we don't have any data
                    continue;
                }

                var data = JsonConvert.DeserializeObject<SourceData>(source.Data);

                // TODO: this will return first source tags.
                // Once we get more sources, we need to determine a ranking in case there are multiple tags
                if (data.Tags != null && data.Tags.Any())
                {
                    return data.Tags.Select(t => SanitizeTag(t)).ToArray();
                }
            }

            return new string[0];
        }

        private string SanitizeTag(string tag) {
            // string out invalid characters and lowercase result
            if (string.IsNullOrWhiteSpace(tag)) {
                return tag;
            }

            return tag.Replace("&", "and").ToLower();
        }

        // TODO: maybe instead of deserializing multiple times we just do once up-front and call these methods with that data?
        private string GetPublications(PersonSource[] sources)
        {
            foreach (var source in sources)
            {
                if (source.Data == null)
                {
                    // skip to the next source if we don't have any data
                    continue;
                }

                var data = JsonConvert.DeserializeObject<SourceData>(source.Data);

                // TODO: this will return first source tags.
                // Once we get more sources, we need to determine a ranking in case there are multiple tags
                if (data.Publications != null && data.Publications.Any())
                {
                    var sb = new StringBuilder();

                    sb.Append("<h3>Publications:</h3>");

                    foreach (var pub in data.Publications.Take(5))
                    {
                        sb.Append("<strong>");
                        sb.Append(pub.Title);
                        sb.Append("</strong>");
                        sb.Append("<br/>");
                        sb.Append(pub.Authors);
                        sb.Append("<br/>");
                        sb.Append(pub.ShortDetail);
                        sb.Append("<br/><br/>");
                    }

                    return sb.ToString();
                }
            }

            return string.Empty;
        }
    }
}