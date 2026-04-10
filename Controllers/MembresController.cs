using CreaState.DTOs.Users;
using CreaState.Mapping;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembresController : ControllerBase
    {
        private readonly MemberService _memberService;

        public MembresController(MemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MembreDto>>> GetAll()
        {
            var membres = await _memberService.GetAllMembersAsync();
            return Ok(membres.Select(m => m.ToDto()));
        }

        [HttpGet("active")]
        public async Task<ActionResult<List<MembreDto>>> GetActive()
        {
            var membres = await _memberService.GetActiveMembersAsync();
            return Ok(membres.Select(m => m.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MembreDto>> GetById(int id)
        {
            var membre = await _memberService.GetMemberByIdAsync(id);
            if (membre == null) return NotFound();
            return Ok(membre.ToDto());
        }

        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateRoles(int id, [FromBody] List<int> roleIds)
        {
            var success = await _memberService.UpdateMemberRolesAsync(id, roleIds);
            if (!success) return BadRequest();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _memberService.RemoveMemberAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
