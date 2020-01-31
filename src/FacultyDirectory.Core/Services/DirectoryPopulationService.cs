using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Models;
using FacultyDirectory.Core.Resources;
using Ietws;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
        // TODO: get from settings
        private const string CaesOrgOId = "F80B657C9EF523A0E0340003BA8A560D";
        private readonly ApplicationDbContext dbContext;
        private readonly HttpClient httpClient;
        private readonly DirectoryConfiguration config;
        private readonly string peopleLookupKey;
        private readonly string PeopleLookupBaseUrl = "https://who.ucdavis.edu";

        public DirectoryPopulationService(ApplicationDbContext dbContext, HttpClient httpClient, IOptions<DirectoryConfiguration> config)
        {
            this.dbContext = dbContext;
            this.httpClient = httpClient;
            this.config = config.Value;

            // TODO: get key from settings
            this.peopleLookupKey = this.config.ApiKey;
        }

        // Go to the campus directory and return the current list of faculty
        private async Task<PPSAssociationsResult[]> GetFacultyAssociations()
        {
            var queryUrl = $"{PeopleLookupBaseUrl}/api/Iamws/PPSAssociation?key={this.peopleLookupKey}&field=bouOrgOId&fieldValue={CaesOrgOId}&retType=default";

            // TODO: this whole section can be extracted
            using (var stream = await this.httpClient.GetStreamAsync(queryUrl))
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var result = new JsonSerializer().Deserialize<PPSAssociationResults>(jsonTextReader);
                    return result.ResponseData.Results;
                }
            }
        }

        private async Task<PeopleResult[]> GetFacultyPeople()
        {
            var queryUrl = $"{PeopleLookupBaseUrl}/api/Iamws/PPSAssociation?key={this.peopleLookupKey}&field=bouOrgOId&fieldValue={CaesOrgOId}&retType=people";

            using (var stream = await this.httpClient.GetStreamAsync(queryUrl))
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var result = new JsonSerializer().Deserialize<PeopleResults>(jsonTextReader);
                    return result.ResponseData.Results;
                }
            }
        }

        private async Task<ContactResult[]> GetContactInfo(string iamId) {
            var queryUrl = $"{PeopleLookupBaseUrl}/api/Iamws/Contact/{iamId}?key={this.peopleLookupKey}";

            using (var stream = await this.httpClient.GetStreamAsync(queryUrl))
            {
                using (var sr = new StreamReader(stream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var result = new JsonSerializer().Deserialize<ContactResults>(jsonTextReader);
                    return result.ResponseData.Results;
                }
            }
        }

        public async Task MergeFaculty(Person[] people)
        {
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

                    // TODO: could improve using merge statement directly in DB
                    // https://stackoverflow.com/questions/23916453/how-can-i-use-use-entity-framework-to-do-a-merge-when-i-dont-know-if-the-record
                    dbPersonRecord.FirstName = person.FirstName;
                    dbPersonRecord.LastName = person.LastName;
                    dbPersonRecord.FullName = person.FullName;
                    dbPersonRecord.Email = person.Email;
                    dbPersonRecord.Phone = person.Phone;
                    dbPersonRecord.Title = person.Title;
                    dbPersonRecord.Departments = person.Departments;
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
                if (personAssociations.Any(a => Titles.Faculty.Contains(a.titleCode)))
                {
                    return true;
                }

                // yes they are emeriti
                if (personAssociations.Any(a => Titles.Emeriti.Contains(a.titleCode)))
                {
                    return true;
                }                

                return false;
            });

            var validCandidates = new List<Person>();

            foreach (var person in validPeople)
            {
                var contactInfo = await GetContactInfo(person.IamId);

                var personAssociations = associations.Where(a => a.IamId == person.IamId);

                var firstAssociation = personAssociations.FirstOrDefault();

                // TODO: get title and deptment names from lookup values
                validCandidates.Add(new Person
                {
                    IamId = person.IamId,
                    Kerberos = person.ExternalId,
                    FirstName = person.DFirstName ?? person.OFirstName,
                    LastName = person.DLastName ?? person.OLastName,
                    FullName = person.DFullName ?? person.OFullName,
                    Email = contactInfo.FirstOrDefault()?.Email,
                    Phone = contactInfo.FirstOrDefault()?.WorkPhone,
                    Title = firstAssociation?.titleDisplayName,
                    Departments = string.Join("|", personAssociations.Select(pa => pa.deptDisplayName).Distinct())
                });
            }

            return validCandidates.ToArray();
        }
    }
}
