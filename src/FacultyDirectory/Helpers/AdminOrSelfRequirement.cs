using Microsoft.AspNetCore.Authorization;

namespace FacultyDirectory.Helpers
{
    public class AdminOrSelfRequirement : IAuthorizationRequirement
    {
    }
}