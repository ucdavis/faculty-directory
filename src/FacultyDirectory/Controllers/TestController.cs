using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FacultyDirectory.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Policy = "Admin")]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return Ok("Hello World!");
        }
    }
}