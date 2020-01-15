using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FacultyDirectory.Core.Services
{
    public interface IBiographyGenerationService
    {
        Task<DrupalPerson> Generate(SitePerson sitePerson);
    }

    public class BiographyGenerationService : IBiographyGenerationService
    {
        private readonly ApplicationDbContext dbContext;

        public BiographyGenerationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<DrupalPerson> Generate(SitePerson sitePerson)
        {
            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).AsNoTracking().ToArrayAsync();

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
                Emails = new[] { "example@ucdavis.edu" },
                Phones = new[] { "555-5555" },
                Departments = departmentValues?.Split("|"), // TODO: should it be null or empty array if we don't have any?
                Tags = tags,
                Websites = websites,
                Bio = bio
            };

            return person;
        }

        private string[] GetSourceTags(PersonSource[] sources)
        {
            foreach (var source in sources)
            {
                var data = JsonConvert.DeserializeObject<SourceData>(source.Data);

                // TODO: this will return first source tags.
                // Once we get more sources, we need to determine a ranking in case there are multiple tags
                if (data.Tags != null && data.Tags.Any())
                {
                    return data.Tags;
                }
            }

            return new string[0];
        }

        // TODO: maybe instead of deserializing multiple times we just do once up-front and call these methods with that data?
        private string GetBiography(PersonSource[] sources)
        {
            foreach (var source in sources)
            {
                var data = JsonConvert.DeserializeObject<SourceData>(source.Data);

                // TODO: this will return first source tags.
                // Once we get more sources, we need to determine a ranking in case there are multiple tags
                if (data.Publications != null && data.Publications.Any())
                {
                    // TODO: generate actual bio out of first N titles
                    // env newline or maybe <br/>?
                    return data.Publications.First().Title;
                }
            }

            return string.Empty;
        }
    }
}