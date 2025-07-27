using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class StudentServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly StudentService _service;
    
    // Test data references
    private readonly Guid _johnStudentId = Guid.NewGuid();
    private readonly Guid _aliceStudentId = Guid.NewGuid();
    private readonly Guid _accountHolderId = Guid.NewGuid();
    private readonly Guid _mathCourseId = Guid.NewGuid();
    private readonly Guid _englishCourseId = Guid.NewGuid();
    private readonly Guid _fallSemesterId = Guid.NewGuid();

    public StudentServiceTests()
    {
        var options = new DbContextOptionsBuilder<StudentRegistrarDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new StudentRegistrarDbContext(options);

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });
        _mapper = config.CreateMapper();

        _service = new StudentService(_context, _mapper);

        SeedTestData();
    }

    private void SeedTestData()
    {
        // Create test semester first
        var semester = new Semester
        {
            Id = _fallSemesterId,
            Name = "Fall 2024",
            Code = "FALL2024",
            StartDate = new DateTime(2024, 8, 15),
            EndDate = new DateTime(2024, 12, 15),
            RegistrationStartDate = new DateTime(2024, 7, 1),
            RegistrationEndDate = new DateTime(2024, 8, 10),
            IsActive = true
        };

        // Create test account holder
        var accountHolder = new AccountHolder
        {
            Id = _accountHolderId,
            FirstName = "Parent",
            LastName = "Guardian",
            EmailAddress = "parent@example.com",
            HomePhone = "555-123-4567",
            KeycloakUserId = "test-keycloak-id"
        };
        
        // Set address using helper method
        accountHolder.SetAddress(new Address
        {
            Street = "123 Parent St",
            City = "Parent City",
            State = "CA",
            PostalCode = "12345"
        });

        var students = new List<Student>
        {
            new Student
            {
                Id = _johnStudentId,
                FirstName = "John",
                LastName = "Doe",
                DateOfBirth = new DateTime(2000, 1, 15),
                AccountHolderId = accountHolder.Id,
                Grade = "12",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new Student
            {
                Id = _aliceStudentId,
                FirstName = "Alice",
                LastName = "Smith",
                DateOfBirth = new DateTime(1999, 5, 20),
                AccountHolderId = accountHolder.Id,
                Grade = "12",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            }
        };

        var courses = new List<Course>
        {
            new Course
            {
                Id = _mathCourseId,
                Name = "Math 101",
                Code = "MATH101",
                Description = "Basic Mathematics",
                SemesterId = semester.Id,
                MaxCapacity = 30,
                Fee = 500.00m,
                AgeGroup = "High School",
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new Course
            {
                Id = _englishCourseId,
                Name = "English 101",
                Code = "ENG101",
                Description = "Basic English",
                SemesterId = semester.Id,
                MaxCapacity = 25,
                Fee = 450.00m,
                AgeGroup = "High School",
                StartTime = new TimeSpan(11, 0, 0),
                EndTime = new TimeSpan(12, 30, 0),
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            }
        };

        var enrollments = new List<Enrollment>
        {
            new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = _johnStudentId,
                CourseId = _mathCourseId,
                SemesterId = semester.Id,
                EnrollmentType = EnrollmentType.Enrolled,
                EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                FeeAmount = 500.00m,
                AmountPaid = 500.00m,
                PaymentStatus = PaymentStatus.Paid
            },
            new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = _aliceStudentId,
                CourseId = _mathCourseId,
                SemesterId = semester.Id,
                EnrollmentType = EnrollmentType.Enrolled,
                EnrollmentDate = DateTime.UtcNow.AddMonths(-1),
                FeeAmount = 500.00m,
                AmountPaid = 250.00m,
                PaymentStatus = PaymentStatus.Partial
            }
        };

        var grades = new List<GradeRecord>
        {
            new GradeRecord
            {
                Id = 1,
                StudentId = _johnStudentId,
                CourseId = _mathCourseId,
                LetterGrade = "A",
                GradeDate = DateTime.UtcNow.AddDays(-14),
                Comments = "Excellent work"
            },
            new GradeRecord
            {
                Id = 2,
                StudentId = _aliceStudentId,
                CourseId = _mathCourseId,
                LetterGrade = "B+",
                GradeDate = DateTime.UtcNow.AddDays(-7),
                Comments = "Good progress"
            }
        };

        _context.Semesters.Add(semester);
        _context.AccountHolders.Add(accountHolder);
        _context.Students.AddRange(students);
        _context.Courses.AddRange(courses);
        _context.Enrollments.AddRange(enrollments);
        _context.GradeRecords.AddRange(grades);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllStudentsAsync_Should_ReturnAllStudents_OrderedByLastNameThenFirstName()
    {
        // Act
        var result = await _service.GetAllStudentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var studentList = result.ToList();
        studentList[0].LastName.Should().Be("Doe");
        studentList[1].LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetStudentByIdAsync_Should_ReturnStudent_WhenStudentExists()
    {
        // Act
        var result = await _service.GetStudentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
    }

    [Fact]
    public async Task GetStudentByIdAsync_Should_ReturnNull_WhenStudentDoesNotExist()
    {
        // Act
        var result = await _service.GetStudentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateStudentAsync_Should_CreateAndReturnStudent()
    {
        // Arrange
        var createDto = new CreateStudentDto
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@example.com",
            PhoneNumber = "555-123-4567",
            DateOfBirth = new DateOnly(2001, 3, 10),
            Address = "789 Pine St",
            City = "Newtown",
            State = "TX",
            ZipCode = "78901",
            EmergencyContactName = "Sarah Johnson",
            EmergencyContactPhone = "555-123-4569"
        };

        // Act
        var result = await _service.CreateStudentAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Johnson");
        result.Email.Should().Be("alice.johnson@example.com");

        // Verify it was saved to database
        var savedStudent = await _context.Students.FindAsync(result.Id);
        savedStudent.Should().NotBeNull();
        savedStudent!.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task UpdateStudentAsync_Should_UpdateAndReturnStudent_WhenStudentExists()
    {
        // Arrange
        var updateDto = new UpdateStudentDto
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            Email = "jonathan.doe@example.com",
            PhoneNumber = "123-456-7890",
            DateOfBirth = new DateOnly(2000, 1, 15),
            Address = "123 Main St Updated",
            City = "Anytown",
            State = "CA",
            ZipCode = "12345",
            EmergencyContactName = "Bob Doe",
            EmergencyContactPhone = "123-456-7892"
        };

        // Act
        var result = await _service.UpdateStudentAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Jonathan");
        result.Address.Should().Be("123 Main St Updated");

        // Verify it was updated in database
        var updatedStudent = await _context.Students.FindAsync(1);
        updatedStudent.Should().NotBeNull();
        updatedStudent!.FirstName.Should().Be("Jonathan");
    }

    [Fact]
    public async Task UpdateStudentAsync_Should_ReturnNull_WhenStudentDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateStudentDto
        {
            FirstName = "NonExistent",
            LastName = "Student",
            Email = "nonexistent@example.com"
        };

        // Act
        var result = await _service.UpdateStudentAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStudentAsync_Should_ReturnTrue_WhenStudentExists()
    {
        // Act
        var result = await _service.DeleteStudentAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify student was deleted
        var deletedStudent = await _context.Students.FindAsync(1);
        deletedStudent.Should().BeNull();
    }

    [Fact]
    public async Task DeleteStudentAsync_Should_ReturnFalse_WhenStudentDoesNotExist()
    {
        // Act
        var result = await _service.DeleteStudentAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetStudentEnrollmentsAsync_Should_ReturnStudentEnrollments()
    {
        // Act
        var result = await _service.GetStudentEnrollmentsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var enrollment = result.First();
        enrollment.StudentId.Should().Be(1);
        enrollment.CourseId.Should().Be(1);
        enrollment.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetStudentEnrollmentsAsync_Should_ReturnEmptyList_WhenStudentHasNoEnrollments()
    {
        // Act
        var result = await _service.GetStudentEnrollmentsAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetStudentGradesAsync_Should_ReturnStudentGrades_OrderedByGradeDateDescending()
    {
        // Add multiple grades for testing order
        var additionalGrade = new GradeRecord
        {
            Id = 3,
            StudentId = _johnStudentId,
            CourseId = _mathCourseId,
            LetterGrade = "B",
            GradeDate = DateTime.UtcNow.AddDays(-1),
            Comments = "Recent grade"
        };
        _context.GradeRecords.Add(additionalGrade);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentGradesAsync(_johnStudentId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var gradesList = result.ToList();
        gradesList[0].LetterGrade.Should().Be("B"); // Most recent grade first
        gradesList[1].LetterGrade.Should().Be("A"); // Older grade second
    }

    [Fact]
    public async Task GetStudentGradesAsync_Should_ReturnEmptyList_WhenStudentHasNoGrades()
    {
        // Act
        var result = await _service.GetStudentGradesAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
