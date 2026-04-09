using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    /// <summary>
    /// MVC Controller for cookie-based login/logout.
    /// Blazor Server cannot use SignInManager directly (no HttpContext in SignalR circuit),
    /// so these endpoints handle authentication via form POST + redirect.
    /// </summary>
    [Route("account")]
    [IgnoreAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly AuthService _authService;
        private readonly IEmailSender<User> _emailSender;

        public AccountController(AuthService authService, IEmailSender<User> emailSender)
        {
            _authService = authService;
            _emailSender = emailSender;
        }

        /// <summary>
        /// POST /account/login — form-based login that sets the auth cookie.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] string email, [FromForm] string password)
        {
            var result = await _authService.ValidateLoginAsync(email, password);

            if (result == null)
                return Redirect("/?error=invalid");

            if (result.EmailNotConfirmed)
                return Redirect("/?error=email_not_confirmed");

            // Set the authentication cookie
            await _authService.SignInAsync(result.User, isPersistent: true);

            // Redirect based on user type
            if (result.HasPrivateAccess)
                return Redirect("/private");

            return Redirect("/public");
        }

        /// <summary>
        /// POST /account/register — form-based registration with email confirmation.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromForm] string email,
            [FromForm] string password,
            [FromForm] string firstName,
            [FromForm] string lastName,
            [FromForm] ClassYearEnum classYear)
        {
            var (success, error, user) = await _authService.RegisterAsync(email, password, firstName, lastName, classYear);

            if (!success || user == null)
                return Redirect($"/register?error={Uri.EscapeDataString(error ?? "Erreur")}");

            // Generate and send email confirmation
            var token = await _authService.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(
                nameof(ConfirmEmail), "Account",
                new { userId = user.Id, token },
                Request.Scheme);

            await _emailSender.SendConfirmationLinkAsync(user, user.Email!, confirmationLink!);

            return Redirect("/?registered=true");
        }

        /// <summary>
        /// GET /account/confirm-email — confirms the user's email via token link.
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] int userId, [FromQuery] string token)
        {
            if (userId <= 0 || string.IsNullOrEmpty(token))
                return Redirect("/?error=invalid_token");

            var success = await _authService.ConfirmEmailAsync(userId, token);
            if (!success)
                return Redirect("/?error=invalid_token");

            return Redirect("/?emailConfirmed=true");
        }

        /// <summary>
        /// POST /account/logout — clears the auth cookie and redirects to login.
        /// </summary>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.SignOutAsync();
            return Redirect("/");
        }

        /// <summary>
        /// GET /account/logout — convenience GET logout (for nav links).
        /// </summary>
        [HttpGet("logout")]
        public async Task<IActionResult> LogoutGet()
        {
            await _authService.SignOutAsync();
            return Redirect("/");
        }
    }
}
