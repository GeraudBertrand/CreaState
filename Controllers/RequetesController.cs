using CreaState.DTOs.Requetes;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RequetesController : ControllerBase
    {
        private readonly RequestService _requestService;

        public RequetesController(RequestService requestService)
        {
            _requestService = requestService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RequeteDto>>> GetAll([FromQuery] RequestStatus? status)
        {
            var requetes = await _requestService.GetAllRequestsAsync(status);
            return Ok(requetes.Select(r => r.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RequeteDto>> GetById(int id)
        {
            var requete = await _requestService.GetRequestByIdAsync(id);
            if (requete == null) return NotFound();
            return Ok(requete.ToDto());
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<List<RequeteDto>>> GetByUser(int userId)
        {
            var requetes = await _requestService.GetRequestsForUserAsync(userId);
            return Ok(requetes.Select(r => r.ToDto()));
        }

        [HttpPost]
        public async Task<ActionResult<RequeteDto>> Create([FromBody] CreateRequeteRequest dto)
        {
            var requete = new Requete
            {
                Type = dto.Type,
                Context = dto.Context,
                Title = dto.Title,
                Description = dto.Description,
                DemandeurId = dto.DemandeurId
            };

            var created = await _requestService.CreateRequestAsync(requete);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateRequeteStatusRequest dto)
        {
            var success = await _requestService.UpdateStatusAsync(id, dto.Status, dto.AssigneId, dto.RejectionReason);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
