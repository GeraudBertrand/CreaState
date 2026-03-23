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
            };

            // Ajouter un claim Role par rôle
            foreach (var mr in _currentMember.MemberRoles)
                claims.Add(new(ClaimTypes.Role, mr.Role?.Name ?? ""));

            // Ajouter les permissions (union de tous les rôles) comme claims
            var allPermissions = _currentMember.MemberRoles
                .SelectMany(mr => mr.Role?.RolePermissions ?? [])
                .Select(rp => rp.Permission.Code)
                .Distinct();
            foreach (var perm in allPermissions)
                claims.Add(new Claim("Permission", perm));

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
            return _currentMember?.MemberRoles?
                .SelectMany(mr => mr.Role?.RolePermissions ?? [])
                .Any(rp => rp.Permission.Code == permissionCode) ?? false;
        }
    }
}
