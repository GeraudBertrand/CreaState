using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class ConsommableRepository : Repository<Consommable>, IConsommableRepository
    {
        public ConsommableRepository(AppDbContext db) : base(db) { }

        public async Task<List<Consommable>> GetLowStockAsync()
            => await _dbSet.Where(c => c.Quantite <= c.Seuil).ToListAsync();

        public async Task<List<Consommable>> GetByTypeAsync(string type)
            => await _dbSet.Where(c => c.Type == type).ToListAsync();
    }
}
