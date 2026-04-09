using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IMaintenanceRepository : IRepository<Models.Maintenance>
    {
        Task<List<Models.Maintenance>> GetByPrinterIdAsync(int printerId);
        Task<List<Models.Maintenance>> GetAllWithDetailsAsync();
    }
}
