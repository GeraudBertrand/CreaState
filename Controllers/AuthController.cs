using CreaState.DTOs.Auth;
using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;
        private readonly IEmailSender<User> _emailSender;

        public AuthController(AuthService authService, IEmailSender<User> emailSender)
        {
            _authService = authService;
            _emailSender = emailSender;
        }

        /// <summary>
        /// POST /api/auth/register
        /// Registers a new user and sends email confirmation.
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest dto)
        {
            var (success, error, user) = await _authService.RegisterAsync(
                dto.Email, dto.Password, dto.FirstName, dto.LastName, dto.ClassYear);

            if (!success || user == null)
                return BadRequest(new AuthResponse { Success = false, Message = error });

            // Generate email confirmation token and send email
            var token = await _authService.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail), "Auth",
                new { userId = user.Id, token },
                Request.Scheme);

            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink!);

            return Ok(new AuthResponse
            {
                Success = true,
                Message = "Inscription reussie ! Verifiez votre email pour confirmer votre compte."
            });
        }

        /// <summary>
        /// POST /api/auth/login
        /// Validates credentials, sets authentication cookie, returns user info.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest dto)
        {
            var result = await _authService.ValidateLoginAsync(dto.Email, dto.Password);
            if (result == null)
                return Unauthorized(new AuthResponse { Success = false, Message = "Email ou mot de passe incorrect." });

            if (result.EmailNotConfirmed)
                return Unauthorized(new AuthResponse { Success = false, Message = "Veuillez confirmer votre email avant de vous connecter. Verifiez votre boite de reception." });

            // Set the authentication cookie via Identity
            await _authService.SignInAsync(result.User, isPersistent: true);

            return Ok(new AuthResponse
            {
                Success = true,
                UserId = result.User.Id,
                Email = result.User.Email,
                FullName = $"{result.User.FirstName} {result.User.LastName}",
                UserType = result.User.UserType.ToString(),
                IsMembre = result.IsMembre,
                HasPrivateAccess = result.HasPrivateAccess
            });
        }

        /// <summary>
        /// POST /api/auth/logout
        /// Clears the authentication cookie.
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            await _authService.SignOutAsync();
            return Ok(new { message = "Deconnexion reussie." });
        }

        /// <summary>
        /// GET /api/auth/confirm-email?userId=X&token=Y
        /// Confirms the user's email address.
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
        {
            if (userId <= 0 || string.IsNullOrEmpty(token))
                return BadRequest("Lien de confirmation invalide.");

            var success = await _authService.ConfirmEmailAsync(userId, token);
            if (!success)
                return BadRequest("Le lien de confirmation est invalide ou a expire.");

            // Redirect to login page with success message
            return Redirect("/?emailConfirmed=true");
        }

        /// <summary>
        /// POST /api/auth/resend-confirmation
        /// Resends the confirmation email.
        /// </summary>
        [HttpPost("resend-confirmation")]
        public async Task<ActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest dto)
        {
            var result = await _authService.ValidateLoginAsync(dto.Email, dto.Password);
            if (result == null)
                return BadRequest(new { message = "Identifiants invalides." });

            if (!result.EmailNotConfirmed)
                return BadRequest(new { message = "L'email est deja confirme." });

            var token = await _authService.GenerateEmailConfirmationTokenAsync(result.User);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail), "Auth",
                new { userId = result.User.Id, token },
                Request.Scheme);

            await _emailSender.SendConfirmationLinkAsync(result.User, result.User.Email!, confirmationLink!);

            return Ok(new { message = "Email de confirmation renvoye." });
        }
    }

    public class ResendConfirmationRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
