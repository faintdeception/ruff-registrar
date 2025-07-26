using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class StudentsControllerTests
{
    private readonly Mock<IStudentService> _mockStudentService;
    private readonly StudentsController _controller;

    public StudentsControllerTests()
    {
        _mockStudentService = new Mock<IStudentService>();
        _controller = new StudentsController(_mockStudentService.Object);
    }

    [Fact]
    public async Task GetStudents_Should_ReturnOkWithStudents()
    {
        // Arrange
        var expectedStudents = new List<StudentDto>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john@example.com" },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane@example.com" }
        };

        _mockStudentService
            .Setup(s => s.GetAllStudentsAsync())
            .Returns(Task.FromResult<IEnumerable<StudentDto>>(expectedStudents));

        // Act
        var result = await _controller.GetStudents();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStudents);
    }

    [Fact]
    public async Task GetStudent_Should_ReturnOkWithStudent_WhenStudentExists()
    {
        // Arrange
        var studentId = 1;
        var expectedStudent = new StudentDto 
        { 
            Id = studentId, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "john@example.com" 
        };

        _mockStudentService
            .Setup(s => s.GetStudentByIdAsync(studentId))
            .Returns(Task.FromResult<StudentDto?>(expectedStudent));

        // Act
        var result = await _controller.GetStudent(studentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedStudent);
    }

    [Fact]
    public async Task GetStudent_Should_ReturnNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = 999;

        _mockStudentService
            .Setup(s => s.GetStudentByIdAsync(studentId))
            .Returns(Task.FromResult<StudentDto?>(null));

        // Act
        var result = await _controller.GetStudent(studentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateStudent_Should_ReturnCreatedStudent_WhenValidData()
    {
        // Arrange
        var createDto = new CreateStudentDto
        {
            FirstName = "New",
            LastName = "Student",
            Email = "new.student@example.com",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-20))
        };

        var createdStudent = new StudentDto
        {
            Id = 3,
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email,
            DateOfBirth = createDto.DateOfBirth
        };

        _mockStudentService
            .Setup(s => s.CreateStudentAsync(createDto))
            .Returns(Task.FromResult(createdStudent));

        // Act
        var result = await _controller.CreateStudent(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdStudent);
    }

    [Fact]
    public async Task UpdateStudent_Should_ReturnOkWithUpdatedStudent_WhenStudentExists()
    {
        // Arrange
        var studentId = 1;
        var updateDto = new UpdateStudentDto
        {
            FirstName = "Updated",
            LastName = "Student",
            Email = "updated@example.com",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-21))
        };

        var updatedStudent = new StudentDto
        {
            Id = studentId,
            FirstName = updateDto.FirstName,
            LastName = updateDto.LastName,
            Email = updateDto.Email,
            DateOfBirth = updateDto.DateOfBirth
        };

        _mockStudentService
            .Setup(s => s.UpdateStudentAsync(studentId, updateDto))
            .Returns(Task.FromResult<StudentDto?>(updatedStudent));

        // Act
        var result = await _controller.UpdateStudent(studentId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedStudent);
    }

    [Fact]
    public async Task UpdateStudent_Should_ReturnNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        var studentId = 999;
        var updateDto = new UpdateStudentDto
        {
            FirstName = "Updated",
            LastName = "Student",
            Email = "updated@example.com",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-21))
        };

        _mockStudentService
            .Setup(s => s.UpdateStudentAsync(studentId, updateDto))
            .Returns(Task.FromResult<StudentDto?>(null));

        // Act
        var result = await _controller.UpdateStudent(studentId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteStudent_Should_ReturnNoContent_WhenStudentDeletedSuccessfully()
    {
        // Arrange
        var studentId = 1;

        _mockStudentService
            .Setup(s => s.DeleteStudentAsync(studentId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteStudent(studentId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteStudent_Should_ReturnNotFound_WhenStudentNotFound()
    {
        // Arrange
        var studentId = 999;

        _mockStudentService
            .Setup(s => s.DeleteStudentAsync(studentId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteStudent(studentId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetStudentEnrollments_Should_ReturnOkWithEnrollments()
    {
        // Arrange
        var studentId = 1;
        var expectedEnrollments = new List<EnrollmentDto>
        {
            new() { Id = 1, StudentId = studentId, CourseId = 101 },
            new() { Id = 2, StudentId = studentId, CourseId = 102 }
        };

        _mockStudentService
            .Setup(s => s.GetStudentEnrollmentsAsync(studentId))
            .Returns(Task.FromResult<IEnumerable<EnrollmentDto>>(expectedEnrollments));

        // Act
        var result = await _controller.GetStudentEnrollments(studentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedEnrollments);
    }

    [Fact]
    public async Task GetStudentGrades_Should_ReturnOkWithGrades()
    {
        // Arrange
        var studentId = 1;
        var expectedGrades = new List<GradeRecordDto>
        {
            new() { Id = 1, StudentId = studentId, CourseId = 101, LetterGrade = "A" },
            new() { Id = 2, StudentId = studentId, CourseId = 102, LetterGrade = "B+" }
        };

        _mockStudentService
            .Setup(s => s.GetStudentGradesAsync(studentId))
            .Returns(Task.FromResult<IEnumerable<GradeRecordDto>>(expectedGrades));

        // Act
        var result = await _controller.GetStudentGrades(studentId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedGrades);
    }
}
