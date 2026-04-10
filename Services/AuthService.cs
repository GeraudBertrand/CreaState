using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IMembreRepository _membreRepo;

        private const string AllowedDomain = "@edu.devinci.fr";

        public AuthService(UserManager<User> userManager, SignInManager<User> signInManager, IMembreRepository membreRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _membreRepo = membreRepo;
        }

        /// <summary>
        /// Register a new user (Eleve by default). Uses Identity's UserManager.
        /// Returns the user and any errors.
        /// </summary>
        public async Task<(bool Success, string? Error, User? User)> RegisterAsync(
            string email, string password, string firstName, string lastName, ClassYearEnum classYear)
        {
            email = email.Trim().ToLowerInvariant();

            if (!email.EndsWith(AllowedDomain))
                return (false, $"L'email doit se terminer par {AllowedDomain}", null);

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
                return (false, "Un compte existe deja avec cet email", null);

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                ClassYear = classYear,
                UserType = UserType.Eleve,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return (false, errors, null);
            }

            // Assign default "Eleve" role
            await _userManager.AddToRoleAsync(user, "Eleve");

            return (true, null, user);
        }

        /// <summary>
        /// Login: validates credentials via Identity, returns LoginResult with User + Membre info.
        /// Does NOT set the cookie — the caller (controller) does that via SignInManager.
        /// </summary>
        public async Task<LoginResult?> ValidateLoginAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (!passwordValid)
                return null;

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                return new LoginResult { User = user, Membre = null, EmailNotConfirmed = true };

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            // If the user is a Membre, load with roles and permissions
            Membre? membre = null;
            if (user.UserType == UserType.Membre)
            {
                membre = await _membreRepo.GetByEmailWithRolesAsync(email);
            }

            return new LoginResult { User = user, Membre = membre };
        }

        /// <summary>
        /// Sign in the user with a persistent cookie via Identity SignInManager.
        /// Must be called from an HTTP context (controller), not from Blazor Server directly.
        /// </summary>
        public async Task SignInAsync(User user, bool isPersistent = true)
        {
            await _signInManager.SignInAsync(user, isPersistent);
        }

        /// <summary>
        /// Sign out — clears the authentication cookie.
        /// </summary>
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        /// <summary>
        /// Generate email confirmation token for a user.
        /// </summary>
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        /// <summary>
        /// Confirm user's email with token.
        /// </summary>
        public async Task<bool> ConfirmEmailAsync(int userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return false;

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded;
        }
    }

    public class LoginResult
    {
        public required User User { get; set; }
        public Membre? Membre { get; set; }
        public bool EmailNotConfirmed { get; set; }

        public bool IsMembre => Membre != null;
        public bool HasPrivateAccess => Membre?.UserRoles.Any(ur => ur.Role?.Name != "Eleve") ?? false;
    }
}
