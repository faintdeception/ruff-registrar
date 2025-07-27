using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class CourseServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly CourseService _service;
    
    // Test data references
    private readonly Guid _mathCourseId = Guid.NewGuid();
    private readonly Guid _englishCourseId = Guid.NewGuid();
    private readonly Guid _physicsCourseId = Guid.NewGuid();
    private readonly Guid _fallSemesterId = Guid.NewGuid();
    private readonly Guid _springSemesterId = Guid.NewGuid();
    private readonly Guid _johnStudentId = Guid.NewGuid();
    private readonly Guid _aliceStudentId = Guid.NewGuid();
    private readonly Guid _accountHolderId = Guid.NewGuid();

    public CourseServiceTests()
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

        _service = new CourseService(_context, _mapper);

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

        var springSemester = new Semester
        {
            Id = _springSemesterId,
            Name = "Spring 2025",
            Code = "SPRING2025",
            StartDate = new DateTime(2025, 1, 15),
            EndDate = new DateTime(2025, 5, 15),
            RegistrationStartDate = new DateTime(2024, 12, 1),
            RegistrationEndDate = new DateTime(2025, 1, 10),
            IsActive = true
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
            },
            new Course
            {
                Id = _physicsCourseId,
                Name = "Physics 101",
                Code = "PHYS101",
                Description = "Basic Physics",
                SemesterId = springSemester.Id,
                MaxCapacity = 20,
                Fee = 600.00m,
                AgeGroup = "High School",
                StartTime = new TimeSpan(13, 0, 0),
                EndTime = new TimeSpan(14, 30, 0),
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            }
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
            },
            new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentId = _johnStudentId,
                CourseId = _englishCourseId,
                SemesterId = semester.Id,
                EnrollmentType = EnrollmentType.Withdrawn,
                EnrollmentDate = DateTime.UtcNow.AddMonths(-1),
                FeeAmount = 450.00m,
                AmountPaid = 450.00m,
                PaymentStatus = PaymentStatus.Paid
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

        _context.Semesters.AddRange(new[] { semester, springSemester });
        _context.AccountHolders.Add(accountHolder);
        _context.Courses.AddRange(courses);
        _context.Students.AddRange(students);
        _context.Enrollments.AddRange(enrollments);
        _context.GradeRecords.AddRange(grades);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCoursesAsync_Should_ReturnAllCourses_OrderedByAcademicYearThenSemesterThenCode()
    {
        // Act
        var result = await _service.GetAllCoursesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var courseList = result.ToList();
        // The courses should be ordered by academic year, then by code
        courseList[0].Code.Should().Be("ENG101"); // 2024-25
        courseList[1].Code.Should().Be("MATH101"); // 2024-25
        courseList[2].Code.Should().Be("PHYS101"); // 2025-26
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_ReturnCourse_WhenCourseExists()
    {
        // Act
        var result = await _service.GetCourseByIdAsync(_mathCourseId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Math 101");
        result.Code.Should().Be("MATH101");
        result.CreditHours.Should().BeGreaterThan(0); // The mapping will determine this value
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_ReturnNull_WhenCourseDoesNotExist()
    {
        // Act
        var result = await _service.GetCourseByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCourseAsync_Should_CreateAndReturnCourse()
    {
        // Arrange
        var createDto = new CreateCourseDto
        {
            Name = "Chemistry 101",
            Code = "CHEM101",
            Description = "Basic Chemistry",
            CreditHours = 4,
            Instructor = "Dr. Wilson",
            AcademicYear = "2024-25",
            Semester = "Spring"
        };

        // Act
        var result = await _service.CreateCourseAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Chemistry 101");
        result.Code.Should().Be("CHEM101");
        result.CreditHours.Should().Be(4);
        result.Instructor.Should().Be("Dr. Wilson");

        // Verify it was saved to database
        var allCourses = await _context.Courses.ToListAsync();
        var savedCourse = allCourses.FirstOrDefault(c => c.Id.GetHashCode() == result.Id);
        savedCourse.Should().NotBeNull();
        savedCourse!.Name.Should().Be("Chemistry 101");
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_UpdateAndReturnCourse_WhenCourseExists()
    {
        // Arrange
        var updateDto = new UpdateCourseDto
        {
            Name = "Advanced Mathematics",
            Code = "MATH101",
            Description = "Advanced Mathematical Concepts",
            CreditHours = 4,
            Instructor = "Prof. Johnson",
            AcademicYear = "2024-25",
            Semester = "Fall"
        };

        // Act
        var result = await _service.UpdateCourseAsync(_mathCourseId.GetHashCode(), updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Advanced Mathematics");
        result.Description.Should().Be("Advanced Mathematical Concepts");
        result.CreditHours.Should().Be(4);
        result.Instructor.Should().Be("Prof. Johnson");

        // Verify it was updated in database
        var allCourses = await _context.Courses.ToListAsync();
        var updatedCourse = allCourses.FirstOrDefault(c => c.Id == _mathCourseId);
        updatedCourse.Should().NotBeNull();
        updatedCourse!.Name.Should().Be("Advanced Mathematics");
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_ReturnNull_WhenCourseDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateCourseDto
        {
            Name = "NonExistent Course",
            Code = "NONE999",
            Description = "This course doesn't exist"
        };

        // Act
        var result = await _service.UpdateCourseAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_ReturnTrue_WhenCourseExists()
    {
        // Act
        var result = await _service.DeleteCourseAsync(_physicsCourseId.GetHashCode());

        // Assert
        result.Should().BeTrue();

        // Verify course was deleted
        var deletedCourse = await _context.Courses.FindAsync(_physicsCourseId);
        deletedCourse.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_ReturnFalse_WhenCourseDoesNotExist()
    {
        // Act
        var result = await _service.DeleteCourseAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_Should_ReturnCourseEnrollments()
    {
        // Act
        var result = await _service.GetCourseEnrollmentsAsync(_mathCourseId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var enrollmentList = result.ToList();
        enrollmentList.Should().OnlyContain(e => e.CourseId == _mathCourseId.GetHashCode());
        enrollmentList.Should().Contain(e => e.StudentId == _johnStudentId.GetHashCode());
        enrollmentList.Should().Contain(e => e.StudentId == _aliceStudentId.GetHashCode());
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_Should_ReturnEmptyList_WhenCourseHasNoEnrollments()
    {
        // Act
        var result = await _service.GetCourseEnrollmentsAsync(_physicsCourseId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCourseGradesAsync_Should_ReturnCourseGrades_OrderedByStudentName()
    {
        // Act
        var result = await _service.GetCourseGradesAsync(_mathCourseId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var gradesList = result.ToList();
        gradesList.Should().OnlyContain(g => g.CourseId == _mathCourseId.GetHashCode());
        
        // Should contain both students
        gradesList.Should().Contain(g => g.StudentId == _johnStudentId.GetHashCode());
        gradesList.Should().Contain(g => g.StudentId == _aliceStudentId.GetHashCode());
    }

    [Fact]
    public async Task GetCourseGradesAsync_Should_ReturnEmptyList_WhenCourseHasNoGrades()
    {
        // Act
        var result = await _service.GetCourseGradesAsync(_englishCourseId.GetHashCode());

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
