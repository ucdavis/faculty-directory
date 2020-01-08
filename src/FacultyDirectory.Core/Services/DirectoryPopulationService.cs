using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Resources;
using Ietws;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Core.Services
{
    public interface IDirectoryPopulationService
    {
        Task<Person[]> ExtractCandidates();
        Task MergeFaculty(Person[] people);
    }

    // TODO: Maybe use IAM for web services
    public class DirectoryPopulationService : IDirectoryPopulationService
    {
        private const string CaesOrgOId = "F80B657C9EF523A0E0340003BA8A560D";
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient httpClient;
        private readonly IetClient ietClient;

        public DirectoryPopulationService(ApplicationDbContext dbContext, HttpClient httpClient)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClient;

            // TODO: get key from settings
            var key = "";
            this.ietClient = new IetClient(this.httpClient, key);
        }

        // Go to the campus directory and return the current list of faculty
        private async Task<PPSAssociationsResult[]> GetFacultyAssociations()
        {
            var result = await ietClient.PPSAssociations.Search(PPSAssociationsSearchField.bouOrgOId, CaesOrgOId);

            return result.ResponseData.Results;
        }

        private async Task<PeopleResult[]> GetFacultyPeople()
        {
            var result = await ietClient.PPSAssociations.Search<PeopleResults>(PPSAssociationsSearchField.bouOrgOId, CaesOrgOId, retType: "people");

            return result.ResponseData.Results;
        }

        public async Task MergeFaculty(Person[] people)
        {
            // TODO: could improve using merge statement directly in DB
            // https://stackoverflow.com/questions/23916453/how-can-i-use-use-entity-framework-to-do-a-merge-when-i-dont-know-if-the-record

            // for now, we get a list of all faculty which match our IAMIDs
            var peopleIamIds = people.Select(p => p.IamId).ToArray();
            var dbFaculty = await dbContext.People.Where(dbf => peopleIamIds.Contains(dbf.IamId)).AsNoTracking().ToListAsync();

            foreach (var person in people)
            {
                var dbPersonRecord = dbFaculty.SingleOrDefault(dbf => dbf.IamId == person.IamId);

                if (dbPersonRecord != null)
                {
                    // update existing person
                    dbContext.People.Attach(dbPersonRecord);

                    dbPersonRecord.FirstName = person.FirstName;
                    dbPersonRecord.LastName = person.LastName;
                    dbPersonRecord.FullName = person.FullName;
                }
                else
                {
                    // new person
                    await dbContext.People.AddAsync(person);
                }
            }

            await dbContext.SaveChangesAsync();
        }

        public async Task<Person[]> ExtractCandidates()
        {
            var people = await GetFacultyPeople();
            var associations = await GetFacultyAssociations();

            // TODO: select out new db record version directly instead
            var validPeople = people.Where(p => p.IsFaculty).Where(person =>
            {
                // keep if person has at least one valid association
                var personAssociations = associations.Where(a => a.IamId == person.IamId);

                if (personAssociations == null || !personAssociations.Any())
                {
                    return false;
                }

                // yes they are faculty
                if (personAssociations.Any(a => TitleCodes.Faculty.Contains(a.titleCode)))
                {
                    return true;
                }

                // yes they are emeriti
                if (personAssociations.Any(a => TitleCodes.Emeriti.Contains(a.titleCode)))
                {
                    return true;
                }                

                return false;
            });

            return validPeople.Select(person =>
            {
                var personAssociations = associations.Where(a => a.IamId == person.IamId);

                var firstAssociation = personAssociations.FirstOrDefault();

                return new Person
                {
                    IamId = person.IamId,
                    Kerberos = person.ExternalId,
                    FirstName = person.DFirstName ?? person.OFirstName,
                    LastName = person.DLastName ?? person.OLastName,
                    FullName = person.DFullName ?? person.OFullName,
                    Email = "example@ucdavis.edu",
                    Phone = "555-5555",
                    Title = firstAssociation?.titleDisplayName,
                    Departments = string.Join("|", personAssociations.Select(pa => pa.deptDisplayName))
                };
            }).ToArray();
        }
    }
}
