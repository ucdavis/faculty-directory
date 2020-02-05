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

            // TODO: add tags to sitePerson and check there too
            var tags = GetSourceTags(sources);

            var bio = string.IsNullOrWhiteSpace(sitePerson.Bio) ? GetBiography(sources) : sitePerson.Bio;

            var websites = new DrupalWebsite[] { new DrupalWebsite { Uri = "https://ucdavis.edu", Title = "UC Davis" } };

            var person = new DrupalPerson
            {
                FirstName = sitePerson.FirstName ?? sitePerson.Person.FirstName,
                LastName = sitePerson.LastName ?? sitePerson.Person.LastName,
                Title = sitePerson.Title ?? sitePerson.Person.Title,
                Emails = GetEmails(sitePerson, sources),
                Phones = GetPhones(sitePerson, sources),
                Departments = departmentValues?.Split("|").ToArray(), // TODO: should it be null or empty array if we don't have any?
                Tags = tags,
                Websites = websites,
                Bio = bio
            };

            return person;
        }


        public async Task<DrupalPerson> Generate(SitePerson sitePerson)
        {
            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).AsNoTracking().ToArrayAsync();

            return Generate(sitePerson, sources);
        }

        private string[] GetEmails(SitePerson sitePerson, PersonSource[] sources) {
            if (!string.IsNullOrWhiteSpace(sitePerson.Emails)) {
                // site person entry overrides all
                return new[] { sitePerson.Emails };
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
                return new[] { sitePerson.Phones };
            }
            else if (!string.IsNullOrWhiteSpace(sitePerson.Person.Phone))
            {
                return new[] { sitePerson.Person.Phone };
            }

            // TODO: pull from sources
            return new string[0];
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
                    return data.Tags.Select(t => t.ToLower()).ToArray();
                }
            }

            return new string[0];
        }

        // TODO: maybe instead of deserializing multiple times we just do once up-front and call these methods with that data?
        private string GetBiography(PersonSource[] sources)
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
                        sb.Append(pub.Title);
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