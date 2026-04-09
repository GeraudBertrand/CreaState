namespace CreaState.DTOs.Roles
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<PermissionDto> Permissions { get; set; } = [];
    }
}
