using System.Security.Claims;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;

namespace CreaState.Services
{
    /// <summary>
    /// Adds Permission claims to the user's identity based on their roles.
    /// This runs on every request after the cookie is read, so the
    /// [Authorize(Policy = "...")] attributes work with our custom Permission system.
    /// </summary>
    public class PermissionClaimsTransformation : IClaimsTransformation
    {
        private readonly IServiceProvider _serviceProvider;

        public PermissionClaimsTransformation(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity?.IsAuthenticated != true)
                return principal;

            // Check if we already added permission claims (avoid duplicating)
            if (principal.HasClaim(c => c.Type == "Permission"))
                return principal;

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return principal;

            using var scope = _serviceProvider.CreateScope();
            var membreRepo = scope.ServiceProvider.GetRequiredService<IMembreRepository>();

            var membre = await membreRepo.GetWithRolesAsync(userId);
            if (membre == null)
                return principal;

            var identity = principal.Identity as ClaimsIdentity;
            if (identity == null)
                return principal;

            // Add role claims
            foreach (var ur in membre.UserRoles)
            {
                if (!string.IsNullOrEmpty(ur.Role?.Name))
                {
                    if (!principal.HasClaim(ClaimTypes.Role, ur.Role.Name))
                        identity.AddClaim(new Claim(ClaimTypes.Role, ur.Role.Name));
                }
            }

            // Add permission claims from role-permission matrix
            var permissions = membre.UserRoles
                .SelectMany(ur => ur.Role?.RolePermissions ?? [])
                .Select(rp => rp.Permission.Code)
                .Distinct();

            foreach (var perm in permissions)
            {
                identity.AddClaim(new Claim("Permission", perm));
            }

            return principal;
        }
    }
}
