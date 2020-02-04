﻿using System;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Controllers
{
    // TODO: authorize
    [ApiController]
    [Route("[controller]")]
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
        public async Task<ActionResult> List()
        {
            return Ok(await SitePersonJoinQuery().ToArrayAsync());
        }

        [HttpGet("{personId}")]
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
        public async Task<ActionResult> Post(int personId, SitePerson sitePerson) {
            var dbSitePerson = await this.dbContext.SitePeople.Where(sp => sp.PersonId == personId && sp.SiteId == SiteId).SingleOrDefaultAsync();

            if (dbSitePerson == null) {
                dbSitePerson = sitePerson; // TODO: copy properties

                this.dbContext.SitePeople.Add(dbSitePerson);
            } else {
                // existing site person, just update props

                // TOOD: copy properties
                dbSitePerson.FirstName = sitePerson.FirstName;
                dbSitePerson.LastName = sitePerson.LastName;
                dbSitePerson.Title = sitePerson.Title;
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