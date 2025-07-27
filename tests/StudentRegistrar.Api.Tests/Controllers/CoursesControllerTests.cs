using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class CoursesControllerTests
{
    private readonly Mock<ICourseService> _mockCourseService;
    private readonly CoursesController _controller;

    public CoursesControllerTests()
    {
        _mockCourseService = new Mock<ICourseService>();
        _controller = new CoursesController(_mockCourseService.Object);
    }

    [Fact]
    public async Task GetCourses_Should_ReturnOkWithCourses()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var expectedCourses = new List<CourseDto>
        {
            new() { Id = Guid.NewGuid(), SemesterId = semesterId, Name = "Mathematics 101", Code = "MATH101", MaxCapacity = 25, Fee = 150.00m, AgeGroup = "Elementary" },
            new() { Id = Guid.NewGuid(), SemesterId = semesterId, Name = "Physics 201", Code = "PHYS201", MaxCapacity = 20, Fee = 200.00m, AgeGroup = "Middle School" }
        };

        _mockCourseService
            .Setup(s => s.GetAllCoursesAsync())
            .Returns(Task.FromResult<IEnumerable<CourseDto>>(expectedCourses));

        // Act
        var result = await _controller.GetCourses();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedCourses);
    }

    [Fact]
    public async Task GetCourse_Should_ReturnOkWithCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var semesterId = Guid.NewGuid();
        var expectedCourse = new CourseDto 
        { 
            Id = courseId, 
            SemesterId = semesterId,
            Name = "Mathematics 101", 
            Code = "MATH101", 
            MaxCapacity = 25,
            Fee = 150.00m,
            AgeGroup = "Elementary"
        };

        _mockCourseService
            .Setup(s => s.GetCourseByIdAsync(courseId))
            .Returns(Task.FromResult<CourseDto?>(expectedCourse));

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
            .Returns(Task.FromResult<CourseDto?>(null));

        // Act
        var result = await _controller.GetCourse(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateCourse_Should_ReturnCreatedCourse_WhenValidData()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var createDto = new CreateCourseDto
        {
            SemesterId = semesterId,
            Name = "New Course",
            Code = "NEW101",
            MaxCapacity = 25,
            Fee = 150.00m,
            AgeGroup = "Elementary",
            Room = "Room 101",
            PeriodCode = "P1"
        };

        var createdCourse = new CourseDto
        {
            Id = Guid.NewGuid(),
            SemesterId = semesterId,
            Name = createDto.Name,
            Code = createDto.Code,
            MaxCapacity = createDto.MaxCapacity,
            Fee = createDto.Fee,
            AgeGroup = createDto.AgeGroup,
            Room = createDto.Room,
            PeriodCode = createDto.PeriodCode
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
    public async Task UpdateCourse_Should_ReturnOkWithUpdatedCourse_WhenCourseExists()
    {
        // Arrange
        var courseId = Guid.NewGuid();
        var semesterId = Guid.NewGuid();
        var updateDto = new UpdateCourseDto
        {
            Name = "Updated Course",
            Code = "UPD101",
            MaxCapacity = 30,
            Fee = 175.00m,
            AgeGroup = "Middle School",
            Room = "Room 102",
            PeriodCode = "P2"
        };

        var updatedCourse = new CourseDto
        {
            Id = courseId,
            SemesterId = semesterId,
            Name = updateDto.Name,
            Code = updateDto.Code,
            MaxCapacity = updateDto.MaxCapacity,
            Fee = updateDto.Fee,
            AgeGroup = updateDto.AgeGroup,
            Room = updateDto.Room,
            PeriodCode = updateDto.PeriodCode
        };

        _mockCourseService
            .Setup(s => s.UpdateCourseAsync(courseId, updateDto))
            .Returns(Task.FromResult<CourseDto?>(updatedCourse));

        // Act
        var result = await _controller.UpdateCourse(courseId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedCourse);
    }

    [Fact]
    public async Task DeleteCourse_Should_ReturnNoContent_WhenCourseDeletedSuccessfully()
    {
        // Arrange
        var courseId = 1;

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
        var courseId = 999;

        _mockCourseService
            .Setup(s => s.DeleteCourseAsync(courseId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteCourse(courseId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetCourseEnrollments_Should_ReturnOkWithEnrollments()
    {
        // Arrange
        var courseId = 1;
        var expectedEnrollments = new List<EnrollmentDto>
        {
            new() { Id = 1, StudentId = 101, CourseId = courseId },
            new() { Id = 2, StudentId = 102, CourseId = courseId }
        };

        _mockCourseService
            .Setup(s => s.GetCourseEnrollmentsAsync(courseId))
            .Returns(Task.FromResult<IEnumerable<EnrollmentDto>>(expectedEnrollments));

        // Act
        var result = await _controller.GetCourseEnrollments(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedEnrollments);
    }

    [Fact]
    public async Task GetCourseGrades_Should_ReturnOkWithGrades()
    {
        // Arrange
        var courseId = 1;
        var expectedGrades = new List<GradeRecordDto>
        {
            new() { Id = 1, StudentId = 101, CourseId = courseId, LetterGrade = "A" },
            new() { Id = 2, StudentId = 102, CourseId = courseId, LetterGrade = "B+" }
        };

        _mockCourseService
            .Setup(s => s.GetCourseGradesAsync(courseId))
            .Returns(Task.FromResult<IEnumerable<GradeRecordDto>>(expectedGrades));

        // Act
        var result = await _controller.GetCourseGrades(courseId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGrades);
    }
}
