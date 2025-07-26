using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class GradesControllerTests
{
    private readonly Mock<IGradeService> _mockGradeService;
    private readonly GradesController _controller;

    public GradesControllerTests()
    {
        _mockGradeService = new Mock<IGradeService>();
        _controller = new GradesController(_mockGradeService.Object);
    }

    [Fact]
    public async Task GetGrades_Should_ReturnOkWithGrades()
    {
        // Arrange
        var expectedGrades = new List<GradeRecordDto>
        {
            new() { Id = 1, StudentId = 101, CourseId = 201, LetterGrade = "A", Comments = "Excellent work" },
            new() { Id = 2, StudentId = 102, CourseId = 202, LetterGrade = "B+", Comments = "Good performance" }
        };

        _mockGradeService
            .Setup(s => s.GetAllGradesAsync())
            .Returns(Task.FromResult<IEnumerable<GradeRecordDto>>(expectedGrades));

        // Act
        var result = await _controller.GetGrades();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGrades);
    }

    [Fact]
    public async Task GetGrade_Should_ReturnOkWithGrade_WhenGradeExists()
    {
        // Arrange
        var gradeId = 1;
        var expectedGrade = new GradeRecordDto 
        { 
            Id = gradeId, 
            StudentId = 101, 
            CourseId = 201, 
            LetterGrade = "A",
            Comments = "Excellent work",
            GradeDate = DateTime.Now
        };

        _mockGradeService
            .Setup(s => s.GetGradeByIdAsync(gradeId))
            .Returns(Task.FromResult<GradeRecordDto?>(expectedGrade));

        // Act
        var result = await _controller.GetGrade(gradeId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGrade);
    }

    [Fact]
    public async Task GetGrade_Should_ReturnNotFound_WhenGradeDoesNotExist()
    {
        // Arrange
        var gradeId = 999;

        _mockGradeService
            .Setup(s => s.GetGradeByIdAsync(gradeId))
            .Returns(Task.FromResult<GradeRecordDto?>(null));

        // Act
        var result = await _controller.GetGrade(gradeId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateGrade_Should_ReturnCreatedGrade_WhenValidData()
    {
        // Arrange
        var createDto = new CreateGradeRecordDto
        {
            StudentId = 101,
            CourseId = 201,
            LetterGrade = "A",
            Comments = "Excellent work",
            GradeDate = DateTime.Now
        };

        var createdGrade = new GradeRecordDto
        {
            Id = 3,
            StudentId = createDto.StudentId,
            CourseId = createDto.CourseId,
            LetterGrade = createDto.LetterGrade,
            Comments = createDto.Comments,
            GradeDate = createDto.GradeDate
        };

        _mockGradeService
            .Setup(s => s.CreateGradeAsync(createDto))
            .Returns(Task.FromResult(createdGrade));

        // Act
        var result = await _controller.CreateGrade(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdGrade);
    }

    [Fact]
    public async Task UpdateGrade_Should_ReturnOkWithUpdatedGrade_WhenGradeExists()
    {
        // Arrange
        var gradeId = 1;
        var updateDto = new CreateGradeRecordDto
        {
            StudentId = 101,
            CourseId = 201,
            LetterGrade = "A-",
            Comments = "Updated grade",
            GradeDate = DateTime.Now
        };

        var updatedGrade = new GradeRecordDto
        {
            Id = gradeId,
            StudentId = updateDto.StudentId,
            CourseId = updateDto.CourseId,
            LetterGrade = updateDto.LetterGrade,
            Comments = updateDto.Comments,
            GradeDate = updateDto.GradeDate
        };

        _mockGradeService
            .Setup(s => s.UpdateGradeAsync(gradeId, updateDto))
            .Returns(Task.FromResult<GradeRecordDto?>(updatedGrade));

        // Act
        var result = await _controller.UpdateGrade(gradeId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedGrade);
    }

    [Fact]
    public async Task UpdateGrade_Should_ReturnNotFound_WhenGradeDoesNotExist()
    {
        // Arrange
        var gradeId = 999;
        var updateDto = new CreateGradeRecordDto
        {
            StudentId = 101,
            CourseId = 201,
            LetterGrade = "A-",
            Comments = "Updated grade",
            GradeDate = DateTime.Now
        };

        _mockGradeService
            .Setup(s => s.UpdateGradeAsync(gradeId, updateDto))
            .Returns(Task.FromResult<GradeRecordDto?>(null));

        // Act
        var result = await _controller.UpdateGrade(gradeId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteGrade_Should_ReturnNoContent_WhenGradeDeletedSuccessfully()
    {
        // Arrange
        var gradeId = 1;

        _mockGradeService
            .Setup(s => s.DeleteGradeAsync(gradeId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteGrade(gradeId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteGrade_Should_ReturnNotFound_WhenGradeNotFound()
    {
        // Arrange
        var gradeId = 999;

        _mockGradeService
            .Setup(s => s.DeleteGradeAsync(gradeId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteGrade(gradeId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
