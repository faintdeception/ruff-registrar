using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class SemesterServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly SemesterService _service;

    public SemesterServiceTests()
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

        _service = new SemesterService(_context, _mapper);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var semesters = new List<Semester>
        {
            new Semester
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Fall 2024",
                Code = "FALL2024",
                StartDate = new DateTime(2024, 8, 15),
                EndDate = new DateTime(2024, 12, 15),
                RegistrationStartDate = new DateTime(2024, 7, 1),
                RegistrationEndDate = new DateTime(2024, 8, 10),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                UpdatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new Semester
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "Spring 2025",
                Code = "SPRING2025",
                StartDate = new DateTime(2025, 1, 15),
                EndDate = new DateTime(2025, 5, 15),
                RegistrationStartDate = new DateTime(2024, 11, 1),
                RegistrationEndDate = new DateTime(2025, 1, 10),
                IsActive = false,
                CreatedAt = DateTime.UtcNow.AddMonths(-3),
                UpdatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new Semester
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Active Semester",
                Code = "ACTIVE2025",
                StartDate = DateTime.UtcNow.AddDays(-30),
                EndDate = DateTime.UtcNow.AddDays(30),
                RegistrationStartDate = DateTime.UtcNow.AddDays(-60),
                RegistrationEndDate = DateTime.UtcNow.AddDays(-35),
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-2),
                UpdatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        _context.Semesters.AddRange(semesters);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllSemestersAsync_Should_ReturnAllSemesters_OrderedByStartDateDescending()
    {
        // Act
        var result = await _service.GetAllSemestersAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var semesterList = result.ToList();
        // Should be ordered by start date descending (most recent first)
        semesterList[0].Name.Should().Be("Active Semester");
        semesterList[1].Name.Should().Be("Spring 2025");
        semesterList[2].Name.Should().Be("Fall 2024");
    }

    [Fact]
    public async Task GetSemesterByIdAsync_Should_ReturnSemester_WhenSemesterExists()
    {
        // Arrange
        var semesterId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetSemesterByIdAsync(semesterId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(semesterId);
        result.Name.Should().Be("Fall 2024");
        result.Code.Should().Be("FALL2024");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetSemesterByIdAsync_Should_ReturnNull_WhenSemesterDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.GetSemesterByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveSemesterAsync_Should_ReturnActiveSemester_WhenOneExists()
    {
        // Act
        var result = await _service.GetActiveSemesterAsync();

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Active Semester");
        result.Code.Should().Be("ACTIVE2025");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveSemesterAsync_Should_ReturnNull_WhenNoActiveSemesterExists()
    {
        // Arrange - Remove the active semester or modify dates to be outside current range
        var activeSemester = await _context.Semesters.FindAsync(Guid.Parse("33333333-3333-3333-3333-333333333333"));
        if (activeSemester != null)
        {
            activeSemester.EndDate = DateTime.UtcNow.AddDays(-1); // Make it expired
            await _context.SaveChangesAsync();
        }

        // Act
        var result = await _service.GetActiveSemesterAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateSemesterAsync_Should_CreateAndReturnSemester()
    {
        // Arrange
        var createDto = new CreateSemesterDto
        {
            Name = "Summer 2025",
            Code = "SUMMER2025",
            StartDate = new DateTime(2025, 6, 1),
            EndDate = new DateTime(2025, 8, 31),
            RegistrationStartDate = new DateTime(2025, 4, 1),
            RegistrationEndDate = new DateTime(2025, 5, 25),
            IsActive = true
        };

        // Act
        var result = await _service.CreateSemesterAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Summer 2025");
        result.Code.Should().Be("SUMMER2025");
        result.IsActive.Should().BeTrue();
        result.Id.Should().NotBe(Guid.Empty);

        // Verify it was saved to database
        var savedSemester = await _context.Semesters.FindAsync(result.Id);
        savedSemester.Should().NotBeNull();
        savedSemester!.Name.Should().Be("Summer 2025");
    }

    [Fact]
    public async Task UpdateSemesterAsync_Should_UpdateAndReturnSemester_WhenSemesterExists()
    {
        // Arrange
        var semesterId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var updateDto = new UpdateSemesterDto
        {
            Name = "Fall 2024 - Updated",
            Code = "FALL2024UPD",
            StartDate = new DateTime(2024, 8, 20),
            EndDate = new DateTime(2024, 12, 20),
            RegistrationStartDate = new DateTime(2024, 7, 5),
            RegistrationEndDate = new DateTime(2024, 8, 15),
            IsActive = false
        };

        // Act
        var result = await _service.UpdateSemesterAsync(semesterId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(semesterId);
        result.Name.Should().Be("Fall 2024 - Updated");
        result.Code.Should().Be("FALL2024UPD");
        result.IsActive.Should().BeFalse();

        // Verify it was updated in database
        var updatedSemester = await _context.Semesters.FindAsync(semesterId);
        updatedSemester.Should().NotBeNull();
        updatedSemester!.Name.Should().Be("Fall 2024 - Updated");
        updatedSemester.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateSemesterAsync_Should_ReturnNull_WhenSemesterDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateSemesterDto
        {
            Name = "NonExistent Semester",
            Code = "NONE2025"
        };

        // Act
        var result = await _service.UpdateSemesterAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSemesterAsync_Should_ReturnTrue_WhenSemesterExists()
    {
        // Arrange
        var semesterId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        // Act
        var result = await _service.DeleteSemesterAsync(semesterId);

        // Assert
        result.Should().BeTrue();

        // Verify semester was deleted
        var deletedSemester = await _context.Semesters.FindAsync(semesterId);
        deletedSemester.Should().BeNull();
    }

    [Fact]
    public async Task DeleteSemesterAsync_Should_ReturnFalse_WhenSemesterDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.DeleteSemesterAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CreateSemesterAsync_Should_SetCreatedAtAndUpdatedAt()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var createDto = new CreateSemesterDto
        {
            Name = "Test Semester",
            Code = "TEST2025",
            StartDate = DateTime.UtcNow.AddDays(30),
            EndDate = DateTime.UtcNow.AddDays(120),
            RegistrationStartDate = DateTime.UtcNow.AddDays(1),
            RegistrationEndDate = DateTime.UtcNow.AddDays(25),
            IsActive = true
        };

        // Act
        var result = await _service.CreateSemesterAsync(createDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        result.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        result.CreatedAt.Should().BeOnOrBefore(afterCreate);
        result.UpdatedAt.Should().BeOnOrAfter(beforeCreate);
        result.UpdatedAt.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task UpdateSemesterAsync_Should_UpdateUpdatedAtTimestamp()
    {
        // Arrange
        var semesterId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var originalSemester = await _context.Semesters.FindAsync(semesterId);
        var originalUpdatedAt = originalSemester!.UpdatedAt;
        
        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);
        
        var updateDto = new UpdateSemesterDto
        {
            Name = "Updated Name",
            Code = "UPDATED",
            StartDate = originalSemester.StartDate,
            EndDate = originalSemester.EndDate,
            RegistrationStartDate = originalSemester.RegistrationStartDate,
            RegistrationEndDate = originalSemester.RegistrationEndDate,
            IsActive = originalSemester.IsActive
        };

        // Act
        var result = await _service.UpdateSemesterAsync(semesterId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
