using System;
using System.Linq;
using System.Security.Claims;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
    }
}