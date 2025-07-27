using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class EnrollmentsControllerTests
{
    private readonly Mock<IEnrollmentService> _mockEnrollmentService;
    private readonly EnrollmentsController _controller;

    public EnrollmentsControllerTests()
    {
        _mockEnrollmentService = new Mock<IEnrollmentService>();
        _controller = new EnrollmentsController(_mockEnrollmentService.Object);
    }

    [Fact]
    public async Task GetEnrollments_Should_ReturnOkWithEnrollments()
    {
        // Arrange
        var expectedEnrollments = new List<EnrollmentDto>
        {
            new() { Id = 1, StudentId = 101, CourseId = 201, Status = "Active" },
            new() { Id = 2, StudentId = 102, CourseId = 202, Status = "Completed" }
        };

        _mockEnrollmentService
            .Setup(s => s.GetAllEnrollmentsAsync())
            .Returns(Task.FromResult<IEnumerable<EnrollmentDto>>(expectedEnrollments));

        // Act
        var result = await _controller.GetEnrollments();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedEnrollments);
    }

    [Fact]
    public async Task GetEnrollment_Should_ReturnOkWithEnrollment_WhenEnrollmentExists()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var expectedEnrollment = new EnrollmentDto 
        { 
            Id = 1, 
            StudentId = 101, 
            CourseId = 201, 
            Status = "Active",
            EnrollmentDate = DateTime.Now
        };

        _mockEnrollmentService
            .Setup(s => s.GetEnrollmentByIdAsync(enrollmentId))
            .Returns(Task.FromResult<EnrollmentDto?>(expectedEnrollment));

        // Act
        var result = await _controller.GetEnrollment(enrollmentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedEnrollment);
    }

    [Fact]
    public async Task GetEnrollment_Should_ReturnNotFound_WhenEnrollmentDoesNotExist()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();

        _mockEnrollmentService
            .Setup(s => s.GetEnrollmentByIdAsync(enrollmentId))
            .Returns(Task.FromResult<EnrollmentDto?>(null));

        // Act
        var result = await _controller.GetEnrollment(enrollmentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateEnrollment_Should_ReturnCreatedEnrollment_WhenValidData()
    {
        // Arrange
        var createDto = new CreateEnrollmentDto
        {
            StudentId = 101,
            CourseId = 201,
            EnrollmentDate = DateTime.Now,
            Status = "Active"
        };

        var createdEnrollment = new EnrollmentDto
        {
            Id = 3,
            StudentId = createDto.StudentId,
            CourseId = createDto.CourseId,
            EnrollmentDate = createDto.EnrollmentDate,
            Status = createDto.Status
        };

        _mockEnrollmentService
            .Setup(s => s.CreateEnrollmentAsync(createDto))
            .Returns(Task.FromResult(createdEnrollment));

        // Act
        var result = await _controller.CreateEnrollment(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdEnrollment);
    }

    [Fact]
    public async Task UpdateEnrollmentStatus_Should_ReturnOkWithUpdatedEnrollment_WhenEnrollmentExists()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var updateDto = new UpdateEnrollmentStatusDto { Status = "Completed" };

        var updatedEnrollment = new EnrollmentDto
        {
            Id = 1,
            StudentId = 101,
            CourseId = 201,
            Status = updateDto.Status,
            EnrollmentDate = DateTime.Now
        };

        _mockEnrollmentService
            .Setup(s => s.UpdateEnrollmentStatusAsync(enrollmentId, updateDto.Status))
            .Returns(Task.FromResult<EnrollmentDto?>(updatedEnrollment));

        // Act
        var result = await _controller.UpdateEnrollmentStatus(enrollmentId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedEnrollment);
    }

    [Fact]
    public async Task UpdateEnrollmentStatus_Should_ReturnNotFound_WhenEnrollmentDoesNotExist()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var updateDto = new UpdateEnrollmentStatusDto { Status = "Completed" };

        _mockEnrollmentService
            .Setup(s => s.UpdateEnrollmentStatusAsync(enrollmentId, updateDto.Status))
            .Returns(Task.FromResult<EnrollmentDto?>(null));

        // Act
        var result = await _controller.UpdateEnrollmentStatus(enrollmentId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteEnrollment_Should_ReturnNoContent_WhenEnrollmentDeletedSuccessfully()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();

        _mockEnrollmentService
            .Setup(s => s.DeleteEnrollmentAsync(enrollmentId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteEnrollment(enrollmentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteEnrollment_Should_ReturnNotFound_WhenEnrollmentNotFound()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();

        _mockEnrollmentService
            .Setup(s => s.DeleteEnrollmentAsync(enrollmentId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteEnrollment(enrollmentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
