using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class SemestersControllerTests
{
    private readonly Mock<ISemesterService> _mockSemesterService;
    private readonly Mock<ILogger<SemestersController>> _mockLogger;
    private readonly SemestersController _controller;

    public SemestersControllerTests()
    {
        _mockSemesterService = new Mock<ISemesterService>();
        _mockLogger = new Mock<ILogger<SemestersController>>();
        _controller = new SemestersController(_mockSemesterService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetSemesters_Should_ReturnOkWithSemesters()
    {
        // Arrange
        var expectedSemesters = new List<SemesterDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Fall 2024", Code = "F24", IsActive = true },
            new() { Id = Guid.NewGuid(), Name = "Spring 2025", Code = "S25", IsActive = false }
        };

        _mockSemesterService
            .Setup(s => s.GetAllSemestersAsync())
            .Returns(Task.FromResult<IEnumerable<SemesterDto>>(expectedSemesters));

        // Act
        var result = await _controller.GetSemesters();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSemesters);
    }

    [Fact]
    public async Task GetSemester_Should_ReturnOkWithSemester_WhenSemesterExists()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var expectedSemester = new SemesterDto 
        { 
            Id = semesterId, 
            Name = "Fall 2024", 
            Code = "F24", 
            IsActive = true 
        };

        _mockSemesterService
            .Setup(s => s.GetSemesterByIdAsync(semesterId))
            .Returns(Task.FromResult<SemesterDto?>(expectedSemester));

        // Act
        var result = await _controller.GetSemester(semesterId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSemester);
    }

    [Fact]
    public async Task GetSemester_Should_ReturnNotFound_WhenSemesterDoesNotExist()
    {
        // Arrange
        var semesterId = Guid.NewGuid();

        _mockSemesterService
            .Setup(s => s.GetSemesterByIdAsync(semesterId))
            .Returns(Task.FromResult<SemesterDto?>(null));

        // Act
        var result = await _controller.GetSemester(semesterId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetActiveSemester_Should_ReturnOkWithSemester_WhenActiveSemesterExists()
    {
        // Arrange
        var expectedSemester = new SemesterDto 
        { 
            Id = Guid.NewGuid(), 
            Name = "Fall 2024", 
            Code = "F24", 
            IsActive = true 
        };

        _mockSemesterService
            .Setup(s => s.GetActiveSemesterAsync())
            .Returns(Task.FromResult<SemesterDto?>(expectedSemester));

        // Act
        var result = await _controller.GetActiveSemester();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSemester);
    }

    [Fact]
    public async Task GetActiveSemester_Should_ReturnNotFound_WhenNoActiveSemesterExists()
    {
        // Arrange
        _mockSemesterService
            .Setup(s => s.GetActiveSemesterAsync())
            .Returns(Task.FromResult<SemesterDto?>(null));

        // Act
        var result = await _controller.GetActiveSemester();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = actionResult as NotFoundObjectResult;
        notFoundResult!.Value.Should().Be("No active semester found");
    }

    [Fact]
    public async Task CreateSemester_Should_ReturnCreatedSemester_WhenValidData()
    {
        // Arrange
        var createDto = new CreateSemesterDto
        {
            Name = "Summer 2024",
            Code = "SU24",
            StartDate = DateTime.Now.AddDays(30),
            EndDate = DateTime.Now.AddDays(120),
            RegistrationStartDate = DateTime.Now.AddDays(10),
            RegistrationEndDate = DateTime.Now.AddDays(20),
            IsActive = true
        };

        var createdSemester = new SemesterDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Code = createDto.Code,
            StartDate = createDto.StartDate,
            EndDate = createDto.EndDate,
            RegistrationStartDate = createDto.RegistrationStartDate,
            RegistrationEndDate = createDto.RegistrationEndDate,
            IsActive = createDto.IsActive
        };

        _mockSemesterService
            .Setup(s => s.CreateSemesterAsync(createDto))
            .Returns(Task.FromResult(createdSemester));

        // Act
        var result = await _controller.CreateSemester(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdSemester);
    }

    [Fact]
    public async Task UpdateSemester_Should_ReturnOkWithUpdatedSemester_WhenSemesterExists()
    {
        // Arrange
        var semesterId = Guid.NewGuid();
        var updateDto = new UpdateSemesterDto
        {
            Name = "Updated Semester",
            Code = "UPD24",
            StartDate = DateTime.Now.AddDays(40),
            EndDate = DateTime.Now.AddDays(130),
            RegistrationStartDate = DateTime.Now.AddDays(15),
            RegistrationEndDate = DateTime.Now.AddDays(25),
            IsActive = false
        };

        var updatedSemester = new SemesterDto
        {
            Id = semesterId,
            Name = updateDto.Name,
            Code = updateDto.Code,
            StartDate = updateDto.StartDate,
            EndDate = updateDto.EndDate,
            RegistrationStartDate = updateDto.RegistrationStartDate,
            RegistrationEndDate = updateDto.RegistrationEndDate,
            IsActive = updateDto.IsActive
        };

        _mockSemesterService
            .Setup(s => s.UpdateSemesterAsync(semesterId, updateDto))
            .Returns(Task.FromResult<SemesterDto?>(updatedSemester));

        // Act
        var result = await _controller.UpdateSemester(semesterId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(updatedSemester);
    }

    [Fact]
    public async Task DeleteSemester_Should_ReturnNoContent_WhenSemesterDeletedSuccessfully()
    {
        // Arrange
        var semesterId = Guid.NewGuid();

        _mockSemesterService
            .Setup(s => s.DeleteSemesterAsync(semesterId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteSemester(semesterId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteSemester_Should_ReturnNotFound_WhenSemesterNotFound()
    {
        // Arrange
        var semesterId = Guid.NewGuid();

        _mockSemesterService
            .Setup(s => s.DeleteSemesterAsync(semesterId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteSemester(semesterId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
