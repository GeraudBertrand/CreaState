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
                .Include(m => m.MembreRoles)
                .FirstOrDefaultAsync(m => m.Id == membreId);
            if (membre == null) return false;

            var validRoles = await _db.Roles.Where(r => roleIds.Contains(r.Id)).ToListAsync();
            if (validRoles.Count != roleIds.Count) return false;

            _db.MembreRoles.RemoveRange(membre.MembreRoles);
            foreach (var roleId in roleIds)
                _db.MembreRoles.Add(new MembreRole { MembreId = membreId, RoleId = roleId });

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
