using CreaState.Models;
using Microsoft.EntityFrameworkCore;

namespace CreaState.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Printer> Printers => Set<Printer>();
        public DbSet<PrintJob> PrintJobs => Set<PrintJob>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Formation> Formations => Set<Formation>();
        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<Request> Requests => Set<Request>();
        public DbSet<RequestFile> RequestFiles => Set<RequestFile>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // TPH : User/Member dans une seule table avec discriminateur
            modelBuilder.Entity<User>()
                .HasDiscriminator<string>("UserType")
                .HasValue<User>("User")
                .HasValue<Member>("Member");

            // Index unique sur l'email
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

            // Member -> Role
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Role)
                .WithMany()
                .HasForeignKey(m => m.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Printer : clé string
            modelBuilder.Entity<Printer>()
                .HasKey(p => p.Id);

            // PrintJob relations
            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.Printer)
                .WithMany(p => p.PrintJobs)
                .HasForeignKey(pj => pj.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.RequestedBy)
                .WithMany()
                .HasForeignKey(pj => pj.RequestedByUserId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<PrintJob>()
                .HasOne(pj => pj.Request)
                .WithMany(r => r.PrintJobs)
                .HasForeignKey(pj => pj.RequestId)
                .OnDelete(DeleteBehavior.SetNull);

            // Request relations
            modelBuilder.Entity<Request>()
                .HasOne(r => r.RequestedBy)
                .WithMany()
                .HasForeignKey(r => r.RequestedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.AssignedTo)
                .WithMany()
                .HasForeignKey(r => r.AssignedToMemberId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Request>()
                .HasOne(r => r.Printer)
                .WithMany()
                .HasForeignKey(r => r.PrinterId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.Status);

            modelBuilder.Entity<Request>()
                .HasIndex(r => r.RequestedByMemberId);

            // RequestFile relations
            modelBuilder.Entity<RequestFile>()
                .HasOne(rf => rf.Request)
                .WithMany(r => r.Files)
                .HasForeignKey(rf => rf.RequestId)
                .OnDelete(DeleteBehavior.Cascade);

            // MaintenanceRecord relations
            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.Printer)
                .WithMany(p => p.MaintenanceRecords)
                .HasForeignKey(m => m.PrinterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.PerformedBy)
                .WithMany()
                .HasForeignKey(m => m.PerformedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MaintenanceRecord>()
                .HasOne(m => m.ResolvedBy)
                .WithMany()
                .HasForeignKey(m => m.ResolvedByMemberId)
                .OnDelete(DeleteBehavior.Restrict);

            // Event, Formation, Announcement -> CreatedBy
            modelBuilder.Entity<Event>()
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Formation>()
                .HasOne(f => f.CreatedBy)
                .WithMany()
                .HasForeignKey(f => f.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
