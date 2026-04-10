using CreaState.DTOs.Formations;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FormationsController : ControllerBase
    {
        private readonly IFormationRepository _formationRepo;

        public FormationsController(IFormationRepository formationRepo)
        {
            _formationRepo = formationRepo;
        }

        [HttpGet]
        public async Task<ActionResult<List<FormationDto>>> GetAll()
        {
            var formations = await _formationRepo.GetAllAsync();
            return Ok(formations.Select(f => f.ToDto()));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<FormationDto>>> GetUpcoming()
        {
            var formations = await _formationRepo.GetUpcomingAsync();
            return Ok(formations.Select(f => f.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FormationDto>> GetById(int id)
        {
            var formation = await _formationRepo.GetWithInstructeurAsync(id);
            if (formation == null) return NotFound();
            return Ok(formation.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<FormationDto>> Create([FromBody] CreateFormationRequest dto)
        {
            var formation = new Formation
            {
                Titre = dto.Titre,
                Description = dto.Description,
                InstructeurId = dto.InstructeurId,
                Date = dto.Date,
                StartTime = dto.StartTime,
                Duration = dto.Duration,
                MaxParticipants = dto.MaxParticipants
            };

            var created = await _formationRepo.AddAsync(formation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var formation = await _formationRepo.GetByIdAsync(id);
            if (formation == null) return NotFound();
            await _formationRepo.DeleteAsync(formation);
            return NoContent();
        }
    }
}
