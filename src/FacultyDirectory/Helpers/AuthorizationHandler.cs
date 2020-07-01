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
            var username = context.User.Identity.Name;
            var userExists = await dbContext.Users.AnyAsync(u => u.Username == username);

            if (userExists)
            {
                context.Succeed(requirement);
                return;
            }
        }
    }
}
