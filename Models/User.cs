using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        public string? PasswordHash { get; set; }

        public ClassYearEnum ClassYear { get; set; } = ClassYearEnum.Other;

        public UserType UserType { get; set; } = UserType.Eleve;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }
    }
}
