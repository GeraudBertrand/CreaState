using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IEvenementRepository : IRepository<Evenement>
    {
        Task<List<Evenement>> GetUpcomingAsync();
    }
}
