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
        private readonly IScholarService scholarService;
        private readonly ISiteFarmService siteFarmService;

        public SystemController(IDirectoryPopulationService directoryPopulationService, IScholarService scholarService, ISiteFarmService siteFarmService)
        {
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
        public async Task<ActionResult> Publish()
        {
            var result = await this.siteFarmService.PublishPerson(null);

            return Json(result);
        }
    }
}
