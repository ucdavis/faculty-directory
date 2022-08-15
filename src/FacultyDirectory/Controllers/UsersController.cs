using System.Security.Claims;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using FacultyDirectory.Helpers;

namespace FacultyDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        public UsersController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet("name")]
        public ActionResult Name() {
            // Returns the user's name
            var currentUser = this.User;
            var firstName = currentUser.FindFirst(ClaimTypes.GivenName)?.Value;
            var lastName = currentUser.FindFirst(ClaimTypes.Surname)?.Value;
            var userData = new {name = firstName + " " + lastName};

            return Json(userData);
        }

        [HttpGet]
        public async Task<ActionResult> GetUsers() {
            return Ok(await dbContext.Users.ToArrayAsync());
        }

        [HttpPost]
        public async Task<ActionResult> AddUser(User userData) {
            // Creates a user
            var targetUser = await dbContext.Users.SingleOrDefaultAsync(u => u.Username == userData.Username);

            if (targetUser != null) {
                return BadRequest();
            }

            var user = new User 
            { 
                Username = userData.Username 
            };

            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();

            return Json(user);
        }

        [HttpDelete("{id}")]  
        public async Task<ActionResult> DeleteUser(int id) {  
            // Deletes a user
            var user = await dbContext.Users.FindAsync(id);

            if (user == null) {
                return NotFound();
            }

            dbContext.Users.Remove(user);
            await dbContext.SaveChangesAsync();
            var msg = new {msg = "Deleted a User!"};

            return Json(msg);
        }

        [HttpGet("emulate")]
        public async Task<IActionResult> Emulate(string kerberos, string iamId) {
            if (string.IsNullOrEmpty(kerberos) || string.IsNullOrEmpty(iamId)) {
                return BadRequest();
            }

            // Emulates a user
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, kerberos),
                new Claim(ClaimTypes.Name, kerberos),
                new Claim(ClaimTypes.GivenName, "First"),
                new Claim(ClaimTypes.Surname, "Last"),
                new Claim("name", "First Last"),
                new Claim(ClaimTypes.Email, "fLast@email.com"),
                new Claim(AdminOrSelfAuthorizationHandler.IamIdClaimType, iamId),
            }, CookieAuthenticationDefaults.AuthenticationScheme);
            
            // kill old login
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // create new login
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Ok("Emulated");
        }
        
        [HttpGet("endEmulate")]
        public async Task<IActionResult> Logout() {
            // Logs out the user
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok("Logged out");
        }
    }
}