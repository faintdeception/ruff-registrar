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
        var students = new List<LegacyStudent>
        {
            new LegacyStudent
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                DateOfBirth = new DateOnly(2000, 1, 15),
                Address = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                EmergencyContactName = "Bob Doe",
                EmergencyContactPhone = "123-456-7892",
                CreatedAt = DateTime.Now.AddMonths(-6),
                UpdatedAt = DateTime.Now.AddMonths(-6)
            },
            new LegacyStudent
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                PhoneNumber = "987-654-3210",
                DateOfBirth = new DateOnly(1999, 5, 20),
                Address = "456 Oak Ave",
                City = "Somewhere",
                State = "NY",
                ZipCode = "54321",
                EmergencyContactName = "Mary Smith",
                EmergencyContactPhone = "987-654-3212",
                CreatedAt = DateTime.Now.AddMonths(-3),
                UpdatedAt = DateTime.Now.AddMonths(-3)
            }
        };

        var courses = new List<LegacyCourse>
        {
            new LegacyCourse
            {
                Id = 1,
                Name = "Math 101",
                Code = "MATH101",
                Description = "Basic Mathematics",
                CreditHours = 3,
                Instructor = "Prof. Johnson",
                AcademicYear = "2024-25",
                Semester = "Fall"
            },
            new LegacyCourse
            {
                Id = 2,
                Name = "English 101",
                Code = "ENG101",
                Description = "Basic English",
                CreditHours = 3,
                Instructor = "Prof. Williams",
                AcademicYear = "2024-25",
                Semester = "Fall"
            }
        };

        var enrollments = new List<LegacyEnrollment>
        {
            new LegacyEnrollment
            {
                Id = 1,
                StudentId = 1,
                CourseId = 1,
                EnrollmentDate = DateTime.Now.AddMonths(-2),
                Status = "Active"
            },
            new LegacyEnrollment
            {
                Id = 2,
                StudentId = 2,
                CourseId = 2,
                EnrollmentDate = DateTime.Now.AddMonths(-1),
                Status = "Active"
            }
        };

        var grades = new List<GradeRecord>
        {
            new GradeRecord
            {
                Id = 1,
                StudentId = 1,
                CourseId = 1,
                LetterGrade = "A",
                GradeDate = DateTime.Now.AddDays(-14),
                Comments = "Excellent work"
            },
            new GradeRecord
            {
                Id = 2,
                StudentId = 2,
                CourseId = 2,
                LetterGrade = "B+",
                GradeDate = DateTime.Now.AddDays(-7),
                Comments = "Good progress"
            }
        };

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
            StudentId = 1,
            CourseId = 1,
            LetterGrade = "B",
            GradeDate = DateTime.Now.AddDays(-1),
            Comments = "Recent grade"
        };
        _context.GradeRecords.Add(additionalGrade);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetStudentGradesAsync(1);

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
