using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Core.Services
{
    public interface IBiographyGenerationService
    {
        Task<DrupalPerson> Generate(SitePerson sitePerson);
    }

    public class BiographyGenerationService {
        private readonly ApplicationDbContext dbContext;

        public BiographyGenerationService(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<DrupalPerson> Generate(SitePerson sitePerson) {
            // Get external source info for this person
            var sources = await this.dbContext.PeopleSources.Where(s => s.PersonId == sitePerson.PersonId).AsNoTracking().ToArrayAsync();

            var person = new DrupalPerson {
                FirstName = sitePerson.FirstName ?? sitePerson.Person.FirstName,
                LastName = sitePerson.LastName ?? sitePerson.Person.LastName,
                Title = sitePerson.Title ?? sitePerson.Person.Title,
                Emails = new [] { "example@ucdavis.edu" }
            };

            return person;
        }
    }
}