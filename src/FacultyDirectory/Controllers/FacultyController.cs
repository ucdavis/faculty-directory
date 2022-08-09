using System;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Services;
using FacultyDirectory.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Controllers
{
    // This controller is for methods that can be called by anyone with a sitePerson record (or admins)
    [ApiController]
    [Route("api/[controller]")]
    public class FacultyController : ControllerBase
    {
        const int SiteId = 1; // TODO: support more sites

        private readonly ApplicationDbContext dbContext;
        private readonly IHttpContextAccessor contextAccessor;
        private readonly ISiteFarmService siteFarmService;

        public FacultyController(ApplicationDbContext dbContext, IHttpContextAccessor contextAccessor, ISiteFarmService siteFarmService)
        {
            this.dbContext = dbContext;
            this.contextAccessor = contextAccessor;
            this.siteFarmService = siteFarmService;
        }

        // Return current user info
        [HttpGet("userinfo")]
        [Authorize] // only check they have auth'd
        public async Task<ActionResult> Get()
        {
            var username = contextAccessor.HttpContext.User.Identity.Name;

            // first, let's see if the person is an admin
            var isAdmin = await dbContext.Users.AnyAsync(u => u.Username == username);

            // now, let's pull all possible sitePerson records for this user
            // TODO: yes, this is hacky, should put in a user service
            var claims = contextAccessor.HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToArray();

            var count = claims.Length;

            string iamId = contextAccessor.HttpContext.User.Claims.SingleOrDefault(c => c.Type == AdminOrSelfAuthorizationHandler.IamIdClaimType)?.Value;

            var sitePeople = await dbContext.SitePeople.Where(sp => sp.Person.IamId == iamId).ToArrayAsync();

            return Ok(new { username, isAdmin, sitePeople });
        }

        [HttpPost("{personId}/pronunciation")]
        [Authorize(Policy = "Self")]
        public async Task<ActionResult> Post([FromForm] IFormFile audioFile, int personId)
        {
            var dbSitePerson = await this.dbContext.SitePeople.Where(sp => sp.PersonId == personId && sp.SiteId == SiteId).SingleOrDefaultAsync();
            if (dbSitePerson == null)
            {
                return NotFound();
            }

            using var stream = audioFile.OpenReadStream();

            var mediaUid = await this.siteFarmService.PublishAudio(stream, audioFile.FileName);

            dbSitePerson.PronunciationUid = new Guid(mediaUid);

            await this.dbContext.SaveChangesAsync();

            return Ok(new { MediaUid = mediaUid });
        }

        [HttpGet("{personId}/pronunciation")]
        [Authorize(Policy = "Self")]
        public async Task<IActionResult> GetPronunciation(int personId)
        {
            var dbSitePerson = await this.dbContext.SitePeople.Where(sp => sp.PersonId == personId && sp.SiteId == SiteId).SingleOrDefaultAsync();
            if (dbSitePerson == null)
            {
                return NotFound();
            }

            var mediaUid = dbSitePerson.PronunciationUid.ToString();
            var mediaStream = await this.siteFarmService.GetAudio(mediaUid);

            return File(mediaStream, "audio/mp3");
        }
    }
}