using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CreaState.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepo;
        private readonly IMembreRepository _membreRepo;
        private readonly IRoleRepository _roleRepo;
        private readonly PasswordHasher<User> _hasher = new();

        private const string AllowedDomain = "@edu.devinci.fr";

        public AuthService(IUserRepository userRepo, IMembreRepository membreRepo, IRoleRepository roleRepo)
        {
            _userRepo = userRepo;
            _membreRepo = membreRepo;
            _roleRepo = roleRepo;
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(
            string email, string password, string firstName, string lastName, ClassYearEnum classYear)
        {
            email = email.Trim().ToLowerInvariant();

            if (!email.EndsWith(AllowedDomain))
                return (false, $"L'email doit se terminer par {AllowedDomain}");

            if (await _userRepo.GetByEmailAsync(email) != null)
                return (false, "Un compte existe déjà avec cet email");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Le mot de passe doit contenir au moins 6 caractères");

            var defaultRole = await _roleRepo.GetDefaultRoleAsync();
            if (defaultRole == null)
                return (false, "Erreur de configuration : aucun rôle par défaut défini");

            // Par défaut, on crée un User (Eleve). Pour un Membre, on utilise PromoteToMembre.
            var user = new User
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email,
                ClassYear = classYear,
                UserType = UserType.Eleve,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _hasher.HashPassword(user, password);

            await _userRepo.AddAsync(user);

            return (true, null);
        }

        public async Task<Membre?> LoginAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();

            var membre = await _membreRepo.GetByEmailWithRolesAsync(email);
            if (membre == null || string.IsNullOrEmpty(membre.PasswordHash))
                return null;

            var result = _hasher.VerifyHashedPassword(membre, membre.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            membre.LastLoginAt = DateTime.UtcNow;
            await _membreRepo.UpdateAsync(membre);

            return membre;
        }

        public async Task<User?> LoginUserAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(user);

            return user;
        }
    }
}
