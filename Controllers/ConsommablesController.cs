using CreaState.DTOs.Consommables;
using CreaState.Mapping;
using CreaState.Models;
using CreaState.Services;
using Microsoft.AspNetCore.Mvc;

namespace CreaState.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConsommablesController : ControllerBase
    {
        private readonly InventoryService _inventoryService;

        public ConsommablesController(InventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ConsommableDto>>> GetAll([FromQuery] string? type)
        {
            var items = await _inventoryService.GetAllItemsAsync(type);
            return Ok(items.Select(c => c.ToDto()));
        }

        [HttpGet("low-stock")]
        public async Task<ActionResult<List<ConsommableDto>>> GetLowStock()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            return Ok(items.Select(c => c.ToDto()));
        }

        [HttpPost]
        public async Task<ActionResult<ConsommableDto>> Create([FromBody] CreateConsommableRequest dto)
        {
            var item = new Consommable
            {
                Type = dto.Type,
                Quantite = dto.Quantite,
                Seuil = dto.Seuil,
                CouleurNom = dto.CouleurNom,
                CouleurHex = dto.CouleurHex
            };

            var created = await _inventoryService.AddItemAsync(item);
            return Ok(created.ToDto());
        }

        [HttpPatch("{id}/quantity")]
        public async Task<IActionResult> UpdateQuantity(int id, [FromBody] int newQuantity)
        {
            var success = await _inventoryService.UpdateQuantityAsync(id, newQuantity);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _inventoryService.DeleteItemAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
