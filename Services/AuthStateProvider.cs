using CreaState.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CreaState.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private Membre? _currentMembre;
        private User? _currentUser;

        private static readonly AuthenticationState Anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        public Membre? CurrentMembre => _currentMembre;
        public User? CurrentUser => _currentUser ?? _currentMembre;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _currentUser ?? (User?)_currentMembre;
            if (user == null)
                return Task.FromResult(Anonymous);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new(ClaimTypes.Email, user.Email),
                new("UserType", user.UserType.ToString()),
            };

            if (_currentMembre != null)
            {
                foreach (var mr in _currentMembre.MembreRoles)
                    claims.Add(new(ClaimTypes.Role, mr.Role?.Name ?? ""));

                var allPermissions = _currentMembre.MembreRoles
                    .SelectMany(mr => mr.Role?.RolePermissions ?? [])
                    .Select(rp => rp.Permission.Code)
                    .Distinct();
                foreach (var perm in allPermissions)
                    claims.Add(new Claim("Permission", perm));
            }

            var identity = new ClaimsIdentity(claims, "CreaState.Auth");
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(principal));
        }

        public void LoginMembre(Membre membre)
        {
            _currentMembre = membre;
            _currentUser = membre;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void LoginUser(User user)
        {
            _currentMembre = null;
            _currentUser = user;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            _currentMembre = null;
            _currentUser = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public bool HasPermission(string permissionCode)
        {
            return _currentMembre?.MembreRoles?
                .SelectMany(mr => mr.Role?.RolePermissions ?? [])
                .Any(rp => rp.Permission.Code == permissionCode) ?? false;
        }
    }
}
