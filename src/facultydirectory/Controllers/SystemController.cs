using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FacultyDirectory.Controllers
{
    public class SystemController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly IDirectoryPopulationService directoryPopulationService;
        private readonly IScholarService scholarService;
        private readonly ISiteFarmService siteFarmService;

        public SystemController(ApplicationDbContext dbContext, IDirectoryPopulationService directoryPopulationService, IScholarService scholarService, ISiteFarmService siteFarmService)
        {
            this.dbContext = dbContext;
            this.directoryPopulationService = directoryPopulationService;
            this.scholarService = scholarService;
            this.siteFarmService = siteFarmService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var result = await this.directoryPopulationService.ExtractCandidates();
            await this.directoryPopulationService.MergeFaculty(result);

            return Json(result);
        }

        [HttpGet]
        public async Task<ActionResult> Scholar()
        {
            var result = await this.scholarService.GetTagsAndPublicationsById("tfLsszUAAAAJ");

            return Json(result);
        }

        [HttpGet]
        public async Task<ActionResult> FullPublish()
        {
            int personId = 1;

            var sitePerson = await this.dbContext.SitePeople.Include(sp => sp.Person).SingleAsync(s => s.Id == personId);
            await this.scholarService.SyncForPerson(sitePerson.PersonId);
            var result = await this.siteFarmService.PublishPerson(sitePerson);

            return Json(result);
        }

        [HttpGet]
        public async Task<ActionResult> Publish()
        {
            var sitePerson = await this.dbContext.SitePeople.Include(sp => sp.Person).SingleAsync(s => s.Id == 1);
            var result = await this.siteFarmService.PublishPerson(sitePerson);

            return Json(result);
        }

        [HttpGet]
        public async Task<ActionResult> SyncTags() {
            var person = await this.dbContext.SitePeople.FirstAsync();

            var tagIds = new List<string>();

            await foreach (var tagId in this.siteFarmService.SyncTags(new[] { "code", "javascript", "html" }))
            {
                tagIds.Add(tagId);                
            }

            return Json(tagIds);
        }

        [HttpGet]
        public async Task<ActionResult> CreatePerson()
        {
            var site = new Site { Name = "Playground", Url = "https://playground.sf.ucdavis.edu" };
            var person = new Person { FirstName = "Test", LastName = "Person", IamId = "123456789" };
            var sitePerson = new SitePerson {  Person = person, Site = site, Name = "Test R. Person" };

            this.dbContext.Add(sitePerson);

            await this.dbContext.SaveChangesAsync();
            
            return Content("done");
        }
    }
}
