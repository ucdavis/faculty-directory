using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Helpers
{
    public class AdminOrSelfAuthorizationHandler : AuthorizationHandler<AdminOrSelfRequirement>
    {
        public static readonly string IamIdClaimType = "ucdPersonIAMID";

        private readonly ApplicationDbContext dbContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        public AdminOrSelfAuthorizationHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.dbContext = dbContext;
            this.httpContextAccessor = httpContextAccessor;
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

            // get personId from the route
            int.TryParse(httpContextAccessor.HttpContext.Request.RouteValues["personId"] as string, out int personId);

            if (iamId != null || personId != 0)
            {
                var isSitePerson = await dbContext.SitePeople.AnyAsync(sp => sp.Person.IamId == iamId && sp.Person.Id == personId);

                if (isSitePerson)
                {
                    context.Succeed(requirement);
                    return;
                }
            }
        }
    }
}
