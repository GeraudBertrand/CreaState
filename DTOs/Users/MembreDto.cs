using CreaState.DTOs.Roles;

namespace CreaState.DTOs.Users
{
    public class MembreDto : UserDto
    {
        public DateTime JoinDate { get; set; }
        public bool IsActive { get; set; }
        public List<RoleDto> Roles { get; set; } = [];
        public string RoleLabel { get; set; } = string.Empty;
        public bool IsBoardMember { get; set; }
        public string AvatarColor { get; set; } = string.Empty;
    }
}
