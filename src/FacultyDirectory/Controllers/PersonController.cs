
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
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

        public SitePeopleController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task<ActionResult> List()
        {
            return Ok(await SitePersonJoinQuery().ToArrayAsync());
        }

        [HttpGet("{personId}")]
        public async Task<ActionResult> Get(int personId)
        {
            var person = SitePersonJoinQuery().Where(p => p.Person.Id == personId);

            return Ok(await person.FirstOrDefaultAsync());
        }

        [HttpPost("{personId}")]
        public async Task<ActionResult> Post(int personId, SitePerson sitePerson) {
            var dbSitePerson = await this.dbContext.SitePeople.Where(sp => sp.PersonId == personId && sp.SiteId == SiteId).SingleOrDefaultAsync();

            if (dbSitePerson == null) {
                dbSitePerson = sitePerson; // TODO: copy properties

                this.dbContext.SitePeople.Add(dbSitePerson);
            }

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