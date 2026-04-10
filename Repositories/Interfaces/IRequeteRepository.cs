using CreaState.Models;

namespace CreaState.Repositories.Interfaces
{
    public interface IRequeteRepository : IRepository<Requete>
    {
        Task<Requete?> GetWithDetailsAsync(int id);
        Task<List<Requete>> GetByDemandeurAsync(int userId);
        Task<List<Requete>> GetByStatusAsync(RequestStatus status);
        Task<List<Requete>> GetAllWithDetailsAsync();
    }
}
