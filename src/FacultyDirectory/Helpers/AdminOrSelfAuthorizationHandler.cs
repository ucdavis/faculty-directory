using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Helpers
{
    public class AdminOrSelfAuthorizationHandler : AuthorizationHandler<AdminOrSelfRequirement>
    {
        public static readonly string IamIdClaimType = "ucdPersonIAMID";

        private readonly ApplicationDbContext dbContext;

        public AdminOrSelfAuthorizationHandler(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       AdminOrSelfRequirement requirement)
        {
            var username = context.User.Identity.Name;
            var userExists = await dbContext.Users.AnyAsync(u => u.Username == username);
    
            if (userExists)
            {
                context.Succeed(requirement);
                return;
            }

            // user is not admin, see if they have a sitePeople record
            string iamId = context.User.Claims.SingleOrDefault(c => c.Type == IamIdClaimType)?.Value;

            if (iamId == null)
            {
                // should never be null, but CAS is unreliable
                var sitePerson = await dbContext.SitePeople.FirstOrDefaultAsync(sp => sp.Person.IamId == iamId);

                if (sitePerson != null)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
