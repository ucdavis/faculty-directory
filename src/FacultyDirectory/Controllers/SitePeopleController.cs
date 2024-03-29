﻿using System;
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
    [ApiController]
    [Route("api/[controller]")]
    public class SitePeopleController : ControllerBase
    {
        const int SiteId = 1; // TODO: support more sites
        private readonly ApplicationDbContext dbContext;
        private readonly IBiographyGenerationService biographyGenerationService;

        public SitePeopleController(ApplicationDbContext dbContext, IBiographyGenerationService biographyGenerationService)
        {
            this.dbContext = dbContext;
            this.biographyGenerationService = biographyGenerationService;
        }

        [HttpGet]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult> List()
        {
            return Ok(await SitePersonJoinQuery().ToArrayAsync());
        }

        [HttpGet("{personId}")]
        [Authorize(Policy = "Self")]
        public async Task<ActionResult> Get(int personId)
        {
            var personQueryResult = await SitePersonJoinQuery().Where(p => p.Person.Id == personId).AsNoTracking().FirstOrDefaultAsync();

            var sources = await dbContext.PeopleSources.Where(p => p.PersonId == personId).AsNoTracking().ToArrayAsync();

            var sitePerson = personQueryResult.SitePerson ?? new SitePerson();
            sitePerson.Person = personQueryResult.Person;

            var bio = this.biographyGenerationService.Generate(sitePerson, sources);

            return Ok(new { SitePerson = sitePerson, Bio = bio, Sources = sources.Select(s => new { s.Source, s.SourceKey }) });
        }

        [HttpPost("{personId}")]
        [Authorize(Policy = "Admin")]
        public async Task<ActionResult> Post(int personId, SitePerson sitePerson) {
            var dbSitePerson = await this.dbContext.SitePeople.Where(sp => sp.PersonId == personId && sp.SiteId == SiteId).SingleOrDefaultAsync();

            if (dbSitePerson == null) {
                dbSitePerson = sitePerson; // TODO: copy properties
                dbSitePerson.Person = null; // don't overwrite person info

                this.dbContext.SitePeople.Add(dbSitePerson);
            } else {

                // existing site person, just update props
                dbSitePerson.FirstName = sitePerson.FirstName.NullIfEmpty();
                dbSitePerson.LastName = sitePerson.LastName.NullIfEmpty();
                dbSitePerson.Title = sitePerson.Title.NullIfEmpty();
                dbSitePerson.Bio = sitePerson.Bio.NullIfEmpty();
                dbSitePerson.Emails = sitePerson.Emails.NullIfEmpty();
                dbSitePerson.Phones = sitePerson.Phones.NullIfEmpty();
                dbSitePerson.Departments = sitePerson.Departments.NullIfEmpty();
                dbSitePerson.Websites = sitePerson.Websites.NullIfEmpty();
                dbSitePerson.Tags = sitePerson.Tags.NullIfEmpty();
                dbSitePerson.PageUid = sitePerson.PageUid;
                dbSitePerson.ShouldSync = sitePerson.ShouldSync;
            }
            
            dbSitePerson.PersonId = personId;
            dbSitePerson.SiteId = SiteId;
            dbSitePerson.LastUpdate = DateTime.UtcNow;

            await this.dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { sitePersonId = dbSitePerson.Id }, dbSitePerson);
        }

        private IQueryable<PersonWithSitePerson> SitePersonJoinQuery()
        {
            // TODO: need to support multiple sites.  Possibly with composite join.
            return from person in this.dbContext.People
                   join sitePerson in this.dbContext.SitePeople.DefaultIfEmpty() on person.Id equals sitePerson.PersonId into gj
                   from item in gj.DefaultIfEmpty()
                       // where item.SiteId == 1
                   select new PersonWithSitePerson
                   {
                       Person = person,
                       SitePerson = item
                   };
        }
    }

    public class PersonWithSitePerson {
        public Person Person { get; set; }
        public SitePerson SitePerson { get; set; }
    }
}