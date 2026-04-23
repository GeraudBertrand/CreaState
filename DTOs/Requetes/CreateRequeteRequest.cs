using System.ComponentModel.DataAnnotations;
using CreaState.Models;

namespace CreaState.DTOs.Requetes
{
    public class CreateRequeteRequest
    {
        [Required]
        public RequestType Type { get; set; }

        [Required]
        public RequestContext Context { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public int DemandeurId { get; set; }
    }
}
