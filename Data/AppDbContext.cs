using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Membre> Membres => Set<Membre>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<MembreRole> MembreRoles => Set<MembreRole>();
        public DbSet<Printer> Printers => Set<Printer>();
        public DbSet<Consommable> Consommables => Set<Consommable>();
        public DbSet<Formation> Formations => Set<Formation>();
        public DbSet<Evenement> Evenements => Set<Evenement>();
        public DbSet<Maintenance> Maintenances => Set<Maintenance>();
        public DbSet<Requete> Requetes => Set<Requete>();
        public DbSet<RequeteFichier> RequeteFichiers => Set<RequeteFichier>();
        public DbSet<RequeteCommentaire> RequeteCommentaires => Set<RequeteCommentaire>();
        public DbSet<PrintJob> PrintJobs => Set<PrintJob>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === User / Membre : TPH avec discriminateur UserType ===
            modelBuilder.Entity<User>()
                .HasDiscriminator(u => u.UserType)
                .HasValue<User>(UserType.Eleve)
                .HasValue<Membre>(UserType.Membre);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // === Role & Permission ===
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.Name)
                .IsUnique();

            modelBuilder.Entity<Permission>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // === Membre <-> Role (N:N via MembreRole) ===
            modelBuilder.Entity<MembreRole>()
                .HasKey(mr => new { mr.MembreId, mr.RoleId });

            modelBuilder.Entity<MembreRole>()
                .HasOne(mr => mr.Membre)
                .WithMany(m => m.MembreRoles)
                .HasForeignKey(mr => mr.MembreId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MembreRole>()
                .HasOne(mr => mr.Role)
                .WithMany(r => r.MembreRoles)
                .HasForeignKey(mr => mr.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Formation -> Membre (Instructeur) ===
            modelBuilder.Entity<Formation>()
                .HasOne(f => f.Instructeur)
                .WithMany()
                .HasForeignKey(f => f.InstructeurId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Maintenance -> Printer, Membre ===
            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Printer)
                .WithMany(p => p.Maintenances)
                .HasForeignKey(m => m.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Maintenance>()
                .HasOne(m => m.Worker)
                .WithMany()
                .HasForeignKey(m => m.WorkerId)
                .OnDelete(DeleteBehavior.Restrict);

            // === Requete -> User (Demandeur), Membre (Assigné) ===
            modelBuilder.Entity<Requete>()
                .HasOne(r => r.Demandeur)
                .WithMany()
                .HasForeignKey(r => r.DemandeurId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Requete>()
                .HasOne(r => r.Assigne)
                .WithMany()
                .HasForeignKey(r => r.AssigneId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Requete>()
                .HasIndex(r => r.Status);

            modelBuilder.Entity<Requete>()
                .HasIndex(r => r.DemandeurId);

            // === RequeteFichier -> Requete ===
            modelBuilder.Entity<RequeteFichier>()
                .HasOne(rf => rf.Requete)
                .WithMany(r => r.Fichiers)
                .HasForeignKey(rf => rf.RequeteId)
                .OnDelete(DeleteBehavior.Cascade);

            // === RequeteCommentaire -> Requete, User ===
            modelBuilder.Entity<RequeteCommentaire>()
                .HasOne(rc => rc.Requete)
                .WithMany(r => r.Commentaires)
                .HasForeignKey(rc => rc.RequeteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RequeteCommentaire>()
                .HasOne(rc => rc.Auteur)
                .WithMany()
                .HasForeignKey(rc => rc.AuteurId)
                .OnDelete(DeleteBehavior.Restrict);

            // === PrintJob -> Printer, Requete ===
            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.Printer)
                .WithMany(p => p.PrintJobs)
                .HasForeignKey(pj => pj.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.Requete)
                .WithMany(r => r.PrintJobs)
                .HasForeignKey(pj => pj.RequeteId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
