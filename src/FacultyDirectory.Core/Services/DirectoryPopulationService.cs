using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using Ietws;

namespace FacultyDirectory.Core.Services
{
    public interface IDirectoryPopulationService
    {
        Task<PPSAssociationResults> GetFacultyAssociations();
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
        public async Task<PPSAssociationResults> GetFacultyAssociations()
        {
            return await ietClient.PPSAssociations.Search(PPSAssociationsSearchField.iamId, "1000020214");
        }
    }
}
