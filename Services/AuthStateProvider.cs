using CreaState.Models;
using CreaState.Repositories.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CreaState.Services
{
    /// <summary>
    /// Cookie-based AuthenticationStateProvider for Blazor Server.
    /// Reads auth state from the Identity cookie, then lazily loads the
    /// Membre entity with roles/permissions for UI use.
    /// </summary>
    public class AuthStateProvider : RevalidatingServerAuthenticationStateProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private Membre? _currentMembre;
        private User? _currentUser;
        private bool _loaded;

        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        public AuthStateProvider(ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
            : base(loggerFactory)
        {
            _serviceProvider = serviceProvider;
        }

        public Membre? CurrentMembre => _currentMembre;
        public User? CurrentUser => _currentUser ?? _currentMembre;

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            // Get a fresh scope for validation
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var userId = authenticationState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return false;

            var user = await userManager.FindByIdAsync(userId);
            if (user == null) return false;

            // Validate security stamp
            var stamp = authenticationState.User.FindFirst("AspNet.Identity.SecurityStamp")?.Value;
            if (stamp != null && user.SecurityStamp != stamp)
                return false;

            return true;
        }

        /// <summary>
        /// Ensures the current Membre/User is loaded from DB.
        /// Call this in any component that needs CurrentMembre/CurrentUser.
        /// </summary>
        public async Task EnsureLoadedAsync(AuthenticationState authState)
        {
            if (_loaded) return;

            var principal = authState.User;
            if (principal.Identity?.IsAuthenticated != true)
            {
                _loaded = true;
                return;
            }

            var userIdStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out var userId))
            {
                _loaded = true;
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var membreRepo = scope.ServiceProvider.GetRequiredService<IMembreRepository>();

            // Try to load as Membre (includes UserRoles + Permissions)
            var membre = await membreRepo.GetWithRolesAsync(userId);
            if (membre != null)
            {
                _currentMembre = membre;
                _currentUser = membre;
            }
            else
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                _currentUser = await userManager.FindByIdAsync(userId.ToString());
                _currentMembre = null;
            }

            _loaded = true;
        }

        /// <summary>
        /// Check if the current membre has a specific permission code.
        /// </summary>
        public bool HasPermission(string permissionCode)
        {
            return _currentMembre?.UserRoles?
                .SelectMany(ur => ur.Role?.RolePermissions ?? [])
                .Any(rp => rp.Permission.Code == permissionCode) ?? false;
        }

        /// <summary>
        /// Force a reload of the user/membre data.
        /// </summary>
        public void Reset()
        {
            _loaded = false;
            _currentUser = null;
            _currentMembre = null;
        }
    }
}
