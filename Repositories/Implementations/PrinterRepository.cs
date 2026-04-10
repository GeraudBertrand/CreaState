using CreaState.Data;
using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Repositories.Implementations
{
    public class PrinterRepository : Repository<Printer>, IPrinterRepository
    {
        public PrinterRepository(AppDbContext db) : base(db) { }

        public async Task<List<Printer>> GetEnabledAsync()
            => await _dbSet.Where(p => p.Enabled).ToListAsync();
    }
}
