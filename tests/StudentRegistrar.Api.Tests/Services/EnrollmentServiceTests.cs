using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class EnrollmentServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
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

        _service = new EnrollmentService(_context, _mapper);

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
                CourseId = 1,
                EnrollmentDate = DateTime.Now.AddMonths(-1),
                Status = "Active"
            },
            new LegacyEnrollment
            {
                Id = 3,
                StudentId = 1,
                CourseId = 2,
                EnrollmentDate = DateTime.Now.AddDays(-14),
                Status = "Completed",
                CompletionDate = DateTime.Now.AddDays(-7)
            }
        };

        _context.Students.AddRange(students);
        _context.Courses.AddRange(courses);
        _context.Enrollments.AddRange(enrollments);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllEnrollmentsAsync_Should_ReturnAllEnrollments_OrderedByEnrollmentDateDescending()
    {
        // Act
        var result = await _service.GetAllEnrollmentsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var enrollmentList = result.ToList();
        // Should be ordered by enrollment date descending (most recent first)
        enrollmentList[0].Id.Should().Be(3); // Most recent enrollment
        enrollmentList[1].Id.Should().Be(2);
        enrollmentList[2].Id.Should().Be(1); // Oldest enrollment
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_Should_ReturnEnrollment_WhenEnrollmentExists()
    {
        // Act
        var result = await _service.GetEnrollmentByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.StudentId.Should().Be(1);
        result.CourseId.Should().Be(1);
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task GetEnrollmentByIdAsync_Should_ReturnNull_WhenEnrollmentDoesNotExist()
    {
        // Act
        var result = await _service.GetEnrollmentByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateEnrollmentAsync_Should_CreateAndReturnEnrollment()
    {
        // Arrange
        var createDto = new CreateEnrollmentDto
        {
            StudentId = 2,
            CourseId = 2,
            EnrollmentDate = DateTime.Now,
            Status = "Active"
        };

        // Act
        var result = await _service.CreateEnrollmentAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(2);
        result.CourseId.Should().Be(2);
        result.Status.Should().Be("Active");
        result.EnrollmentDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

        // Verify it was saved to database
        var savedEnrollment = await _context.Enrollments.FindAsync(result.Id);
        savedEnrollment.Should().NotBeNull();
        savedEnrollment!.StudentId.Should().Be(2);
        savedEnrollment.CourseId.Should().Be(2);
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_Should_ReturnTrue_WhenEnrollmentExists()
    {
        // Act
        var result = await _service.DeleteEnrollmentAsync(1);

        // Assert
        result.Should().BeTrue();

        // Verify enrollment was deleted
        var deletedEnrollment = await _context.Enrollments.FindAsync(1);
        deletedEnrollment.Should().BeNull();
    }

    [Fact]
    public async Task DeleteEnrollmentAsync_Should_ReturnFalse_WhenEnrollmentDoesNotExist()
    {
        // Act
        var result = await _service.DeleteEnrollmentAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateEnrollmentStatusAsync_Should_UpdateStatusAndReturnEnrollment_WhenEnrollmentExists()
    {
        // Act
        var result = await _service.UpdateEnrollmentStatusAsync(1, "Dropped");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Status.Should().Be("Dropped");

        // Verify it was updated in database
        var updatedEnrollment = await _context.Enrollments.FindAsync(1);
        updatedEnrollment.Should().NotBeNull();
        updatedEnrollment!.Status.Should().Be("Dropped");
    }

    [Fact]
    public async Task UpdateEnrollmentStatusAsync_Should_SetCompletionDate_WhenStatusIsCompleted()
    {
        // Arrange
        var beforeUpdate = DateTime.UtcNow;

        // Act
        var result = await _service.UpdateEnrollmentStatusAsync(1, "Completed");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Completed");
        result.CompletionDate.Should().NotBeNull();
        result.CompletionDate.Should().BeOnOrAfter(beforeUpdate);

        // Verify it was updated in database
        var updatedEnrollment = await _context.Enrollments.FindAsync(1);
        updatedEnrollment.Should().NotBeNull();
        updatedEnrollment!.Status.Should().Be("Completed");
        updatedEnrollment.CompletionDate.Should().NotBeNull();
        updatedEnrollment.CompletionDate.Should().BeOnOrAfter(beforeUpdate);
    }

    [Fact]
    public async Task UpdateEnrollmentStatusAsync_Should_ReturnNull_WhenEnrollmentDoesNotExist()
    {
        // Act
        var result = await _service.UpdateEnrollmentStatusAsync(999, "Active");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateEnrollmentStatusAsync_Should_NotSetCompletionDate_WhenStatusIsNotCompleted()
    {
        // Arrange - Get an enrollment that currently has a completion date
        var enrollmentWithCompletion = await _context.Enrollments.FindAsync(3);
        enrollmentWithCompletion.Should().NotBeNull();
        enrollmentWithCompletion!.CompletionDate.Should().NotBeNull();

        // Act - Change status to something other than "Completed"
        var result = await _service.UpdateEnrollmentStatusAsync(3, "Active");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Active");
        // Completion date should remain as it was (service doesn't clear it)
        result.CompletionDate.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
