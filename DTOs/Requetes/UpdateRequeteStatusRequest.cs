using CreaState.Models;

namespace CreaState.DTOs.Requetes
{
    public class UpdateRequeteStatusRequest
    {
        public RequestStatus Status { get; set; }
        public int? AssigneId { get; set; }
        public string? RejectionReason { get; set; }
    }
}
