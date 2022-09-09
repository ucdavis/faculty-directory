using FacultyDirectory.Core.Domain;
using Ietws;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ietws;
using FacultyDirectory.Core.Models;

namespace FacultyDirectory.Core.Services
{
    public interface IIdentityService
    {
        Task<String> GetByKerberos(string kerb);
    }
    public class IdentityService : IIdentityService
    {
        private readonly AuthSettings _authSettings;

        public IdentityService( IOptions<AuthSettings> authSettings)
        {
            _authSettings = authSettings.Value;
        }
        

        public async Task<String> GetByKerberos(string kerb)
        {
            var clientws = new IetClient(_authSettings.IamKey);
            var ucdKerbResult = await clientws.Kerberos.Search(KerberosSearchField.userId, kerb);

            if (ucdKerbResult.ResponseData.Results.Length == 0)
            {
                return null;
            }

            if (ucdKerbResult.ResponseData.Results.Length != 1)
            {
                var iamIds = ucdKerbResult.ResponseData.Results.Select(a => a.IamId).Distinct().ToArray();
                var userIDs = ucdKerbResult.ResponseData.Results.Select(a => a.UserId).Distinct().ToArray();
                if (iamIds.Length != 1 && userIDs.Length != 1)
                {
                    throw new Exception($"IAM issue with non unique values for kerbs: {string.Join(',', userIDs)} IAM: {string.Join(',', iamIds)}");
                }
            }

            var ucdKerbPerson = ucdKerbResult.ResponseData.Results.First();


            return ucdKerbPerson.IamId;
        }

    }
}
