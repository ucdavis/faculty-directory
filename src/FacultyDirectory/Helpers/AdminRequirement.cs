
using Microsoft.AspNetCore.Authorization;

namespace FacultyDirectory.Helpers
{
    public class AdminRequirement : IAuthorizationRequirement
    {
        public readonly bool IsAdmin;

        public AdminRequirement(bool isAdmin)
        {
            IsAdmin = isAdmin;
        }
    }
}