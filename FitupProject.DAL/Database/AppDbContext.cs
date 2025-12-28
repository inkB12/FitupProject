using FitupProject.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace FitupProject.DAL.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<OnboardingProfile> OnboardingProfiles { get; set; }
        public DbSet<WorkoutPlan> WorkoutPlans { get; set; }
        public DbSet<WorkoutSchedule> WorkoutSchedules { get; set; }
        public DbSet<WorkoutSession> WorkoutSessions { get; set; }
        public DbSet<WorkoutSessionExercise> WorkoutSessionExercises { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<WorkoutType> WorkoutTypes { get; set; }

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

            // OnboardingProfiles
            modelBuilder.Entity<OnboardingProfile>()
                .HasOne(x => x.Account)
                .WithMany(a => a.OnboardingProfiles)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OnboardingProfile>()
                .HasIndex(x => x.AccountId);

            // WorkoutPlans
            modelBuilder.Entity<WorkoutPlan>()
                .HasOne(x => x.Account)
                .WithMany(a => a.WorkoutPlans)
                .HasForeignKey(x => x.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutPlan>()
                .HasOne(x => x.OnboardingProfile)
                .WithMany(o => o.WorkoutPlans)
                .HasForeignKey(x => x.OnboardingProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<WorkoutPlan>()
                .HasIndex(x => x.AccountId);

            // WorkoutSchedule (Week)
            modelBuilder.Entity<WorkoutSchedule>()
                .HasOne(x => x.WorkoutPlan)
                .WithMany(p => p.WorkoutSchedules)
                .HasForeignKey(x => x.WorkoutPlanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutSchedule>()
                .HasIndex(x => new { x.WorkoutPlanId, x.WeekNumber })
                .IsUnique();

            // WorkoutSessions (Day)
            modelBuilder.Entity<WorkoutSession>()
                .HasOne(x => x.WorkoutSchedule)
                .WithMany(s => s.WorkoutSessions)
                .HasForeignKey(x => x.WorkoutScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutSession>()
                .HasIndex(x => new { x.WorkoutScheduleId, x.DayNumber })
                .IsUnique();

            // WorkoutSessionExercises
            modelBuilder.Entity<WorkoutSessionExercise>()
                .HasOne(x => x.WorkoutSession)
                .WithMany(s => s.WorkoutSessionExercises)
                .HasForeignKey(x => x.WorkoutSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<WorkoutSessionExercise>()
                .HasOne(x => x.Workout)
                .WithMany(w => w.WorkoutSessionExercises)
                .HasForeignKey(x => x.WorkoutId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WorkoutSessionExercise>()
                .HasIndex(x => new { x.WorkoutSessionId, x.Order })
                .IsUnique();

            // WorkoutType
            modelBuilder.Entity<WorkoutType>()
                .HasMany(t => t.Workouts)
                .WithOne(w => w.WorkoutType)
                .HasForeignKey(w => w.WorkoutTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Workouts 
            modelBuilder.Entity<Workout>()
                .HasIndex(x => x.WorkoutTypeId);
        }
    }
}
