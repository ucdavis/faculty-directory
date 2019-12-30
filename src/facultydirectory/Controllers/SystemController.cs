using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FacultyDirectory.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FacultyDirectory.Controllers
{
    public class SystemController : Controller
    {
        private readonly IDirectoryPopulationService directoryPopulationService;

        public SystemController(IDirectoryPopulationService directoryPopulationService)
        {
            this.directoryPopulationService = directoryPopulationService;
        }

        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var result = await this.directoryPopulationService.GetFacultyAssociations();

            return Json(result);
        }
    }
}
