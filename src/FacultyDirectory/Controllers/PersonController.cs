
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
    public class PersonController : ControllerBase
    {
        const int SiteId = 1; // TODO: support more sites
        private readonly ApplicationDbContext dbContext;

        public PersonController(ApplicationDbContext dbContext)
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

        private IQueryable<PersonWithSitePerson> SitePersonJoinQuery()
        {
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