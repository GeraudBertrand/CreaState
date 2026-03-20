using CreaState.Data;
using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Services
{
    public class InventoryService
    {
        private readonly AppDbContext _db;

        public InventoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<InventoryItem>> GetAllItemsAsync(InventoryCategory? category = null)
        {
            var query = _db.InventoryItems.AsQueryable();
            if (category.HasValue)
                query = query.Where(i => i.Category == category.Value);
            return await query.OrderBy(i => i.Name).ToListAsync();
        }

        public async Task<List<InventoryItem>> GetLowStockItemsAsync()
        {
            return await _db.InventoryItems
                .Where(i => i.QuantityRemaining <= i.LowStockThreshold)
                .OrderBy(i => i.QuantityRemaining)
                .ToListAsync();
        }

        public async Task<InventoryItem> AddItemAsync(InventoryItem item)
        {
            item.LastUpdated = DateTime.UtcNow;
            _db.InventoryItems.Add(item);
            await _db.SaveChangesAsync();
            return item;
        }

        public async Task<bool> UpdateQuantityAsync(int itemId, double newQuantity)
        {
            var item = await _db.InventoryItems.FindAsync(itemId);
            if (item == null) return false;

            item.QuantityRemaining = newQuantity;
            item.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteItemAsync(int itemId)
        {
            var item = await _db.InventoryItems.FindAsync(itemId);
            if (item == null) return false;

            _db.InventoryItems.Remove(item);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
