using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            // === Seed des rôles ===
            if (!await context.Roles.AnyAsync())
            {
                var roles = new List<Role>
                {
                    new() { Name = "Eleve", DisplayName = "Élève", Description = "Étudiant inscrit (rôle par défaut)", IsDefault = true },
                    new() { Name = "Membre", DisplayName = "Membre", Description = "Membre actif de l'association" },
                    new() { Name = "AgentFab", DisplayName = "Agent Fab", Description = "Agent fablab, gère les demandes d'impression" },
                    new() { Name = "TechManager", DisplayName = "Resp. Technique", Description = "Responsable technique du fablab" },
                    new() { Name = "ComManager", DisplayName = "Resp. Communication", Description = "Responsable communication" },
                    new() { Name = "EventManager", DisplayName = "Resp. Événementiel", Description = "Responsable événementiel" },
                    new() { Name = "PartnershipManager", DisplayName = "Resp. Partenariat", Description = "Responsable partenariats" },
                    new() { Name = "Secretary", DisplayName = "Secrétaire", Description = "Secrétaire de l'association" },
                    new() { Name = "Treasurer", DisplayName = "Trésorier", Description = "Trésorier de l'association" },
                    new() { Name = "VicePresident", DisplayName = "Vice-Président", Description = "Vice-président de l'association" },
                    new() { Name = "President", DisplayName = "Président", Description = "Président de l'association" },
                };
                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // === Seed des permissions ===
            if (!await context.Permissions.AnyAsync())
            {
                var permissions = new List<Permission>
                {
                    new() { Code = "view_dashboard", DisplayName = "Voir le dashboard", Category = "Navigation" },
                    new() { Code = "view_printers", DisplayName = "Voir les imprimantes", Category = "Navigation" },
                    new() { Code = "access_private", DisplayName = "Accès espace privé", Category = "Navigation" },
                    new() { Code = "submit_requests", DisplayName = "Soumettre une demande", Category = "Demandes" },
                    new() { Code = "manage_requests", DisplayName = "Gérer les demandes", Category = "Demandes" },
                    new() { Code = "view_inventory", DisplayName = "Voir l'inventaire", Category = "Inventaire" },
                    new() { Code = "manage_inventory", DisplayName = "Gérer l'inventaire", Category = "Inventaire" },
                    new() { Code = "report_breakdown", DisplayName = "Signaler une panne", Category = "Maintenance" },
                    new() { Code = "manage_maintenance", DisplayName = "Gérer la maintenance", Category = "Maintenance" },
                    new() { Code = "view_members", DisplayName = "Voir les membres", Category = "Membres" },
                    new() { Code = "manage_members", DisplayName = "Gérer les membres/rôles", Category = "Membres" },
                    new() { Code = "admin_access", DisplayName = "Accès administration", Category = "Administration" },
                    new() { Code = "manage_printers", DisplayName = "Gérer les imprimantes", Category = "Administration" },
                };
                context.Permissions.AddRange(permissions);
                await context.SaveChangesAsync();
            }

            // === Seed de la matrice rôle <-> permission ===
            if (!await context.RolePermissions.AnyAsync())
            {
                var roles = await context.Roles.ToListAsync();
                var perms = await context.Permissions.ToListAsync();

                Role R(string name) => roles.First(r => r.Name == name);
                Permission P(string code) => perms.First(p => p.Code == code);

                var rolePermissions = new List<RolePermission>();

                void Add(string roleName, string permCode)
                    => rolePermissions.Add(new RolePermission { RoleId = R(roleName).Id, PermissionId = P(permCode).Id });

                void AddMatrix(string[] roleNames, string[] permCodes)
                {
                    foreach (var roleName in roleNames)
                        foreach (var permCode in permCodes)
                            Add(roleName, permCode);
                }

                var allRoleNames = roles.Select(r => r.Name).ToArray();
                var membreAndAbove = allRoleNames.Where(n => n != "Eleve").ToArray();
                var techManagerRoles = new[] { "TechManager" };
                var secretaryPlus = new[] { "Secretary", "Treasurer" };
                var bureauRoles = new[] { "VicePresident", "President" };

                AddMatrix(allRoleNames, new[] { "view_dashboard", "submit_requests" });
                AddMatrix(membreAndAbove, new[] { "access_private", "view_members", "report_breakdown" });
                Add("AgentFab", "manage_requests");
                AddMatrix(techManagerRoles, new[] { "view_printers", "manage_printers", "manage_maintenance" });
                AddMatrix(secretaryPlus, new[] { "view_printers", "manage_printers", "manage_maintenance", "view_inventory", "manage_inventory" });
                AddMatrix(bureauRoles, new[] {
                    "view_printers", "manage_printers", "manage_maintenance",
                    "view_inventory", "manage_inventory",
                    "manage_requests", "manage_members", "admin_access"
                });

                context.RolePermissions.AddRange(rolePermissions);
                await context.SaveChangesAsync();
            }

            // === Seed des imprimantes ===
            if (!await context.Printers.AnyAsync())
            {
                context.Printers.AddRange(
                    new Printer { Name = "Ratome", IpAddress = "10.3.212.16", Model = "A1 Mini", AccessCode = "26110863", SerialNumber = "0309DA3C3100431" },
                    new Printer { Name = "Bonnie", IpAddress = "10.3.212.10", Model = "A1 Mini", AccessCode = "87654321", SerialNumber = "0309DA3C3100060" },
                    new Printer { Name = "Hubble", IpAddress = "10.3.212.19", Model = "A1 Mini", AccessCode = "84466330", SerialNumber = "0309DA422200342" },
                    new Printer { Name = "R2-D2", IpAddress = "10.3.212.23", Model = "A1 Mini", AccessCode = "85255827", SerialNumber = "0309DA441501115" }
                );
            }

            // === Seed d'un membre admin ===
            if (!await context.Membres.AnyAsync())
            {
                var presidentRole = await context.Roles.FirstAsync(r => r.Name == "President");
                var admin = new Membre
                {
                    FirstName = "Admin",
                    LastName = "Créalab",
                    Email = "admin@edu.devinci.fr",
                    PasswordHash = null,
                    ClassYear = ClassYearEnum.A3,
                    UserType = UserType.Membre,
                    IsActive = true,
                    JoinDate = DateTime.UtcNow
                };
                context.Membres.Add(admin);
                await context.SaveChangesAsync();

                context.MembreRoles.Add(new MembreRole { MembreId = admin.Id, RoleId = presidentRole.Id });
            }

            await context.SaveChangesAsync();
        }
    }
}
