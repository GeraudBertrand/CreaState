using CreaState.DTOs.Roles;
using CreaState.Mapping;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RolesController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<List<RoleDto>>> GetAll()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles.Select(r => r.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoleDto>> GetById(int id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role.ToDto());
        }
    }
}
