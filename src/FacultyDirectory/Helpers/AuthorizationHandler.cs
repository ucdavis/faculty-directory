using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Helpers
{
    public class AuthorizationHandler : AuthorizationHandler<AdminRequirement>
    {
        private readonly ApplicationDbContext dbContext;

        public AuthorizationHandler(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                       AdminRequirement requirement)
        {
            // steps
            // Get current username from the current user with context
            // check in database if the current username exists
            // if it does return a success
            var username = context.User.FindFirst(ClaimTypes.Name)?.Value;
            var user = await dbContext.Users.Where(u => u.Username == username).FirstOrDefaultAsync();

            if (user != null) {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
