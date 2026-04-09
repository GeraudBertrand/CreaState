using CreaState.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Data
{
    public class AppDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Membre> Membres => Set<Membre>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Printer> Printers => Set<Printer>();
        public DbSet<Consommable> Consommables => Set<Consommable>();
        public DbSet<Formation> Formations => Set<Formation>();
        public DbSet<Evenement> Evenements => Set<Evenement>();
        public DbSet<Maintenance> Maintenances => Set<Maintenance>();
        public DbSet<Requete> Requetes => Set<Requete>();
        public DbSet<RequeteFichier> RequeteFichiers => Set<RequeteFichier>();
        public DbSet<RequeteCommentaire> RequeteCommentaires => Set<RequeteCommentaire>();
        public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // === Renommer les tables Identity pour rester propre ===
            builder.Entity<User>().ToTable("Users");
            builder.Entity<Role>().ToTable("Roles");
            builder.Entity<AppUserRole>().ToTable("UserRoles");
            builder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
            builder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
            builder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");
            builder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");

            // === User / Membre : TPH avec discriminateur UserType ===
            builder.Entity<User>()
                .HasDiscriminator(u => u.UserType)
                .HasValue<User>(UserType.Eleve)
                .HasValue<Membre>(UserType.Membre);

            // === AppUserRole : navigations bidirectionnelles ===
            builder.Entity<AppUserRole>(b =>
            {
                b.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // === Permission ===
            builder.Entity<Permission>()
                .HasIndex(p => p.Code)
                .IsUnique();

            builder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Formation -> Membre (Instructeur) ===
            builder.Entity<Formation>()
                .HasOne(f => f.Instructeur)
                .WithMany()
                .HasForeignKey(f => f.InstructeurId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Maintenance -> Printer, Membre ===
            builder.Entity<Maintenance>()
                .HasOne(m => m.Printer)
                .WithMany(p => p.Maintenances)
                .HasForeignKey(m => m.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Maintenance>()
                .HasOne(m => m.Worker)
                .WithMany()
                .HasForeignKey(m => m.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Requete -> User (Demandeur), Membre (Assigné) ===
            builder.Entity<Requete>()
                .HasOne(r => r.Demandeur)
                .WithMany()
                .HasForeignKey(r => r.DemandeurId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Requete>()
                .HasOne(r => r.Assigne)
                .WithMany()
                .HasForeignKey(r => r.AssigneId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Requete>().HasIndex(r => r.Status);
            builder.Entity<Requete>().HasIndex(r => r.DemandeurId);

            // === RequeteFichier -> Requete ===
            builder.Entity<RequeteFichier>()
                .HasOne(rf => rf.Requete)
                .WithMany(r => r.Fichiers)
                .HasForeignKey(rf => rf.RequeteId)
                .OnDelete(DeleteBehavior.Cascade);

            // === RequeteCommentaire -> Requete, User ===
            builder.Entity<RequeteCommentaire>()
                .HasOne(rc => rc.Requete)
                .WithMany(r => r.Commentaires)
                .HasForeignKey(rc => rc.RequeteId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RequeteCommentaire>()
                .HasOne(rc => rc.Auteur)
                .WithMany()
                .HasForeignKey(rc => rc.AuteurId)
                .OnDelete(DeleteBehavior.Restrict);

            // === PrintJob -> Printer, Requete ===
            builder.Entity<PrintJob>()
                .HasOne(pj => pj.Printer)
                .WithMany(p => p.PrintJobs)
                .HasForeignKey(pj => pj.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PrintJob>()
                .HasOne(pj => pj.Requete)
                .WithMany(r => r.PrintJobs)
                .HasForeignKey(pj => pj.RequeteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
