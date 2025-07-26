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
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
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

        var grades = new List<GradeRecord>
        {
            new GradeRecord
            {
                Id = 1,
                StudentId = 1,
                CourseId = 1,
                LetterGrade = "A",
                GradeDate = DateTime.Now.AddDays(-21),
                Comments = "Excellent work on midterm"
            },
            new GradeRecord
            {
                Id = 2,
                StudentId = 2,
                CourseId = 1,
                LetterGrade = "B+",
                GradeDate = DateTime.Now.AddDays(-14),
                Comments = "Good understanding of concepts"
            },
            new GradeRecord
            {
                Id = 3,
                StudentId = 1,
                CourseId = 2,
                LetterGrade = "A-",
                GradeDate = DateTime.Now.AddDays(-7),
                Comments = "Strong essay writing skills"
            }
        };

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
        result.StudentId.Should().Be(1);
        result.CourseId.Should().Be(1);
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
            StudentId = 2,
            CourseId = 2,
            LetterGrade = "B",
            Comments = "Good participation in class discussions",
            GradeDate = DateTime.Now
        };

        // Act
        var result = await _service.CreateGradeAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(2);
        result.CourseId.Should().Be(2);
        result.LetterGrade.Should().Be("B");
        result.Comments.Should().Be("Good participation in class discussions");
        result.GradeDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

        // Verify it was saved to database
        var savedGrade = await _context.GradeRecords.FindAsync(result.Id);
        savedGrade.Should().NotBeNull();
        savedGrade!.StudentId.Should().Be(2);
        savedGrade.CourseId.Should().Be(2);
        savedGrade.LetterGrade.Should().Be("B");
    }

    [Fact]
    public async Task UpdateGradeAsync_Should_UpdateAndReturnGrade_WhenGradeExists()
    {
        // Arrange
        var updateDto = new CreateGradeRecordDto
        {
            StudentId = 1,
            CourseId = 1,
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
            StudentId = 1,
            CourseId = 1,
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
            StudentId = 1,
            CourseId = 1,
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
            StudentId = 1,
            CourseId = 1,
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
