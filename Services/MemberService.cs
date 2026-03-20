using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class MemberService
    {
        private readonly AppDbContext _db;

        public MemberService(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Récupère un membre par son ID avec son rôle et permissions.
        /// </summary>
        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            return await _db.Members
                .Include(m => m.Role)
                    .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        /// <summary>
        /// Récupère tous les membres actifs avec leur rôle (pour la page admin).
        /// </summary>
        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _db.Members
                .Include(m => m.Role)
                .Where(m => m.IsActive)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Récupère les membres actifs hors Élèves (pour l'annuaire).
        /// </summary>
        public async Task<List<Member>> GetActiveMembersAsync()
        {
            return await _db.Members
                .Include(m => m.Role)
                .Where(m => m.IsActive && m.Role != null && m.RoleId != 1)
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Change le rôle d'un membre.
        /// </summary>
        public async Task<bool> UpdateMemberRoleAsync(int memberId, int roleId)
        {
            var member = await _db.Members.FindAsync(memberId);
            if (member == null) return false;

            var role = await _db.Roles.FindAsync(roleId);
            if (role == null) return false;

            member.RoleId = roleId;
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Supprime un membre de la base de données.
        /// </summary>
        public async Task<bool> RemoveMemberAsync(int memberId)
        {
            var member = await _db.Members.FindAsync(memberId);
            if (member == null) return false;

            _db.Members.Remove(member);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
