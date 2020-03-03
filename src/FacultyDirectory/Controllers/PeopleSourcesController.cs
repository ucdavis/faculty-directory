using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Data;
using FacultyDirectory.Core.Domain;
using FacultyDirectory.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FacultyDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "Admin")]
    public class PeopleSourcesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public PeopleSourcesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpPost("{personId}")]
        public async Task<ActionResult> Post(int personId, PersonSource[] sources)
        {
            // sync the sources for this person
            var personSources = await dbContext.PeopleSources.Where(s => s.PersonId == personId).ToListAsync();

            foreach (var source in sources)
            {
                // find the existing person source
                var matchingSource = personSources.SingleOrDefault(s => s.PersonId == personId && s.Source == source.Source);

                if (matchingSource == null)
                {
                    // add a new source to the db if it doesn't exits for this person
                    dbContext.PeopleSources.Add(new PersonSource
                    {
                        PersonId = personId,
                        Source = source.Source,
                        SourceKey = source.SourceKey.NullIfEmpty()
                    });
                }
                else if (matchingSource.SourceKey != source.SourceKey)
                {
                    // only if the source key has changed, update the record and reset the data
                    matchingSource.SourceKey = source.SourceKey.NullIfEmpty();
                    matchingSource.Data = null;
                    matchingSource.LastUpdate = null;
                }
            }

            await dbContext.SaveChangesAsync();

            return Ok(personId);
        }
    }
}