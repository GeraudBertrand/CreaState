using CreaState.DTOs.Printers;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PrintersController : ControllerBase
    {
        private readonly IPrinterRepository _printerRepo;
        private readonly PrinterService _printerService;

        public PrintersController(IPrinterRepository printerRepo, PrinterService printerService)
        {
            _printerRepo = printerRepo;
            _printerService = printerService;
        }

        [HttpGet]
        public async Task<ActionResult<List<PrinterDto>>> GetAll()
        {
            var printers = await _printerRepo.GetAllAsync();
            return Ok(printers.Select(p => p.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PrinterDto>> GetById(int id)
        {
            var printer = await _printerRepo.GetByIdAsync(id);
            if (printer == null) return NotFound();
            return Ok(printer.ToDto());
        }

        [HttpGet("status")]
        public ActionResult<List<PrinterStatusDto>> GetStatus()
        {
            var states = _printerService.GetPrinterStates();
            return Ok(states.Select(s => new PrinterStatusDto
            {
                PrinterId = s.PrinterId,
                Name = s.Name,
                Status = s.Status.ToString(),
                StatusLabel = s.Status.GetDisplayName(),
                StatusColor = s.Status switch
                {
                    PrinterStatus.Printing => "printing",
                    PrinterStatus.Idle => "idle",
                    PrinterStatus.Success => "success",
                    PrinterStatus.Error => "error",
                    _ => "offline"
                },
                CurrentFile = s.CurrentFile,
                Progress = s.Progress,
                TimeRemainingMinutes = s.TimeRemainingMinutes,
                NozzleTemp = s.NozzleTemp,
                BedTemp = s.BedTemp,
                FilamentType = s.FilamentType,
                FilamentColor = s.FilamentColor
            }));
        }
    }
}
