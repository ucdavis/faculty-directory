using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FacultyDirectory.Controllers;

[Route("[controller]")]
public class AccountController : Controller
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AccountController(IHttpContextAccessor _httpContextAccessor)
    {
        this.httpContextAccessor = _httpContextAccessor;
    }

    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)] // trigger authentication
    [HttpGet("login")]
    public IActionResult Login(string returnUrl)
    {
        // redirect to return url if it exists, otherwise /
        return Redirect(returnUrl ?? "/");
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        var claims = this.httpContextAccessor.HttpContext?.User.Claims;

        if (claims == null || !claims.Any())
        {
            return Unauthorized();
        }

        return Ok(claims.ToDictionary(c => c.Type, c => c.Value));
    }
}