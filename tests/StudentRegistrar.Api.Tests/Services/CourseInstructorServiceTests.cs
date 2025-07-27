using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class CourseInstructorServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly CourseInstructorService _service;

    // Test data Guid IDs
    private readonly Guid _johnEducatorId = Guid.NewGuid();
    private readonly Guid _aliceEducatorId = Guid.NewGuid();
    private readonly Guid _mathCourseId = Guid.NewGuid();
    private readonly Guid _englishCourseId = Guid.NewGuid();
    private readonly Guid _fallSemesterId = Guid.NewGuid();

    public CourseInstructorServiceTests()
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

        _service = new CourseInstructorService(_context, _mapper);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var semesterId = Guid.Parse("77777777-7777-7777-7777-777777777777");
        
        var courses = new List<Course>
        {
            new Course
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                SemesterId = semesterId,
                Name = "Mathematics 101",
                Description = "Basic Mathematics",
                MaxCapacity = 25,
                Fee = 150.00m,
                AgeGroup = "Adults"
            },
            new Course
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SemesterId = semesterId,
                Name = "Physics 201",
                Description = "Introduction to Physics",
                MaxCapacity = 20,
                Fee = 200.00m,
                AgeGroup = "Adults"
            }
        };

        var instructors = new List<CourseInstructor>
        {
            new CourseInstructor
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@university.edu",
                Phone = "555-123-4567",
                IsPrimary = true,
                InstructorInfoJson = """{"Bio":"Professor of Mathematics","Qualifications":["PhD Mathematics","MSc Mathematics"],"CustomFields":{"Office":"Room 201","OfficeHours":"Mon-Wed 2-4pm"}}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new CourseInstructor
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@university.edu",
                Phone = "555-987-6543",
                IsPrimary = false,
                InstructorInfoJson = """{"Bio":"Teaching Assistant","Qualifications":["MSc Mathematics"],"CustomFields":{"Office":"Room 105"}}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new CourseInstructor
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Robert",
                LastName = "Johnson",
                Email = "robert.johnson@university.edu",
                Phone = "555-555-5555",
                IsPrimary = true,
                InstructorInfoJson = """{"Bio":"Professor of Physics","Qualifications":["PhD Physics","MSc Physics"],"CustomFields":{"Lab":"Physics Lab A"}}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        _context.Courses.AddRange(courses);
        _context.CourseInstructors.AddRange(instructors);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCourseInstructorsAsync_Should_ReturnAllInstructors_OrderedByName()
    {
        // Act
        var result = await _service.GetAllCourseInstructorsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var instructorsList = result.ToList();
        instructorsList[0].LastName.Should().Be("Doe"); // Jane Doe
        instructorsList[1].LastName.Should().Be("Johnson"); // Robert Johnson
        instructorsList[2].LastName.Should().Be("Smith"); // John Smith
        
        // Verify course navigation is included
        instructorsList.All(i => i.Course != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetCourseInstructorByIdAsync_Should_ReturnInstructor_WhenInstructorExists()
    {
        // Arrange
        var instructorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        var result = await _service.GetCourseInstructorByIdAsync(instructorId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(instructorId);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("john.smith@university.edu");
        result.Phone.Should().Be("555-123-4567");
        result.IsPrimary.Should().BeTrue();
        result.Course.Should().NotBeNull();
        result.Course!.Name.Should().Be("Mathematics 101");
    }

    [Fact]
    public async Task GetCourseInstructorByIdAsync_Should_ReturnNull_WhenInstructorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.GetCourseInstructorByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetCourseInstructorsByCourseIdAsync_Should_ReturnInstructorsForCourse_OrderedByPrimary()
    {
        // Arrange
        var courseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetCourseInstructorsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var instructorsList = result.ToList();
        instructorsList[0].IsPrimary.Should().BeTrue(); // Primary instructor first
        instructorsList[0].FirstName.Should().Be("John");
        instructorsList[1].IsPrimary.Should().BeFalse(); // Non-primary instructor second
        instructorsList[1].FirstName.Should().Be("Jane");
    }

    [Fact]
    public async Task GetCourseInstructorsByCourseIdAsync_Should_ReturnEmpty_WhenCourseHasNoInstructors()
    {
        // Arrange
        var courseWithoutInstructors = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = Guid.NewGuid(),
            Name = "Empty Course",
            Description = "Course with no instructors",
            MaxCapacity = 15,
            Fee = 100.00m,
            AgeGroup = "Adults"
        };
        _context.Courses.Add(courseWithoutInstructors);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetCourseInstructorsByCourseIdAsync(courseWithoutInstructors.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCourseInstructorAsync_Should_CreateAndReturnInstructor()
    {
        // Arrange
        var courseId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var createDto = new CreateCourseInstructorDto
        {
            CourseId = courseId,
            FirstName = "Alice",
            LastName = "Williams",
            Email = "alice.williams@university.edu",
            Phone = "555-111-2222",
            IsPrimary = false,
            InstructorInfo = new StudentRegistrar.Api.DTOs.InstructorInfo
            {
                Bio = "Graduate Student",
                Qualifications = new List<string> { "MSc in progress" },
                CustomFields = new Dictionary<string, string> { { "Year", "PhD Year 2" } }
            }
        };

        // Act
        var result = await _service.CreateCourseInstructorAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Williams");
        result.Email.Should().Be("alice.williams@university.edu");
        result.Phone.Should().Be("555-111-2222");
        result.IsPrimary.Should().BeFalse();
        result.CourseId.Should().Be(courseId);
        result.Id.Should().NotBeEmpty();
        result.Course.Should().NotBeNull();

        // Verify it was saved to database
        var savedInstructor = await _context.CourseInstructors.FindAsync(result.Id);
        savedInstructor.Should().NotBeNull();
        savedInstructor!.FirstName.Should().Be("Alice");
    }

    [Fact]
    public async Task CreateCourseInstructorAsync_Should_SetTimestamps()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var createDto = new CreateCourseInstructorDto
        {
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Test",
            LastName = "Instructor"
        };

        // Act
        var result = await _service.CreateCourseInstructorAsync(createDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamps in database
        var savedInstructor = await _context.CourseInstructors.FindAsync(result.Id);
        savedInstructor.Should().NotBeNull();
        savedInstructor!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        savedInstructor.CreatedAt.Should().BeOnOrBefore(afterCreate);
        savedInstructor.UpdatedAt.Should().BeOnOrAfter(beforeCreate);
        savedInstructor.UpdatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task UpdateCourseInstructorAsync_Should_UpdateAndReturnInstructor_WhenInstructorExists()
    {
        // Arrange
        var instructorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var updateDto = new UpdateCourseInstructorDto
        {
            FirstName = "Jonathan",
            LastName = "Smith-Updated",
            Email = "jonathan.smith@university.edu",
            Phone = "555-123-9999",
            IsPrimary = true,
            InstructorInfo = new StudentRegistrar.Api.DTOs.InstructorInfo
            {
                Bio = "Updated Professor of Mathematics",
                Qualifications = new List<string> { "PhD Mathematics", "MSc Mathematics", "BSc Mathematics" },
                CustomFields = new Dictionary<string, string> { { "Office", "Room 301" } }
            }
        };

        // Act
        var result = await _service.UpdateCourseInstructorAsync(instructorId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(instructorId);
        result.FirstName.Should().Be("Jonathan");
        result.LastName.Should().Be("Smith-Updated");
        result.Email.Should().Be("jonathan.smith@university.edu");
        result.Phone.Should().Be("555-123-9999");

        // Verify it was updated in database
        var updatedInstructor = await _context.CourseInstructors.FindAsync(instructorId);
        updatedInstructor.Should().NotBeNull();
        updatedInstructor!.FirstName.Should().Be("Jonathan");
        updatedInstructor.LastName.Should().Be("Smith-Updated");
    }

    [Fact]
    public async Task UpdateCourseInstructorAsync_Should_ReturnNull_WhenInstructorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateCourseInstructorDto
        {
            FirstName = "NonExistent",
            LastName = "Instructor"
        };

        // Act
        var result = await _service.UpdateCourseInstructorAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateCourseInstructorAsync_Should_UpdateTimestamp()
    {
        // Arrange
        var instructorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var originalInstructor = await _context.CourseInstructors.FindAsync(instructorId);
        var originalUpdatedAt = originalInstructor!.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);
        
        var updateDto = new UpdateCourseInstructorDto
        {
            FirstName = "Updated Name",
            LastName = "Updated LastName"
        };

        // Act
        var result = await _service.UpdateCourseInstructorAsync(instructorId, updateDto);

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamp was updated
        var updatedInstructor = await _context.CourseInstructors.FindAsync(instructorId);
        updatedInstructor.Should().NotBeNull();
        updatedInstructor!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task DeleteCourseInstructorAsync_Should_ReturnTrue_WhenInstructorExists()
    {
        // Arrange
        var instructorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        var result = await _service.DeleteCourseInstructorAsync(instructorId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedInstructor = await _context.CourseInstructors.FindAsync(instructorId);
        deletedInstructor.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCourseInstructorAsync_Should_ReturnFalse_WhenInstructorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.DeleteCourseInstructorAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
