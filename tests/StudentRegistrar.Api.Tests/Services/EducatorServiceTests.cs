using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class EducatorServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly EducatorService _service;

    // Test data Guid IDs
    private readonly Guid _johnEducatorId = Guid.NewGuid();
    private readonly Guid _aliceEducatorId = Guid.NewGuid();

    public EducatorServiceTests()
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

        _service = new EducatorService(_context, _mapper);

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
                Name = "Biology 101",
                Description = "Introduction to Biology",
                MaxCapacity = 25,
                Fee = 150.00m,
                AgeGroup = "Adults"
            },
            new Course
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                SemesterId = semesterId,
                Name = "Chemistry 201",
                Description = "Organic Chemistry",
                MaxCapacity = 20,
                Fee = 200.00m,
                AgeGroup = "Adults"
            }
        };

        var educators = new List<Educator>
        {
            new Educator
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Dr. Sarah",
                LastName = "Wilson",
                Email = "sarah.wilson@university.edu",
                Phone = "555-123-4567",
                IsPrimary = true,
                IsActive = true,
                EducatorInfoJson = """{"Bio":"Professor of Biology","Qualifications":["PhD Biology","MSc Biology"],"Specializations":["Cell Biology","Genetics"],"Department":"Biology","CustomFields":{"Office":"Bio Building 301","OfficeHours":"Tue-Thu 10-12pm"}}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Educator
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Mark",
                LastName = "Thompson",
                Email = "mark.thompson@university.edu",
                Phone = "555-987-6543",
                IsPrimary = false,
                IsActive = true,
                EducatorInfoJson = """{"Bio":"Teaching Assistant","Qualifications":["MSc Biology"],"Specializations":["Laboratory Techniques"],"CustomFields":{"Lab":"Bio Lab A"}}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-1),
                UpdatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new Educator
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                CourseId = null, // Unassigned educator
                FirstName = "Lisa",
                LastName = "Anderson",
                Email = "lisa.anderson@university.edu",
                Phone = "555-555-5555",
                IsPrimary = false,
                IsActive = true,
                EducatorInfoJson = """{"Bio":"Independent Educator","Qualifications":["PhD Chemistry","MSc Chemistry"],"Specializations":["Analytical Chemistry"],"Department":"Chemistry"}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-1)
            },
            new Educator
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Michael",
                LastName = "Davis",
                Email = "michael.davis@university.edu",
                Phone = "555-777-8888",
                IsPrimary = true,
                IsActive = false, // Inactive educator
                EducatorInfoJson = """{"Bio":"Former Professor","Qualifications":["PhD Chemistry"],"Department":"Chemistry"}""",
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        _context.Courses.AddRange(courses);
        _context.Educators.AddRange(educators);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllEducatorsAsync_Should_ReturnAllEducators_OrderedByName()
    {
        // Act
        var result = await _service.GetAllEducatorsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        
        var educatorsList = result.ToList();
        educatorsList[0].LastName.Should().Be("Anderson"); // Lisa Anderson
        educatorsList[1].LastName.Should().Be("Davis"); // Michael Davis
        educatorsList[2].LastName.Should().Be("Thompson"); // Mark Thompson
        educatorsList[3].LastName.Should().Be("Wilson"); // Dr. Sarah Wilson
        
        // Verify course navigation is included where applicable
        educatorsList.Where(e => e.CourseId.HasValue).All(e => e.Course != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetEducatorByIdAsync_Should_ReturnEducator_WhenEducatorExists()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        var result = await _service.GetEducatorByIdAsync(educatorId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(educatorId);
        result.FirstName.Should().Be("Dr. Sarah");
        result.LastName.Should().Be("Wilson");
        result.Email.Should().Be("sarah.wilson@university.edu");
        result.Phone.Should().Be("555-123-4567");
        result.IsPrimary.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.Course.Should().NotBeNull();
        result.Course!.Name.Should().Be("Biology 101");
        result.IsAssignedToCourse.Should().BeTrue();
    }

    [Fact]
    public async Task GetEducatorByIdAsync_Should_ReturnNull_WhenEducatorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.GetEducatorByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetEducatorsByCourseIdAsync_Should_ReturnEducatorsForCourse_OrderedByPrimary()
    {
        // Arrange
        var courseId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetEducatorsByCourseIdAsync(courseId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var educatorsList = result.ToList();
        educatorsList[0].IsPrimary.Should().BeTrue(); // Primary educator first
        educatorsList[0].FirstName.Should().Be("Dr. Sarah");
        educatorsList[1].IsPrimary.Should().BeFalse(); // Non-primary educator second
        educatorsList[1].FirstName.Should().Be("Mark");
    }

    [Fact]
    public async Task GetEducatorsByCourseIdAsync_Should_ReturnEmpty_WhenCourseHasNoEducators()
    {
        // Arrange
        var courseWithoutEducators = new Course
        {
            Id = Guid.NewGuid(),
            SemesterId = Guid.NewGuid(),
            Name = "Empty Course",
            Description = "Course with no educators",
            MaxCapacity = 15,
            Fee = 100.00m,
            AgeGroup = "Adults"
        };
        _context.Courses.Add(courseWithoutEducators);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetEducatorsByCourseIdAsync(courseWithoutEducators.Id);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUnassignedEducatorsAsync_Should_ReturnOnlyActiveUnassignedEducators()
    {
        // Act
        var result = await _service.GetUnassignedEducatorsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        
        var educator = result.First();
        educator.FirstName.Should().Be("Lisa");
        educator.LastName.Should().Be("Anderson");
        educator.CourseId.Should().BeNull();
        educator.IsActive.Should().BeTrue();
        educator.IsAssignedToCourse.Should().BeFalse();
    }

    [Fact]
    public async Task CreateEducatorAsync_Should_CreateAndReturnEducator()
    {
        // Arrange
        var createDto = new CreateEducatorDto
        {
            CourseId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FirstName = "Jennifer",
            LastName = "Brown",
            Email = "jennifer.brown@university.edu",
            Phone = "555-111-2222",
            IsPrimary = false,
            IsActive = true,
            EducatorInfo = new StudentRegistrar.Api.DTOs.EducatorInfo
            {
                Bio = "Adjunct Professor",
                Qualifications = new List<string> { "PhD Biology", "MSc Education" },
                Specializations = new List<string> { "Environmental Biology" },
                Department = "Biology",
                CustomFields = new Dictionary<string, string> { { "Status", "Part-time" } }
            }
        };

        // Act
        var result = await _service.CreateEducatorAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Jennifer");
        result.LastName.Should().Be("Brown");
        result.Email.Should().Be("jennifer.brown@university.edu");
        result.Phone.Should().Be("555-111-2222");
        result.IsPrimary.Should().BeFalse();
        result.IsActive.Should().BeTrue();
        result.CourseId.Should().Be(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        result.Id.Should().NotBeEmpty();

        // Verify it was saved to database
        var savedEducator = await _context.Educators.FindAsync(result.Id);
        savedEducator.Should().NotBeNull();
        savedEducator!.FirstName.Should().Be("Jennifer");
    }

    [Fact]
    public async Task CreateEducatorAsync_Should_CreateUnassignedEducator_WhenCourseIdIsNull()
    {
        // Arrange
        var createDto = new CreateEducatorDto
        {
            CourseId = null, // No course assignment
            FirstName = "Independent",
            LastName = "Teacher",
            Email = "independent.teacher@example.com",
            IsActive = true
        };

        // Act
        var result = await _service.CreateEducatorAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.CourseId.Should().BeNull();
        result.IsAssignedToCourse.Should().BeFalse();
        result.FirstName.Should().Be("Independent");
        result.LastName.Should().Be("Teacher");
    }

    [Fact]
    public async Task CreateEducatorAsync_Should_SetTimestamps()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var createDto = new CreateEducatorDto
        {
            FirstName = "Test",
            LastName = "Educator"
        };

        // Act
        var result = await _service.CreateEducatorAsync(createDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamps in database
        var savedEducator = await _context.Educators.FindAsync(result.Id);
        savedEducator.Should().NotBeNull();
        savedEducator!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        savedEducator.CreatedAt.Should().BeOnOrBefore(afterCreate);
        savedEducator.UpdatedAt.Should().BeOnOrAfter(beforeCreate);
        savedEducator.UpdatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task UpdateEducatorAsync_Should_UpdateAndReturnEducator_WhenEducatorExists()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var updateDto = new UpdateEducatorDto
        {
            CourseId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Change course assignment
            FirstName = "Dr. Sarah Updated",
            LastName = "Wilson-Brown",
            Email = "sarah.brown@university.edu",
            Phone = "555-123-9999",
            IsPrimary = false, // Change from primary to non-primary
            IsActive = true,
            EducatorInfo = new StudentRegistrar.Api.DTOs.EducatorInfo
            {
                Bio = "Updated Professor of Biology",
                Qualifications = new List<string> { "PhD Biology", "MSc Biology", "BSc Biology" },
                Specializations = new List<string> { "Cell Biology", "Genetics", "Molecular Biology" },
                Department = "Biology",
                CustomFields = new Dictionary<string, string> { { "Office", "Bio Building 401" } }
            }
        };

        // Act
        var result = await _service.UpdateEducatorAsync(educatorId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(educatorId);
        result.FirstName.Should().Be("Dr. Sarah Updated");
        result.LastName.Should().Be("Wilson-Brown");
        result.Email.Should().Be("sarah.brown@university.edu");
        result.Phone.Should().Be("555-123-9999");
        result.IsPrimary.Should().BeFalse();
        result.CourseId.Should().Be(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        // Verify it was updated in database
        var updatedEducator = await _context.Educators.FindAsync(educatorId);
        updatedEducator.Should().NotBeNull();
        updatedEducator!.FirstName.Should().Be("Dr. Sarah Updated");
        updatedEducator.LastName.Should().Be("Wilson-Brown");
    }

    [Fact]
    public async Task UpdateEducatorAsync_Should_ReturnNull_WhenEducatorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateEducatorDto
        {
            FirstName = "NonExistent",
            LastName = "Educator"
        };

        // Act
        var result = await _service.UpdateEducatorAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEducatorAsync_Should_UpdateTimestamp()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var originalEducator = await _context.Educators.FindAsync(educatorId);
        var originalUpdatedAt = originalEducator!.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);
        
        var updateDto = new UpdateEducatorDto
        {
            FirstName = "Updated Name",
            LastName = "Updated LastName"
        };

        // Act
        var result = await _service.UpdateEducatorAsync(educatorId, updateDto);

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamp was updated
        var updatedEducator = await _context.Educators.FindAsync(educatorId);
        updatedEducator.Should().NotBeNull();
        updatedEducator!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public async Task DeleteEducatorAsync_Should_ReturnTrue_WhenEducatorExists()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        var result = await _service.DeleteEducatorAsync(educatorId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedEducator = await _context.Educators.FindAsync(educatorId);
        deletedEducator.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEducatorAsync_Should_ReturnFalse_WhenEducatorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.DeleteEducatorAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateEducatorAsync_Should_SetIsActiveToFalse_WhenEducatorExists()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        // Act
        var result = await _service.DeactivateEducatorAsync(educatorId);

        // Assert
        result.Should().BeTrue();

        // Verify educator was deactivated in database
        var deactivatedEducator = await _context.Educators.FindAsync(educatorId);
        deactivatedEducator.Should().NotBeNull();
        deactivatedEducator!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateEducatorAsync_Should_ReturnFalse_WhenEducatorDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.DeactivateEducatorAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeactivateEducatorAsync_Should_UpdateTimestamp()
    {
        // Arrange
        var educatorId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var originalEducator = await _context.Educators.FindAsync(educatorId);
        var originalUpdatedAt = originalEducator!.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);

        // Act
        var result = await _service.DeactivateEducatorAsync(educatorId);

        // Assert
        result.Should().BeTrue();
        
        // Verify timestamp was updated
        var updatedEducator = await _context.Educators.FindAsync(educatorId);
        updatedEducator.Should().NotBeNull();
        updatedEducator!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
