using CreaState.Models;
using CreaState.Repositories.Interfaces;

namespace CreaState.Services
{
    public class InventoryService
    {
        private readonly IConsommableRepository _consommableRepo;

        public InventoryService(IConsommableRepository consommableRepo)
        {
            _consommableRepo = consommableRepo;
        }

        public async Task<List<Consommable>> GetAllItemsAsync(string? type = null)
        {
            if (!string.IsNullOrEmpty(type))
                return await _consommableRepo.GetByTypeAsync(type);
            return await _consommableRepo.GetAllAsync();
        }

        public async Task<List<Consommable>> GetLowStockItemsAsync()
            => await _consommableRepo.GetLowStockAsync();

        public async Task<Consommable> AddItemAsync(Consommable item)
            => await _consommableRepo.AddAsync(item);

        public async Task<bool> UpdateQuantityAsync(int itemId, int newQuantity)
        {
            var item = await _consommableRepo.GetByIdAsync(itemId);
            if (item == null) return false;

            item.Quantite = newQuantity;
            await _consommableRepo.UpdateAsync(item);
            return true;
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var item = await _consommableRepo.GetByIdAsync(itemId);
            if (item == null) return false;

            await _consommableRepo.DeleteAsync(item);
            return true;
        }
    }
}
