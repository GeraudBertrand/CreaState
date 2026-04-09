using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IConsommableRepository : IRepository<Consommable>
    {
        Task<List<Consommable>> GetLowStockAsync();
        Task<List<Consommable>> GetByTypeAsync(string type);
    }
}
