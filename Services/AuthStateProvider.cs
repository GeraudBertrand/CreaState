using CreaState.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace CreaState.Services
{
    /// <summary>
    /// AuthenticationStateProvider custom pour Blazor Server.
    /// Stocke le membre connecté dans un champ scoped (durée du circuit Blazor).
    /// </summary>
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private Member? _currentMember;

        private static readonly AuthenticationState Anonymous =
            new(new ClaimsPrincipal(new ClaimsIdentity()));

        public Member? CurrentMember => _currentMember;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_currentMember == null)
                return Task.FromResult(Anonymous);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, _currentMember.Id.ToString()),
                new(ClaimTypes.Name, _currentMember.FullName),
                new(ClaimTypes.Email, _currentMember.Email),
                new(ClaimTypes.Role, _currentMember.Role?.Name ?? "Eleve")
            };

            // Ajouter les permissions comme claims
            if (_currentMember.Role?.RolePermissions != null)
            {
                foreach (var rp in _currentMember.Role.RolePermissions)
                {
                    claims.Add(new Claim("Permission", rp.Permission.Code));
                }
            }

            var identity = new ClaimsIdentity(claims, "CreaState.Auth");
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(principal));
        }

        public void Login(Member member)
        {
            _currentMember = member;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void Logout()
        {
            _currentMember = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        /// <summary>
        /// Vérifie si l'utilisateur connecté a une permission spécifique.
        /// </summary>
        public bool HasPermission(string permissionCode)
        {
            return _currentMember?.Role?.RolePermissions?
                .Any(rp => rp.Permission.Code == permissionCode) ?? false;
        }
    }
}
