using CreaState.DTOs.Auth;
using CreaState.Mapping;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest dto)
        {
            var (success, error) = await _authService.RegisterAsync(
                dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.ClassYear);

            if (!success)
                return BadRequest(new AuthResponse { Success = false, Message = error });

            return Ok(new AuthResponse { Success = true, Message = "Inscription réussie" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest dto)
        {
            var user = await _authService.LoginUserAsync(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(new AuthResponse { Success = false, Message = "Email ou mot de passe incorrect" });

            return Ok(new AuthResponse
            {
                Success = true,
                UserId = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                UserType = user.UserType.ToString()
            });
        }
    }
}
