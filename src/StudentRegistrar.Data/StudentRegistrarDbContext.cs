using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Models;

namespace StudentRegistrar.Data;

public class StudentRegistrarDbContext : DbContext
{
    public StudentRegistrarDbContext(DbContextOptions<StudentRegistrarDbContext> options) : base(options)
    {
    }

    // Current entities
    public DbSet<Student> Students { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<GradeRecord> GradeRecords { get; set; }
    public DbSet<AcademicYear> AcademicYears { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AccountHolder> AccountHolders { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<CourseInstructor> CourseInstructors { get; set; }
    public DbSet<Educator> Educators { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<Room> Rooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Current Entities
        
        // Configure AccountHolder
        modelBuilder.Entity<AccountHolder>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
            entity.Property(e => e.HomePhone).HasMaxLength(20);
            entity.Property(e => e.MobilePhone).HasMaxLength(20);
            entity.Property(e => e.AddressJson).HasColumnType("jsonb");
            entity.Property(e => e.EmergencyContactJson).HasColumnType("jsonb");
            entity.Property(e => e.MembershipDuesOwed).HasPrecision(10, 2);
            entity.Property(e => e.MembershipDuesReceived).HasPrecision(10, 2);
            entity.Property(e => e.KeycloakUserId).IsRequired().HasMaxLength(255);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.EmailAddress).IsUnique();
            entity.HasIndex(e => e.KeycloakUserId).IsUnique();
        });

        // Configure Semester
        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.RegistrationStartDate).IsRequired();
            entity.Property(e => e.RegistrationEndDate).IsRequired();
            entity.Property(e => e.PeriodConfigJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Configure Student
        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("Students");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Grade).HasMaxLength(20);
            entity.Property(e => e.StudentInfoJson).HasColumnType("jsonb");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.AccountHolder)
                .WithMany(a => a.Students)
                .HasForeignKey(e => e.AccountHolderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Course
        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("Courses");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.MaxCapacity).IsRequired();
            entity.Property(e => e.Fee).HasPrecision(10, 2);
            entity.Property(e => e.PeriodCode).HasMaxLength(50);
            entity.Property(e => e.CourseConfigJson).HasColumnType("jsonb");
            entity.Property(e => e.AgeGroup).IsRequired().HasMaxLength(100);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Semester)
                .WithMany(s => s.Courses)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Courses)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.RoomType).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure CourseInstructor
        modelBuilder.Entity<CourseInstructor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.InstructorInfoJson).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Course)
                .WithMany(c => c.CourseInstructors)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Enrollment
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("Enrollments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EnrollmentType).IsRequired();
            entity.Property(e => e.EnrollmentDate).IsRequired();
            entity.Property(e => e.FeeAmount).HasPrecision(10, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(10, 2);
            entity.Property(e => e.PaymentStatus).IsRequired();
            entity.Property(e => e.EnrollmentInfoJson).HasColumnType("jsonb");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Semester)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.SemesterId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.StudentId, e.CourseId, e.SemesterId }).IsUnique();
        });

        // Configure Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.PaymentDate).IsRequired();
            entity.Property(e => e.PaymentMethod).IsRequired();
            entity.Property(e => e.PaymentType).IsRequired();
            entity.Property(e => e.TransactionId).HasMaxLength(255);
            entity.Property(e => e.PaymentInfoJson).HasColumnType("jsonb");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(e => e.AccountHolder)
                .WithMany(a => a.Payments)
                .HasForeignKey(e => e.AccountHolderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Enrollment)
                .WithMany(en => en.Payments)
                .HasForeignKey(e => e.EnrollmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure GradeRecord
        modelBuilder.Entity<GradeRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LetterGrade).HasMaxLength(10);
            entity.Property(e => e.NumericGrade).HasPrecision(5, 2);
            entity.Property(e => e.GradePoints).HasPrecision(3, 2);
            entity.Property(e => e.Comments).HasMaxLength(500);
            entity.Property(e => e.GradeDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasOne(g => g.Student)
                .WithMany()
                .HasForeignKey(g => g.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(g => g.Course)
                .WithMany()
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AcademicYear
        modelBuilder.Entity<AcademicYear>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(20);
            entity.Property(e => e.StartDate).IsRequired();
            entity.Property(e => e.EndDate).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(320);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.KeycloakId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.KeycloakId).IsUnique();
        });

        // Configure UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(50);
            entity.Property(e => e.ZipCode).HasMaxLength(10);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.Bio).HasMaxLength(1000);
            entity.Property(e => e.ProfilePictureUrl).HasMaxLength(500);

            entity.HasOne(p => p.User)
                .WithOne()
                .HasForeignKey<UserProfile>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is GradeRecord || e.Entity is AcademicYear || e.Entity is User ||
                       e.Entity is AccountHolder || e.Entity is Semester || e.Entity is Student ||
                       e.Entity is Course || e.Entity is Enrollment || e.Entity is CourseInstructor ||
                       e.Entity is Educator || e.Entity is Payment || e.Entity is Room)
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
            }
            entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
        }
    }
}
