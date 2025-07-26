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
        var expectedCourses = new List<CourseDto>
        {
            new() { Id = 1, Name = "Mathematics 101", Code = "MATH101", CreditHours = 3 },
            new() { Id = 2, Name = "Physics 201", Code = "PHYS201", CreditHours = 4 }
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
        var courseId = 1;
        var expectedCourse = new CourseDto 
        { 
            Id = courseId, 
            Name = "Mathematics 101", 
            Code = "MATH101", 
            CreditHours = 3 
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
        var courseId = 999;

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
        var createDto = new CreateCourseDto
        {
            Name = "New Course",
            Code = "NEW101",
            CreditHours = 3,
            Instructor = "Dr. Smith",
            AcademicYear = "2024-25",
            Semester = "Fall"
        };

        var createdCourse = new CourseDto
        {
            Id = 3,
            Name = createDto.Name,
            Code = createDto.Code,
            CreditHours = createDto.CreditHours,
            Instructor = createDto.Instructor,
            AcademicYear = createDto.AcademicYear,
            Semester = createDto.Semester
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
        var courseId = 1;
        var updateDto = new UpdateCourseDto
        {
            Name = "Updated Course",
            Code = "UPD101",
            CreditHours = 4,
            Instructor = "Dr. Updated",
            AcademicYear = "2024-25",
            Semester = "Spring"
        };

        var updatedCourse = new CourseDto
        {
            Id = courseId,
            Name = updateDto.Name,
            Code = updateDto.Code,
            CreditHours = updateDto.CreditHours,
            Instructor = updateDto.Instructor,
            AcademicYear = updateDto.AcademicYear,
            Semester = updateDto.Semester
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
