using CreaState.DTOs.Printers;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintJobsController : ControllerBase
    {
        private readonly PrintJobService _printJobService;

        public PrintJobsController(PrintJobService printJobService)
        {
            _printJobService = printJobService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PrintJobDto>>> GetRecent(
            [FromQuery] int days = 30,
            [FromQuery] int? printerId = null,
            [FromQuery] PrintStatus? status = null)
        {
            var jobs = await _printJobService.GetRecentJobsAsync(days, printerId, status);
            return Ok(jobs.Select(j => j.ToDto()));
        }
    }
}
