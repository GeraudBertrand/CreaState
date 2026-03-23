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
        /// Récupère un membre par son ID avec ses rôles et permissions.
        /// </summary>
        public async Task<Member?> GetMemberByIdAsync(int id)
        {
            return await _db.Members
                .Include(m => m.MemberRoles)
                    .ThenInclude(mr => mr.Role)
                    .ThenInclude(r => r!.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
        }

        /// <summary>
        /// Récupère tous les membres actifs avec leurs rôles (pour la page admin).
        /// </summary>
        public async Task<List<Member>> GetAllMembersAsync()
        {
            return await _db.Members
                .Include(m => m.MemberRoles)
                    .ThenInclude(mr => mr.Role)
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
                .Include(m => m.MemberRoles)
                    .ThenInclude(mr => mr.Role)
                .Where(m => m.IsActive && m.MemberRoles.Any(mr => mr.Role!.Name != "Eleve"))
                .OrderBy(m => m.LastName)
                .ThenBy(m => m.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Met à jour les rôles d'un membre (remplace tous les rôles existants).
        /// </summary>
        public async Task<bool> UpdateMemberRolesAsync(int memberId, List<int> roleIds)
        {
            var member = await _db.Members
                .Include(m => m.MemberRoles)
                .FirstOrDefaultAsync(m => m.Id == memberId);
            if (member == null) return false;

            // Vérifier que tous les rôles existent
            var validRoles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            if (validRoles.Count != roleIds.Count) return false;

            // Supprimer les anciens rôles
            _db.MemberRoles.RemoveRange(member.MemberRoles);

            // Ajouter les nouveaux
            foreach (var roleId in roleIds)
            {
                _db.MemberRoles.Add(new MemberRole { MemberId = memberId, RoleId = roleId });
            }

            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Change le rôle principal d'un membre (ancien comportement, remplace tous les rôles par un seul).
        /// </summary>
        public async Task<bool> UpdateMemberRoleAsync(int memberId, int roleId)
        {
            return await UpdateMemberRolesAsync(memberId, new List<int> { roleId });
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
