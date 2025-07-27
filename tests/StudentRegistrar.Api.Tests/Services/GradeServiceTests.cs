using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class GradeServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly GradeService _service;
    
    // Test data references
    private readonly Guid _johnStudentId = Guid.NewGuid();
    private readonly Guid _aliceStudentId = Guid.NewGuid();
    private readonly Guid _accountHolderId = Guid.NewGuid();
    private readonly Guid _mathCourseId = Guid.NewGuid();
    private readonly Guid _englishCourseId = Guid.NewGuid();
    private readonly Guid _fallSemesterId = Guid.NewGuid();

    public GradeServiceTests()
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

        _service = new GradeService(_context, _mapper);

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

        var grades = new List<GradeRecord>
        {
            new GradeRecord
            {
                Id = 1,
                StudentId = _johnStudentId,
                CourseId = _mathCourseId,
                LetterGrade = "A",
                GradeDate = DateTime.UtcNow.AddDays(-21),
                Comments = "Excellent work on midterm"
            },
            new GradeRecord
            {
                Id = 2,
                StudentId = _aliceStudentId,
                CourseId = _mathCourseId,
                LetterGrade = "B+",
                GradeDate = DateTime.UtcNow.AddDays(-14),
                Comments = "Good understanding of concepts"
            },
            new GradeRecord
            {
                Id = 3,
                StudentId = _johnStudentId,
                CourseId = _englishCourseId,
                LetterGrade = "A-",
                GradeDate = DateTime.UtcNow.AddDays(-7),
                Comments = "Strong essay writing skills"
            }
        };

        _context.Semesters.Add(semester);
        _context.AccountHolders.Add(accountHolder);
        _context.Students.AddRange(students);
        _context.Courses.AddRange(courses);
        _context.GradeRecords.AddRange(grades);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllGradesAsync_Should_ReturnAllGrades_OrderedByGradeDateDescending()
    {
        // Act
        var result = await _service.GetAllGradesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var gradesList = result.ToList();
        // Should be ordered by grade date descending (most recent first)
        gradesList[0].Id.Should().Be(3); // Most recent grade
        gradesList[1].Id.Should().Be(2);
        gradesList[2].Id.Should().Be(1); // Oldest grade
    }

    [Fact]
    public async Task GetGradeByIdAsync_Should_ReturnGrade_WhenGradeExists()
    {
        // Act
        var result = await _service.GetGradeByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.StudentId.Should().Be(_johnStudentId.GetHashCode());
        result.CourseId.Should().Be(_mathCourseId.GetHashCode());
        result.LetterGrade.Should().Be("A");
        result.Comments.Should().Be("Excellent work on midterm");
    }

    [Fact]
    public async Task GetGradeByIdAsync_Should_ReturnNull_WhenGradeDoesNotExist()
    {
        // Act
        var result = await _service.GetGradeByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateGradeAsync_Should_CreateAndReturnGrade()
    {
        // Arrange
        var createDto = new CreateGradeRecordDto
        {
            StudentId = _aliceStudentId.GetHashCode(),
            CourseId = _englishCourseId.GetHashCode(),
            LetterGrade = "B",
            Comments = "Good participation in class discussions",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.CreateGradeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(_aliceStudentId.GetHashCode());
        result.CourseId.Should().Be(_englishCourseId.GetHashCode());
        result.LetterGrade.Should().Be("B");
        result.Comments.Should().Be("Good participation in class discussions");
        result.GradeDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

        // Verify it was saved to database
        var savedGrade = await _context.GradeRecords.FindAsync(result.Id);
        savedGrade.Should().NotBeNull();
        savedGrade!.StudentId.Should().Be(_aliceStudentId);
        savedGrade.CourseId.Should().Be(_englishCourseId);
        savedGrade.LetterGrade.Should().Be("B");
    }

    [Fact]
    public async Task UpdateGradeAsync_Should_UpdateAndReturnGrade_WhenGradeExists()
    {
        // Arrange
        var updateDto = new CreateGradeRecordDto
        {
            StudentId = _johnStudentId.GetHashCode(),
            CourseId = _mathCourseId.GetHashCode(),
            LetterGrade = "A+",
            Comments = "Outstanding performance on final exam",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.UpdateGradeAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.LetterGrade.Should().Be("A+");
        result.Comments.Should().Be("Outstanding performance on final exam");

        // Verify it was updated in database
        var updatedGrade = await _context.GradeRecords.FindAsync(1);
        updatedGrade.Should().NotBeNull();
        updatedGrade!.LetterGrade.Should().Be("A+");
        updatedGrade.Comments.Should().Be("Outstanding performance on final exam");
    }

    [Fact]
    public async Task UpdateGradeAsync_Should_ReturnNull_WhenGradeDoesNotExist()
    {
        // Arrange
        var updateDto = new CreateGradeRecordDto
        {
            StudentId = _johnStudentId.GetHashCode(),
            CourseId = _mathCourseId.GetHashCode(),
            LetterGrade = "F",
            Comments = "This grade doesn't exist",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.UpdateGradeAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGradeAsync_Should_ReturnTrue_WhenGradeExists()
    {
        // Act
        var result = await _service.DeleteGradeAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify grade was deleted
        var deletedGrade = await _context.GradeRecords.FindAsync(1);
        deletedGrade.Should().BeNull();
    }

    [Fact]
    public async Task DeleteGradeAsync_Should_ReturnFalse_WhenGradeDoesNotExist()
    {
        // Act
        var result = await _service.DeleteGradeAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("A+")]
    [InlineData("A")]
    [InlineData("A-")]
    [InlineData("B+")]
    [InlineData("B")]
    [InlineData("B-")]
    [InlineData("C+")]
    [InlineData("C")]
    [InlineData("C-")]
    [InlineData("D+")]
    [InlineData("D")]
    [InlineData("D-")]
    [InlineData("F")]
    public async Task CreateGradeAsync_Should_AcceptValidGradeValues(string gradeValue)
    {
        // Arrange
        var createDto = new CreateGradeRecordDto
        {
            StudentId = _johnStudentId.GetHashCode(),
            CourseId = _mathCourseId.GetHashCode(),
            LetterGrade = gradeValue,
            Comments = $"Test grade: {gradeValue}",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.CreateGradeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.LetterGrade.Should().Be(gradeValue);
    }

    [Fact]
    public async Task CreateGradeAsync_Should_SetGradeDate_ToCurrentDateTime()
    {
        // Arrange
        var beforeCreate = DateTime.Now;
        var createDto = new CreateGradeRecordDto
        {
            StudentId = _johnStudentId.GetHashCode(),
            CourseId = _mathCourseId.GetHashCode(),
            LetterGrade = "B",
            Comments = "Testing date assignment",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.CreateGradeAsync(createDto);
        var afterCreate = DateTime.Now;

        // Assert
        result.Should().NotBeNull();
        result.GradeDate.Should().BeOnOrAfter(beforeCreate);
        result.GradeDate.Should().BeOnOrBefore(afterCreate);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
