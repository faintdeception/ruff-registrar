using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class NewCoursesControllerTests
{
    private readonly Mock<INewCourseService> _mockCourseService;
    private readonly Mock<ILogger<NewCoursesController>> _mockLogger;
    private readonly NewCoursesController _controller;

    public NewCoursesControllerTests()
    {
        _mockCourseService = new Mock<INewCourseService>();
        _mockLogger = new Mock<ILogger<NewCoursesController>>();
        _controller = new NewCoursesController(_mockCourseService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetCourses_Should_ReturnOkWithAllCourses_WhenNoSemesterIdProvided()
    {
        // Arrange
        var expectedCourses = new List<NewCourseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Math 101", Fee = 100 },
            new() { Id = Guid.NewGuid(), Name = "English 101", Fee = 150 }
        };

        _mockCourseService
            .Setup(s => s.GetAllCoursesAsync())
            .Returns(Task.FromResult<IEnumerable<NewCourseDto>>(expectedCourses));

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCourses);
    }

    [Fact]
    public async Task GetCourses_Should_ReturnOkWithSemesterCourses_WhenSemesterIdProvided()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var expectedCourses = new List<NewCourseDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Math 101", Fee = 100, SemesterId = semesterId }
        };

        _mockCourseService
            .Setup(s => s.GetCoursesBySemesterAsync(semesterId))
            .Returns(Task.FromResult<IEnumerable<NewCourseDto>>(expectedCourses));

        // Act
        var result = await _controller.GetCourses(semesterId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCourses);
    }

    [Fact]
    public async Task GetCourses_Should_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        _mockCourseService
            .Setup(s => s.GetAllCoursesAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<ObjectResult>();
        var objectResult = actionResult as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task GetCourse_Should_ReturnOkWithCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var expectedCourse = new NewCourseDto 
        { 
            Id = courseId, 
            Name = "Math 101",
            Fee = 100,
            Description = "Introduction to Mathematics"
        };

        _mockCourseService
            .Setup(s => s.GetCourseByIdAsync(courseId))
            .Returns(Task.FromResult<NewCourseDto?>(expectedCourse));

        // Act
        var result = await _controller.GetCourse(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCourse);
    }

    [Fact]
    public async Task GetCourse_Should_ReturnNotFound_WhenCourseDoesNotExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseService
            .Setup(s => s.GetCourseByIdAsync(courseId))
            .Returns(Task.FromResult<NewCourseDto?>(null));

        // Act
        var result = await _controller.GetCourse(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCourse_Should_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseService
            .Setup(s => s.GetCourseByIdAsync(courseId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetCourse(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<ObjectResult>();
        var objectResult = actionResult as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task CreateCourse_Should_ReturnCreatedCourse_WhenValidData()
    {
        // Arrange
        var createDto = new CreateNewCourseDto
        {
            Name = "Math 101",
            Fee = 100,
            Description = "Introduction to Mathematics",
            SemesterId = Guid.NewGuid()
        };

        var createdCourse = new NewCourseDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Fee = createDto.Fee,
            Description = createDto.Description,
            SemesterId = createDto.SemesterId
        };

        _mockCourseService
            .Setup(s => s.CreateCourseAsync(createDto))
            .Returns(Task.FromResult(createdCourse));

        // Act
        var result = await _controller.CreateCourse(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdCourse);
    }

    [Fact]
    public async Task CreateCourse_Should_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var createDto = new CreateNewCourseDto
        {
            Name = "Math 101",
            Fee = 100
        };

        _mockCourseService
            .Setup(s => s.CreateCourseAsync(createDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateCourse(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<ObjectResult>();
        var objectResult = actionResult as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateCourse_Should_ReturnOkWithUpdatedCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var updateDto = new UpdateNewCourseDto
        {
            Name = "Math 102",
            Fee = 120,
            Description = "Advanced Mathematics"
        };

        var updatedCourse = new NewCourseDto
        {
            Id = courseId,
            Name = updateDto.Name,
            Fee = updateDto.Fee,
            Description = updateDto.Description
        };

        _mockCourseService
            .Setup(s => s.UpdateCourseAsync(courseId, updateDto))
            .Returns(Task.FromResult<NewCourseDto?>(updatedCourse));

        // Act
        var result = await _controller.UpdateCourse(courseId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedCourse);
    }

    [Fact]
    public async Task UpdateCourse_Should_ReturnNotFound_WhenCourseDoesNotExist()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var updateDto = new UpdateNewCourseDto
        {
            Name = "Math 102",
            Fee = 120
        };

        _mockCourseService
            .Setup(s => s.UpdateCourseAsync(courseId, updateDto))
            .Returns(Task.FromResult<NewCourseDto?>(null));

        // Act
        var result = await _controller.UpdateCourse(courseId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateCourse_Should_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var updateDto = new UpdateNewCourseDto
        {
            Name = "Math 102",
            Fee = 120
        };

        _mockCourseService
            .Setup(s => s.UpdateCourseAsync(courseId, updateDto))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateCourse(courseId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<ObjectResult>();
        var objectResult = actionResult as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task DeleteCourse_Should_ReturnNoContent_WhenCourseDeletedSuccessfully()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseService
            .Setup(s => s.DeleteCourseAsync(courseId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteCourse(courseId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCourse_Should_ReturnNotFound_WhenCourseNotFound()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseService
            .Setup(s => s.DeleteCourseAsync(courseId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteCourse(courseId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteCourse_Should_ReturnInternalServerError_WhenExceptionOccurs()
    {
        // Arrange
        var courseId = Guid.NewGuid();

        _mockCourseService
            .Setup(s => s.DeleteCourseAsync(courseId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.DeleteCourse(courseId);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }
}
