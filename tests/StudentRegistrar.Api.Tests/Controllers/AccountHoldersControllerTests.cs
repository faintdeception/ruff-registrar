using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class AccountHoldersControllerTests
{
    private readonly Mock<IAccountHolderService> _mockAccountHolderService;
    private readonly Mock<ILogger<AccountHoldersController>> _mockLogger;
    private readonly StudentRegistrarDbContext _context;
    private readonly AccountHoldersController _controller;

    public AccountHoldersControllerTests()
    {
        _mockAccountHolderService = new Mock<IAccountHolderService>();
        _mockLogger = new Mock<ILogger<AccountHoldersController>>();
        
        // Create an in-memory database context
        var options = new DbContextOptionsBuilder<StudentRegistrarDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new StudentRegistrarDbContext(options);
        
        _controller = new AccountHoldersController(_mockAccountHolderService.Object, _mockLogger.Object, _context);
        
        // Set up a mock user context
        SetupMockUserContext();
    }

    [Fact]
    public async Task GetMyAccountHolder_Should_ReturnAccountHolder_WhenUserExists()
    {
        // Arrange
        var expectedAccountHolder = new AccountHolderDto
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = "John",
            LastName = "Doe",
            EmailAddress = "john.doe@example.com"
        };

        _mockAccountHolderService
            .Setup(s => s.GetAccountHolderByUserIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<AccountHolderDto?>(expectedAccountHolder));

        // Act
        var result = await _controller.GetMyAccountHolder();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedAccountHolder);
    }

    [Fact]
    public async Task GetMyAccountHolder_Should_ReturnBadRequest_WhenUserDoesNotExistAndInsufficientClaimsForAutoCreation()
    {
        // Arrange
        _mockAccountHolderService
            .Setup(s => s.GetAccountHolderByUserIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<AccountHolderDto?>(null));

        // Act
        var result = await _controller.GetMyAccountHolder();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = actionResult as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be("Insufficient user information in token to create account holder");
    }

    [Fact]
    public async Task CreateAccountHolder_Should_ReturnCreatedAccountHolder_WhenValidData()
    {
        // Arrange
        var createDto = new CreateAccountHolderDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            EmailAddress = "jane.smith@example.com"
        };

        var createdAccountHolder = new AccountHolderDto
        {
            Id = Guid.NewGuid().ToString(),
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            EmailAddress = createDto.EmailAddress
        };

        _mockAccountHolderService
            .Setup(s => s.CreateAccountHolderAsync(createDto))
            .Returns(Task.FromResult(createdAccountHolder));

        // Act
        var result = await _controller.CreateAccountHolder(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdAccountHolder);
    }

    [Fact]
    public async Task AddStudentToMyAccount_Should_ReturnCreatedStudent_WhenValidData()
    {
        // Arrange
        var accountHolderId = Guid.NewGuid();
        var createStudentDto = new CreateStudentForAccountDto
        {
            FirstName = "Student",
            LastName = "Test"
        };

        var accountHolder = new AccountHolderDto { Id = accountHolderId.ToString() };
        var createdStudent = new StudentDto
        {
            Id = 1,
            FirstName = createStudentDto.FirstName,
            LastName = createStudentDto.LastName,
            Email = "student.test@example.com",
            DateOfBirth = DateOnly.FromDateTime(DateTime.Now.AddYears(-10))
        };

        _mockAccountHolderService
            .Setup(s => s.GetAccountHolderByUserIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult<AccountHolderDto?>(accountHolder));

        _mockAccountHolderService
            .Setup(s => s.AddStudentToAccountAsync(accountHolderId, createStudentDto))
            .Returns(Task.FromResult(createdStudent));

        // Act
        var result = await _controller.AddStudentToMyAccount(createStudentDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(createdStudent);
    }

    private void SetupMockUserContext()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "test-user-id"),
            new(ClaimTypes.Email, "test@example.com")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }
}
