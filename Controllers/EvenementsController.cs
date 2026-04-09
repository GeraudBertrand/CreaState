using CreaState.DTOs.Evenements;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvenementsController : ControllerBase
    {
        private readonly IEvenementRepository _evenementRepo;

        public EvenementsController(IEvenementRepository evenementRepo)
        {
            _evenementRepo = evenementRepo;
        }

        [HttpGet]
        public async Task<ActionResult<List<EvenementDto>>> GetAll()
        {
            var evenements = await _evenementRepo.GetAllAsync();
            return Ok(evenements.Select(e => e.ToDto()));
        }

        [HttpGet("upcoming")]
        public async Task<ActionResult<List<EvenementDto>>> GetUpcoming()
        {
            var evenements = await _evenementRepo.GetUpcomingAsync();
            return Ok(evenements.Select(e => e.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EvenementDto>> GetById(int id)
        {
            var evenement = await _evenementRepo.GetByIdAsync(id);
            if (evenement == null) return NotFound();
            return Ok(evenement.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<EvenementDto>> Create([FromBody] CreateEvenementRequest dto)
        {
            var evenement = new Evenement
            {
                Title = dto.Title,
                Description = dto.Description,
                Date = dto.Date,
                Location = dto.Location,
                Icone = dto.Icone
            };

            var created = await _evenementRepo.AddAsync(evenement);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var evenement = await _evenementRepo.GetByIdAsync(id);
            if (evenement == null) return NotFound();
            await _evenementRepo.DeleteAsync(evenement);
            return NoContent();
        }
    }
}
