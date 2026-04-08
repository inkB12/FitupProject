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
        public DbSet<PT> PTs { get; set; }
        public DbSet<Slot> Slots { get; set; }
        public DbSet<SlotForBooking> SlotForBookings { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingReview> BookingReviews { get; set; }
        public DbSet<ConversionRate> ConversionRates { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PremiumType> PremiumTypes { get; set; }
        public DbSet<Premium> Premiums { get; set; }
        public DbSet<ServicePayment> ServicePayments { get; set; }
        public DbSet<BookingPayment> BookingPayments { get; set; }
        public DbSet<PremiumPayment> PremiumPayments { get; set; }
        public DbSet<PTCertificationFile> PTCertificationFiles { get; set; }
        public DbSet<PTReviewLog> PTReviewLogs { get; set; }

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

            modelBuilder.Entity<PT>(e =>
            {
                e.HasIndex(x => x.AccountId).IsUnique();

                e.Property(x => x.PricePerHour).HasPrecision(18, 2);
                e.Property(x => x.Rating).HasPrecision(18, 2);
                e.Property(x => x.VerificationStatus).HasConversion<string>();
                e.Property(x => x.CertificationsJson).HasDefaultValue("[]");
                e.Property(x => x.SpecialtiesJson).HasDefaultValue("[]");
                e.Property(x => x.LanguagesJson).HasDefaultValue("[]");

                e.HasOne(x => x.Account)
                 .WithOne(a => a.PT) // nếu bạn CHƯA add a.PT thì đổi thành .WithMany() và bỏ unique index
                 .HasForeignKey<PT>(x => x.AccountId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Slot (PT 1-n Slot)
            modelBuilder.Entity<Slot>(e =>
            {
                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<string>();

                e.HasOne(x => x.PT)
                 .WithMany(p => p.Slots)
                 .HasForeignKey(x => x.PTId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.PTId);
            });

            // SlotForBooking (Slot 1-n SlotForBooking)
            modelBuilder.Entity<SlotForBooking>(e =>
            {
                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<string>();

                e.HasOne(x => x.Slot)
                 .WithMany(s => s.SlotForBookings)
                 .HasForeignKey(x => x.SlotId)
                 .OnDelete(DeleteBehavior.Cascade);

                // tránh trùng 1 slot + 1 ngày
                e.HasIndex(x => new { x.SlotId, x.BookingDate }).IsUnique();
            });

            // Booking (Account 1-n Booking) + (SlotForBooking 1-n Booking)
            // Nếu bạn muốn SlotForBooking chỉ có đúng 1 booking => mình sẽ đổi sang 1-1 + unique index
            modelBuilder.Entity<Booking>(e =>
            {
                e.Property(x => x.Total).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<string>();

                e.HasOne(x => x.Account)
                 .WithMany(a => a.Bookings)
                 .HasForeignKey(x => x.AccountId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.SlotForBooking)
                 .WithMany(sfb => sfb.Bookings)
                 .HasForeignKey(x => x.SlotForBookingId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.AccountId);
                e.HasIndex(x => x.SlotForBookingId);
            });

            // BookingReview (Booking 1-0..1 Review)
            modelBuilder.Entity<BookingReview>(e =>
            {
                e.Property(x => x.Status).HasConversion<string>();

                e.HasOne(x => x.Booking)
                 .WithOne(b => b.BookingReview)
                 .HasForeignKey<BookingReview>(x => x.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.BookingId).IsUnique();
            });

            // ConversionRate
            modelBuilder.Entity<ConversionRate>(e =>
            {
                e.Property(x => x.Rate).HasPrecision(18, 6);
                e.Property(x => x.Status).HasConversion<string>();
                e.HasIndex(x => x.Type).IsUnique();
            });

            // Payment (top-up tiền thật)
            modelBuilder.Entity<Payment>(e =>
            {
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<string>();


                e.HasOne(x => x.Account)
                 .WithMany(a => a.Payments)
                 .HasForeignKey(x => x.AccountId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.ConversionRate)
                 .WithMany(cr => cr.Payments)
                 .HasForeignKey(x => x.ConversionRateId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // PremiumType
            modelBuilder.Entity<PremiumType>(e =>
            {
                e.Property(x => x.Price).HasPrecision(18, 2);
                e.Property(x => x.Status).HasConversion<string>();
            });

            // Premium (Account 1-n Premium, PremiumType 1-n Premium)
            modelBuilder.Entity<Premium>(e =>
            {
                e.Property(x => x.Status).HasConversion<string>();

                e.HasOne(x => x.Account)
                 .WithMany(a => a.Premiums)
                 .HasForeignKey(x => x.AccountId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(x => x.PremiumType)
                 .WithMany(t => t.Premiums)
                 .HasForeignKey(x => x.PremiumTypeId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.AccountId);
                e.HasIndex(x => x.PremiumTypeId);
            });

            // ServicePayment (trừ point)
            modelBuilder.Entity<ServicePayment>(e =>
            {
                e.Property(x => x.Amount).HasPrecision(18, 2);
                e.Property(x => x.ServiceType).HasConversion<string>();
                e.Property(x => x.Status).HasConversion<string>();
            });

            // BookingPayment
            modelBuilder.Entity<BookingPayment>(e =>
            {
                e.Property(x => x.Price).HasPrecision(18, 2);

                e.HasOne(x => x.Booking)
                 .WithMany(b => b.BookingPayments)
                 .HasForeignKey(x => x.BookingId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.ServicePayment)
                 .WithMany(sp => sp.BookingPayments)
                 .HasForeignKey(x => x.ServicePaymentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.BookingId);
                e.HasIndex(x => x.ServicePaymentId);
            });

            // PremiumPayment
            modelBuilder.Entity<PremiumPayment>(e =>
            {
                e.Property(x => x.Price).HasPrecision(18, 2);

                e.HasOne(x => x.Premium)
                 .WithMany(p => p.PremiumPayments)
                 .HasForeignKey(x => x.PremiumId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.ServicePayment)
                 .WithMany(sp => sp.PremiumPayments)
                 .HasForeignKey(x => x.ServicePaymentId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(x => x.PremiumId);
                e.HasIndex(x => x.ServicePaymentId);
            });

            // --- PTCertificationFile ---
            modelBuilder.Entity<PTCertificationFile>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.FileName).IsRequired().HasMaxLength(255);
                e.Property(x => x.FileUrl).IsRequired();

                // Quan hệ PT 1 - n PTCertificationFile
                e.HasOne(x => x.PT)
                 .WithMany(p => p.CertificationFiles)
                 .HasForeignKey(x => x.PTId)
                 .OnDelete(DeleteBehavior.Cascade); // Xóa PT thì xóa luôn file chứng chỉ

                e.HasIndex(x => x.PTId);
            });

            // --- PTReviewLog ---
            modelBuilder.Entity<PTReviewLog>(e =>
            {
                e.HasKey(x => x.Id);

                e.Property(x => x.Action).HasConversion<string>(); // Lưu Enum dưới dạng String
                e.Property(x => x.Reason).HasMaxLength(1000);

                // Quan hệ PT 1 - n PTReviewLog
                e.HasOne(x => x.PT)
                 .WithMany(p => p.ReviewLogs)
                 .HasForeignKey(x => x.PTId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(x => x.PTId);
            });
        }
    }
}
