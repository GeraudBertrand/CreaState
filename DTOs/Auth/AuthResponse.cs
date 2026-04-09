namespace CreaState.DTOs.Auth
{
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? UserType { get; set; }
    }
}
