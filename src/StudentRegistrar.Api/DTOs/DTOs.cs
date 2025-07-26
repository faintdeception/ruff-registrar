using System.ComponentModel.DataAnnotations;
using StudentRegistrar.Models;

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

// User Management DTOs
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    public UserRole Role { get; set; }
    
    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; set; } = string.Empty;
    
    public UserProfileDto? Profile { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public UserRole Role { get; set; }
    public string RoleDisplay => Role.ToString();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public UserProfileDto? Profile { get; set; }
    
    // Additional properties for authentication
    public string Username { get; set; } = string.Empty;
    public string KeycloakId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

public class UpdateUserRequest
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
    public UserProfileDto? Profile { get; set; }
}

public class UserProfileDto
{
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class AuthUserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string KeycloakId { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

// Course Instructor DTOs
public class CourseInstructorDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public InstructorInfo InstructorInfo { get; set; } = new();
    public CourseDto? Course { get; set; }
}

public class CreateCourseInstructorDto
{
    [Required]
    public Guid CourseId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    public InstructorInfo? InstructorInfo { get; set; }
}

public class UpdateCourseInstructorDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    
    public InstructorInfo? InstructorInfo { get; set; }
}

public class InstructorInfo
{
    public string? Bio { get; set; }
    public List<string> Qualifications { get; set; } = new();
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

// Independent Educator DTOs (replaces CourseInstructor system)
public class EducatorDto
{
    public Guid Id { get; set; }
    public Guid? CourseId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public EducatorInfo EducatorInfo { get; set; } = new();
    public bool IsAssignedToCourse => CourseId.HasValue;
    public CourseDto? Course { get; set; }
}

public class CreateEducatorDto
{
    public Guid? CourseId { get; set; } // Optional - can create educators without courses
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public EducatorInfo? EducatorInfo { get; set; }
}

public class UpdateEducatorDto
{
    public Guid? CourseId { get; set; } // Can assign/unassign courses
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }
    
    [Phone]
    [StringLength(20)]
    public string? Phone { get; set; }
    
    public bool IsPrimary { get; set; } = false;
    public bool IsActive { get; set; } = true;
    
    public EducatorInfo? EducatorInfo { get; set; }
}

public class EducatorInfo
{
    public string? Bio { get; set; }
    public List<string> Qualifications { get; set; } = new();
    public List<string> Specializations { get; set; } = new();
    public string? Department { get; set; }
    public Dictionary<string, string> CustomFields { get; set; } = new();
}

// AccountHolder DTOs for the frontend
public class AccountHolderDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string? HomePhone { get; set; }
    public string? MobilePhone { get; set; }
    public AddressInfo AddressJson { get; set; } = new();
    public EmergencyContactInfo EmergencyContactJson { get; set; } = new();
    public decimal MembershipDuesOwed { get; set; }
    public decimal MembershipDuesReceived { get; set; }
    public DateTime MemberSince { get; set; }
    public DateTime? LastLogin { get; set; }
    public DateTime LastEdit { get; set; }
    public List<StudentDetailDto> Students { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class CreateAccountHolderDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string EmailAddress { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? HomePhone { get; set; }
    
    [StringLength(20)]
    public string? MobilePhone { get; set; }
    
    public AddressInfo? AddressJson { get; set; }
    public EmergencyContactInfo? EmergencyContactJson { get; set; }
}

public class UpdateAccountHolderDto
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    
    [StringLength(100)]
    public string? LastName { get; set; }
    
    [EmailAddress]
    [StringLength(255)]
    public string? EmailAddress { get; set; }
    
    [StringLength(20)]
    public string? HomePhone { get; set; }
    
    [StringLength(20)]
    public string? MobilePhone { get; set; }
    
    public AddressInfo? AddressJson { get; set; }
    public EmergencyContactInfo? EmergencyContactJson { get; set; }
}

public class StudentDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Grade { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public StudentInfoDetails StudentInfoJson { get; set; } = new();
    public string? Notes { get; set; }
    public List<EnrollmentDetailDto> Enrollments { get; set; } = new();
}

public class CreateStudentForAccountDto
{
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    
    [StringLength(20)]
    public string? Grade { get; set; }
    
    public DateTime? DateOfBirth { get; set; }
    public StudentInfoDetails? StudentInfoJson { get; set; }
    public string? Notes { get; set; }
}

public class EnrollmentDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string? CourseCode { get; set; }
    public string SemesterName { get; set; } = string.Empty;
    public string EnrollmentType { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public decimal FeeAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public int? WaitlistPosition { get; set; }
    public string? Notes { get; set; }
}

public class PaymentDto
{
    public string Id { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime PaymentDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
}

public class AddressInfo
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

public class EmergencyContactInfo
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? HomePhone { get; set; }
    public string? MobilePhone { get; set; }
    public string Email { get; set; } = string.Empty;
}

public class StudentInfoDetails
{
    public List<string> SpecialConditions { get; set; } = new();
    public List<string> Allergies { get; set; } = new();
    public List<string> Medications { get; set; } = new();
    public string? PreferredName { get; set; }
    public string? ParentNotes { get; set; }
}

// New Course System DTOs

public class SemesterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsRegistrationOpen { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<NewCourseDto> Courses { get; set; } = new();
}

public class CreateSemesterDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateSemesterDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime RegistrationStartDate { get; set; }
    public DateTime RegistrationEndDate { get; set; }
    public bool IsActive { get; set; }
}

public class NewCourseDto
{
    public Guid Id { get; set; }
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Room { get; set; }
    public int MaxCapacity { get; set; }
    public decimal Fee { get; set; }
    public string? PeriodCode { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? TimeSlot { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
    public int CurrentEnrollment { get; set; }
    public int AvailableSpots { get; set; }
    public bool IsFull { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public SemesterDto? Semester { get; set; }
    public List<CourseInstructorDto> Instructors { get; set; } = new();
    public List<string> InstructorNames { get; set; } = new(); // Add instructor names for frontend compatibility
}

public class CreateNewCourseDto
{
    public Guid SemesterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Room { get; set; }
    public int MaxCapacity { get; set; }
    public decimal Fee { get; set; } = 0;
    public string? PeriodCode { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
}

public class UpdateNewCourseDto
{
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string? Description { get; set; }
    public string? Room { get; set; }
    public int MaxCapacity { get; set; }
    public decimal Fee { get; set; }
    public string? PeriodCode { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string AgeGroup { get; set; } = string.Empty;
}
