using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class RequestComment
    {
        [Key]
        public int Id { get; set; }

        public int RequestId { get; set; }
        public Request? Request { get; set; }

        public int AuthorMemberId { get; set; }
        public Member? Author { get; set; }

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
