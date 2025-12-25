using FitupProject.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.DAL.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Account
            modelBuilder.Entity<Account>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique();

                e.Property(x => x.PointAmount).HasPrecision(18, 2);

                e.Property(x => x.Status).HasConversion<string>();
                e.Property(x => x.Role).HasConversion<string>();

                e.Property(x => x.EmailOtpHash).HasMaxLength(256);
            });

            // UserProfile
            modelBuilder.Entity<UserProfile>(e =>
            {
                e.HasKey(x => x.AccountId);

                e.HasOne(x => x.Account)
                 .WithOne(a => a.UserProfile)
                 .HasForeignKey<UserProfile>(x => x.AccountId)
                 .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
