using CreaState.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Data
{
    public static class DbSeeder
    {
        /// <summary>
        /// Seeds the database with roles, permissions, role-permission matrix,
        /// printers, and an admin account.
        /// Uses UserManager/RoleManager for Identity-compatible seeding.
        /// </summary>
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<AppDbContext>();
            var userManager = services.GetRequiredService<UserManager<User>>();
            var roleManager = services.GetRequiredService<RoleManager<Role>>();

            await context.Database.MigrateAsync();

            await SeedRolesAsync(roleManager);
            await SeedPermissionsAsync(context);
            await SeedRolePermissionsAsync(context);
            await SeedPrintersAsync(context);
            await SeedAdminAsync(context, userManager);
            await SeedEleveAsync(userManager);

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<Role> roleManager)
        {
            var roles = new (string Name, string DisplayName, string Description, bool IsDefault)[]
            {
                ("Eleve", "Eleve", "Etudiant inscrit (role par defaut)", true),
                ("Membre", "Membre", "Membre actif de l'association", false),
                ("AgentFab", "Agent Fab", "Agent fablab, gere les demandes d'impression", false),
                ("TechManager", "Resp. Technique", "Responsable technique du fablab", false),
                ("NumManager", "Resp. Numerique", "Responsable numerique du fablab", false),
                ("ComManager", "Resp. Communication", "Responsable communication", false),
                ("EventManager", "Resp. Evenementiel", "Responsable evenementiel", false),
                ("PartnershipManager", "Resp. Partenariat", "Responsable partenariats", false),
                ("Secretary", "Secretaire", "Secretaire de l'association", false),
                ("Treasurer", "Tresorier", "Tresorier de l'association", false),
                ("VicePresident", "Vice-President", "Vice-president de l'association", false),
                ("President", "President", "President de l'association", false),
            };

            foreach (var (name, displayName, description, isDefault) in roles)
            {
                if (!await roleManager.RoleExistsAsync(name))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = name,
                        DisplayName = displayName,
                        Description = description,
                        IsDefault = isDefault
                    });
                }
            }
        }

        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync()) return;

            var permissions = new List<Permission>
            {
                new() { Code = "view_dashboard", DisplayName = "Voir le dashboard", Category = "Navigation" },
                new() { Code = "view_printers", DisplayName = "Voir les imprimantes", Category = "Navigation" },
                new() { Code = "access_private", DisplayName = "Acces espace prive", Category = "Navigation" },
                new() { Code = "submit_requests", DisplayName = "Soumettre une demande", Category = "Demandes" },
                new() { Code = "manage_requests", DisplayName = "Gerer les demandes", Category = "Demandes" },
                new() { Code = "view_inventory", DisplayName = "Voir l'inventaire", Category = "Inventaire" },
                new() { Code = "manage_inventory", DisplayName = "Gerer l'inventaire", Category = "Inventaire" },
                new() { Code = "report_breakdown", DisplayName = "Signaler une panne", Category = "Maintenance" },
                new() { Code = "manage_maintenance", DisplayName = "Gerer la maintenance", Category = "Maintenance" },
                new() { Code = "view_members", DisplayName = "Voir les membres", Category = "Membres" },
                new() { Code = "manage_members", DisplayName = "Gerer les membres/roles", Category = "Membres" },
                new() { Code = "admin_access", DisplayName = "Acces administration", Category = "Administration" },
                new() { Code = "manage_printers", DisplayName = "Gerer les imprimantes", Category = "Administration" },
            };
            context.Permissions.AddRange(permissions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedRolePermissionsAsync(AppDbContext context)
        {
            if (await context.RolePermissions.AnyAsync()) return;

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

            var allRoleNames = roles.Select(r => r.Name!).ToArray();
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

        private static async Task SeedPrintersAsync(AppDbContext context)
        {
            if (await context.Printers.AnyAsync()) return;

            context.Printers.AddRange(
                new Printer { Name = "Ratome", IpAddress = "10.3.212.16", Model = "A1 Mini", AccessCode = "26110863", SerialNumber = "0309DA3C3100431" },
                new Printer { Name = "Bonnie", IpAddress = "10.3.212.10", Model = "A1 Mini", AccessCode = "87654321", SerialNumber = "0309DA3C3100060" },
                new Printer { Name = "Hubble", IpAddress = "10.3.212.19", Model = "A1 Mini", AccessCode = "84466330", SerialNumber = "0309DA422200342" },
                new Printer { Name = "R2-D2", IpAddress = "10.3.212.23", Model = "A1 Mini", AccessCode = "85255827", SerialNumber = "0309DA441501115" }
            );
            await context.SaveChangesAsync();
        }

        private static async Task SeedAdminAsync(AppDbContext context, UserManager<User> userManager)
        {
            // ============================================================
            // !! MOT DE PASSE ADMIN PAR DEFAUT : Password123!
            // !! A CHANGER EN PRODUCTION !!
            // ============================================================
            const string adminEmail = "admin@edu.devinci.fr";
            const string adminPassword = "Password123!"; // <-- MOT DE PASSE ADMIN

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return; // Admin already exists

            var admin = new Membre
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true, // Admin is pre-confirmed
                FirstName = "Admin",
                LastName = "Crealab",
                ClassYear = ClassYearEnum.A3,
                UserType = UserType.Membre,
                IsActive = true,
                JoinDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                // Assign all bureau roles to admin
                await userManager.AddToRoleAsync(admin, "President");
                await userManager.AddToRoleAsync(admin, "Membre");
            }
        }

        private static async Task SeedEleveAsync(UserManager<User> userManager)
        {
            // ============================================================
            // !! ELEVE DE TEST : eleve@edu.devinci.fr / Password123!
            // !! Acces partie publique uniquement
            // ============================================================
            const string eleveEmail = "eleve@edu.devinci.fr";
            const string elevePassword = "Password123!";

            if (await userManager.FindByEmailAsync(eleveEmail) != null)
                return;

            var eleve = new User
            {
                UserName = eleveEmail,
                Email = eleveEmail,
                EmailConfirmed = true, // Pre-confirmed for testing
                FirstName = "Eleve",
                LastName = "Test",
                ClassYear = ClassYearEnum.A1,
                UserType = UserType.Eleve,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(eleve, elevePassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(eleve, "Eleve");
            }
        }
    }
}
