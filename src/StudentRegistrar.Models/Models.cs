using System.ComponentModel.DataAnnotations;

namespace StudentRegistrar.Models;

// Legacy models kept for backward compatibility during migration
// Will be removed in Phase 2 of the refactor

public class LegacyStudent
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public DateOnly DateOfBirth { get; set; }
    
    [StringLength(20)]
    public string? PhoneNumber { get; set; }
    
    [StringLength(500)]
    public string? Address { get; set; }
    
    [StringLength(100)]
    public string? City { get; set; }
    
    [StringLength(10)]
    public string? State { get; set; }
    
    [StringLength(10)]
    public string? ZipCode { get; set; }
    
    [StringLength(100)]
    public string? EmergencyContactName { get; set; }
    
    [StringLength(20)]
    public string? EmergencyContactPhone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Link to user account (optional - students might not have accounts)
    public Guid? UserId { get; set; }
    public virtual User? User { get; set; }
    
    // Navigation properties
    public virtual ICollection<LegacyEnrollment> Enrollments { get; set; } = new List<LegacyEnrollment>();
    public virtual ICollection<GradeRecord> GradeRecords { get; set; } = new List<GradeRecord>();
}

public class LegacyCourse
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public int CreditHours { get; set; }
    
    // Link to educator who created/manages the course
    public Guid? CreatedByUserId { get; set; }
    public virtual User? CreatedByUser { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = string.Empty;
    
    [Required]
    public string AcademicYear { get; set; } = string.Empty;
    
    [Required]
    public string Semester { get; set; } = string.Empty; // Fall, Spring, Summer
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<LegacyEnrollment> Enrollments { get; set; } = new List<LegacyEnrollment>();
    public virtual ICollection<GradeRecord> GradeRecords { get; set; } = new List<GradeRecord>();
}

public class LegacyEnrollment
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public LegacyStudent Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public LegacyCourse Course { get; set; } = null!;
    
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    
    public string Status { get; set; } = "Active"; // Active, Completed, Dropped, Withdrawn
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class GradeRecord
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public LegacyStudent Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public LegacyCourse Course { get; set; } = null!;
    
    [StringLength(10)]
    public string? LetterGrade { get; set; } // A, B, C, D, F
    
    public decimal? NumericGrade { get; set; } // 0-100
    
    public decimal? GradePoints { get; set; } // 0-4.0
    
    [StringLength(500)]
    public string? Comments { get; set; }
    
    public DateTime GradeDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AcademicYear
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(20)]
    public string Name { get; set; } = string.Empty; // e.g., "2024-2025"
    
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    
    public bool IsActive { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// User Management Models
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string KeycloakId { get; set; } = string.Empty; // Links to Keycloak user
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public virtual ICollection<LegacyStudent> Students { get; set; } = new List<LegacyStudent>();
    public virtual ICollection<LegacyCourse> CoursesCreated { get; set; } = new List<LegacyCourse>();
    public virtual UserProfile? UserProfile { get; set; }
}

public enum UserRole
{
    Student = 0,
    Parent = 1,
    Educator = 2,
    Administrator = 3
}

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; } = "US";
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}

// ...existing code...