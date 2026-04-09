using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class MemberService
    {
        private readonly IMembreRepository _membreRepo;
        private readonly AppDbContext _db;

        public MemberService(IMembreRepository membreRepo, AppDbContext db)
        {
            _membreRepo = membreRepo;
            _db = db;
        }

        public async Task<Membre?> GetMemberByIdAsync(int id)
            => await _membreRepo.GetWithRolesAsync(id);

        public async Task<List<Membre>> GetAllMembersAsync()
            => await _membreRepo.GetAllWithRolesAsync();

        public async Task<List<Membre>> GetActiveMembersAsync()
            => await _membreRepo.GetAllActiveAsync();

        public async Task<bool> UpdateMemberRolesAsync(int membreId, List<int> roleIds)
        {
            var membre = await _db.Membres
                .Include(m => m.UserRoles)
                .FirstOrDefaultAsync(m => m.Id == membreId);
            if (membre == null) return false;

            var validRoles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            if (validRoles.Count != roleIds.Count) return false;

            // Remove existing roles via the UserRoles join table
            var existingUserRoles = _db.Set<AppUserRole>().Where(ur => ur.UserId == membreId);
            _db.Set<AppUserRole>().RemoveRange(existingUserRoles);

            foreach (var roleId in roleIds)
                _db.Set<AppUserRole>().Add(new AppUserRole { UserId = membreId, RoleId = roleId });

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int membreId)
        {
            var membre = await _membreRepo.GetByIdAsync(membreId);
            if (membre == null) return false;

            await _membreRepo.DeleteAsync(membre);
            return true;
        }
    }
}
