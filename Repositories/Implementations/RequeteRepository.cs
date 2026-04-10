using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class RequeteRepository : Repository<Requete>, IRequeteRepository
    {
        public RequeteRepository(AppDbContext db) : base(db) { }

        public async Task<Requete?> GetWithDetailsAsync(int id)
            => await _dbSet
                .Include(r => r.Demandeur)
                .Include(r => r.Assigne)
                .Include(r => r.Fichiers)
                .Include(r => r.Commentaires).ThenInclude(c => c.Auteur)
                .FirstOrDefaultAsync(r => r.Id == id);

        public async Task<List<Requete>> GetByDemandeurAsync(int userId)
            => await _dbSet
                .Include(r => r.Demandeur)
                .Include(r => r.Fichiers)
                .Where(r => r.DemandeurId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<List<Requete>> GetByStatusAsync(RequestStatus status)
            => await _dbSet
                .Include(r => r.Demandeur)
                .Where(r => r.Status == status)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<List<Requete>> GetAllWithDetailsAsync()
            => await _dbSet
                .Include(r => r.Demandeur)
                .Include(r => r.Assigne)
                .Include(r => r.Fichiers)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
    }
}
