using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IFormationRepository : IRepository<Formation>
    {
        Task<List<Formation>> GetUpcomingAsync();
        Task<Formation?> GetWithInstructeurAsync(int id);
    }
}
