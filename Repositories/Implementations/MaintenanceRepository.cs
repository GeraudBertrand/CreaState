using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class MaintenanceRepository : Repository<Models.Maintenance>, IMaintenanceRepository
    {
        public MaintenanceRepository(AppDbContext db) : base(db) { }

        public async Task<List<Models.Maintenance>> GetByPrinterIdAsync(int printerId)
            => await _dbSet
                .Include(m => m.Worker)
                .Where(m => m.PrinterId == printerId)
                .OrderByDescending(m => m.Date)
                .ToListAsync();

        public async Task<List<Models.Maintenance>> GetAllWithDetailsAsync()
            => await _dbSet
                .Include(m => m.Printer)
                .Include(m => m.Worker)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
    }
}
