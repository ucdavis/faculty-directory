using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using Ietws;

namespace FacultyDirectory.Core.Services
{
    public interface IDirectoryPopulationService
    {
        Task<Person[]> ExtractCandidates();
    }

    // TODO: Maybe use IAM for web services
    public class DirectoryPopulationService : IDirectoryPopulationService
    {
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
            var result = await ietClient.PPSAssociations.Search(PPSAssociationsSearchField.bouOrgOId, "F80B657C9EF523A0E0340003BA8A560D");

            return result.ResponseData.Results;
        }

        private async Task<PeopleResult[]> GetFacultyPeople()
        {
            var result = await ietClient.PPSAssociations.Search<PeopleResults>(PPSAssociationsSearchField.bouOrgOId, "F80B657C9EF523A0E0340003BA8A560D", retType: "people");

            return result.ResponseData.Results;
        }

        public async Task<Person[]> ExtractCandidates()
        {
            var people = await GetFacultyPeople();
            var associations = await GetFacultyAssociations();

            // TODO: select out new db record version
            var validPeople = people.Where(p => p.IsFaculty).Where(person =>
            {
                // keep if person has at least one valid association
                var personAssociations = associations.Where(a => a.IamId == person.IamId);

                if (personAssociations == null || !personAssociations.Any())
                {
                    return false;
                }

                // yes they are faculty
                if (personAssociations.Any(a => a.titleCode == "001675"))
                {
                    return true;
                }

                return false;
            });

            return validPeople.Select(person =>
            {
                return new Person { IamId = person.IamId, Kerberos = person.ExternalId };
            }).ToArray();
        }
    }
}
