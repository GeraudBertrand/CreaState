using CreaState.DTOs.Maintenance;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly MaintenanceService _maintenanceService;

        public MaintenanceController(MaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MaintenanceDto>>> GetAll()
        {
            var records = await _maintenanceService.GetAllRecordsAsync();
            return Ok(records.Select(m => m.ToDto()));
        }

        [HttpGet("printer/{printerId}")]
        public async Task<ActionResult<List<MaintenanceDto>>> GetByPrinter(int printerId)
        {
            var records = await _maintenanceService.GetRecordsForPrinterAsync(printerId);
            return Ok(records.Select(m => m.ToDto()));
        }

        [HttpPost]
        public async Task<ActionResult<MaintenanceDto>> Create([FromBody] CreateMaintenanceRequest dto)
        {
            var record = new Models.Maintenance
            {
                PrinterId = dto.PrinterId,
                WorkerId = dto.WorkerId,
                Type = dto.Type,
                Description = dto.Description,
                Date = DateTime.UtcNow
            };

            var created = await _maintenanceService.AddRecordAsync(record);
            return Ok(created.ToDto());
        }
    }
}
