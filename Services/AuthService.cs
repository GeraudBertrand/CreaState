using CreaState.Data;
using CreaState.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class AuthService
    {
        private readonly AppDbContext _db;
        private readonly PasswordHasher<Member> _hasher = new();

        private const string AllowedDomain = "@edu.devinci.fr";

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(
            string email, string password, string firstName, string lastName, ClassYearEnum classYear)
        {
            email = email.Trim().ToLowerInvariant();

            if (!email.EndsWith(AllowedDomain))
                return (false, $"L'email doit se terminer par {AllowedDomain}");

            if (await _db.Users.AnyAsync(u => u.Email == email))
                return (false, "Un compte existe déjà avec cet email");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return (false, "Le mot de passe doit contenir au moins 6 caractères");

            // Rôle par défaut (Élève)
            var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.IsDefault);
            if (defaultRole == null)
                return (false, "Erreur de configuration : aucun rôle par défaut défini");

            var member = new Member
            {
                FirstName = firstName.Trim(),
                LastName = lastName.Trim(),
                Email = email,
                ClassYear = classYear,
                IsActive = true,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            member.PasswordHash = _hasher.HashPassword(member, password);

            _db.Members.Add(member);
            await _db.SaveChangesAsync();

            // Assigner le rôle par défaut via MemberRole
            _db.MemberRoles.Add(new MemberRole { MemberId = member.Id, RoleId = defaultRole.Id });
            await _db.SaveChangesAsync();

            return (true, null);
        }

        public async Task<Member?> LoginAsync(string email, string password)
        {
            email = email.Trim().ToLowerInvariant();

            var member = await _db.Members
                .Include(m => m.MemberRoles)
                    .ThenInclude(mr => mr.Role)
                    .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.Email == email && m.IsActive);

            if (member == null || string.IsNullOrEmpty(member.PasswordHash))
                return null;

            var result = _hasher.VerifyHashedPassword(member, member.PasswordHash, password);
            if (result == PasswordVerificationResult.Failed)
                return null;

            // Mettre à jour la date de dernière connexion
            member.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return member;
        }
    }
}
