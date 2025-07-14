using System.ComponentModel.DataAnnotations;

namespace StudentRegistrar.Models;

public class Student
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
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<GradeRecord> GradeRecords { get; set; } = new List<GradeRecord>();
}

public class Course
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
    
    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = string.Empty;
    
    [Required]
    public string AcademicYear { get; set; } = string.Empty;
    
    [Required]
    public string Semester { get; set; } = string.Empty; // Fall, Spring, Summer
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<GradeRecord> GradeRecords { get; set; } = new List<GradeRecord>();
}

public class Enrollment
{
    public int Id { get; set; }
    
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
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
    public Student Student { get; set; } = null!;
    
    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
    
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
