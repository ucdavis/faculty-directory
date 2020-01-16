
namespace FacultyDirectory.Controllers
{
    // TODO: authorize
    [ApiController]
    [Route("[controller]")]
    public class PersonController : ControllerBase {
        private readonly ApplicationDbContext dbContext;

        public PersonController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public async Task List() {
            return "hello";
        }
    }
}