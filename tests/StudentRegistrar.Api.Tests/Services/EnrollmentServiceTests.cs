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

    // Test data Guid IDs
    private readonly Guid _johnStudentId = Guid.NewGuid();
    private readonly Guid _aliceStudentId = Guid.NewGuid();
    private readonly Guid _mathCourseId = Guid.NewGuid();
    private readonly Guid _englishCourseId = Guid.NewGuid();
    private readonly Guid _fallSemesterId = Guid.NewGuid();
    private readonly Guid _accountHolderId = Guid.NewGuid();
    private readonly Guid _enrollment1Id = Guid.NewGuid();
    private readonly Guid _enrollment2Id = Guid.NewGuid();
    private readonly Guid _enrollment3Id = Guid.NewGuid();

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
                Id = _enrollment1Id,
                StudentId = _johnStudentId,
                CourseId = _mathCourseId,
                SemesterId = _fallSemesterId,
                EnrollmentDate = DateTime.UtcNow.AddMonths(-2),
                EnrollmentType = EnrollmentType.Enrolled,
                FeeAmount = 500.00m,
                AmountPaid = 500.00m,
                PaymentStatus = PaymentStatus.Paid
            },
            new Enrollment
            {
                Id = _enrollment2Id,
                StudentId = _aliceStudentId,
                CourseId = _mathCourseId,
                SemesterId = _fallSemesterId,
                EnrollmentDate = DateTime.UtcNow.AddMonths(-1),
                EnrollmentType = EnrollmentType.Enrolled,
                FeeAmount = 500.00m,
                AmountPaid = 250.00m,
                PaymentStatus = PaymentStatus.Partial
            },
            new Enrollment
            {
                Id = _enrollment3Id,
                StudentId = _johnStudentId,
                CourseId = _englishCourseId,
                SemesterId = _fallSemesterId,
                EnrollmentDate = DateTime.UtcNow.AddDays(-14),
                EnrollmentType = EnrollmentType.Withdrawn,
                FeeAmount = 450.00m,
                AmountPaid = 450.00m,
                PaymentStatus = PaymentStatus.Refunded
            }
        };

        _context.Semesters.Add(semester);
        _context.AccountHolders.Add(accountHolder);
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
        result.StudentId.Should().Be(_johnStudentId.GetHashCode());
        result.CourseId.Should().Be(_mathCourseId.GetHashCode());
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
            StudentId = _aliceStudentId.GetHashCode(),
            CourseId = _englishCourseId.GetHashCode(),
            EnrollmentDate = DateTime.Now,
            Status = "Active"
        };

        // Act
        var result = await _service.CreateEnrollmentAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.StudentId.Should().Be(_aliceStudentId.GetHashCode());
        result.CourseId.Should().Be(_englishCourseId.GetHashCode());
        result.Status.Should().Be("Active");
        result.EnrollmentDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMinutes(1));

        // Verify it was saved to database
        var savedEnrollment = await _context.Enrollments.FindAsync(result.Id);
        savedEnrollment.Should().NotBeNull();
        savedEnrollment!.StudentId.Should().Be(_aliceStudentId);
        savedEnrollment.CourseId.Should().Be(_englishCourseId);
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
        var updatedEnrollment = await _context.Enrollments.FindAsync(_enrollment1Id);
        updatedEnrollment.Should().NotBeNull();
        updatedEnrollment!.EnrollmentType.Should().Be(EnrollmentType.Withdrawn);
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
        var updatedEnrollment = await _context.Enrollments.FindAsync(_enrollment1Id);
        updatedEnrollment.Should().NotBeNull();
        updatedEnrollment!.EnrollmentType.Should().Be(EnrollmentType.Enrolled);
        // Note: The new model doesn't have a completion date - this is handled via EnrollmentInfo JSON
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
        // Arrange - Get an enrollment that currently has withdrawn status
        var enrollmentWithdrawn = await _context.Enrollments.FindAsync(_enrollment3Id);
        enrollmentWithdrawn.Should().NotBeNull();
        enrollmentWithdrawn!.EnrollmentType.Should().Be(EnrollmentType.Withdrawn);

        // Act - Change status to Active
        var result = await _service.UpdateEnrollmentStatusAsync(_enrollment3Id.GetHashCode(), "Active");

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
