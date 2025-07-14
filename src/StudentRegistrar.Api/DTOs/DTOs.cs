using System.ComponentModel.DataAnnotations;

namespace StudentRegistrar.Api.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateStudentDto
{
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
}

public class UpdateStudentDto
{
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
}

public class CourseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CreditHours { get; set; }
    public string Instructor { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string Semester { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCourseDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int CreditHours { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = string.Empty;
    
    [Required]
    public string AcademicYear { get; set; } = string.Empty;
    
    [Required]
    public string Semester { get; set; } = string.Empty;
}

public class UpdateCourseDto
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [StringLength(20)]
    public string Code { get; set; } = string.Empty;
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int CreditHours { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Instructor { get; set; } = string.Empty;
    
    [Required]
    public string AcademicYear { get; set; } = string.Empty;
    
    [Required]
    public string Semester { get; set; } = string.Empty;
}

public class EnrollmentDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public StudentDto Student { get; set; } = null!;
    public int CourseId { get; set; }
    public CourseDto Course { get; set; } = null!;
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateEnrollmentDto
{
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int CourseId { get; set; }
    
    [Required]
    public DateTime EnrollmentDate { get; set; }
    
    [StringLength(20)]
    public string Status { get; set; } = "Active";
}

public class GradeRecordDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public StudentDto Student { get; set; } = null!;
    public int CourseId { get; set; }
    public CourseDto Course { get; set; } = null!;
    public string? LetterGrade { get; set; }
    public decimal? NumericGrade { get; set; }
    public decimal? GradePoints { get; set; }
    public string? Comments { get; set; }
    public DateTime GradeDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateGradeRecordDto
{
    [Required]
    public int StudentId { get; set; }
    
    [Required]
    public int CourseId { get; set; }
    
    [StringLength(10)]
    public string? LetterGrade { get; set; }
    
    [Range(0, 100)]
    public decimal? NumericGrade { get; set; }
    
    [Range(0, 4)]
    public decimal? GradePoints { get; set; }
    
    [StringLength(500)]
    public string? Comments { get; set; }
    
    [Required]
    public DateTime GradeDate { get; set; }
}
