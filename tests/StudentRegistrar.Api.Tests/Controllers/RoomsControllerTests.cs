using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using StudentRegistrar.Api.Controllers;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Controllers;

public class RoomsControllerTests
{
    private readonly Mock<IRoomService> _mockRoomService;
    private readonly Mock<ILogger<RoomsController>> _mockLogger;
    private readonly RoomsController _controller;

    public RoomsControllerTests()
    {
        _mockRoomService = new Mock<IRoomService>();
        _mockLogger = new Mock<ILogger<RoomsController>>();
        _controller = new RoomsController(_mockRoomService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetRooms_Should_ReturnOkWithRooms()
    {
        // Arrange
        var expectedRooms = new List<RoomDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Math Lab A", Capacity = 25, RoomType = RoomType.Lab, CourseCount = 2 },
            new() { Id = Guid.NewGuid(), Name = "Classroom 101", Capacity = 30, RoomType = RoomType.Classroom, CourseCount = 3 }
        };

        _mockRoomService
            .Setup(s => s.GetAllRoomsAsync())
            .Returns(Task.FromResult<IEnumerable<RoomDto>>(expectedRooms));

        // Act
        var result = await _controller.GetRooms();

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedRooms);
    }

    [Fact]
    public async Task GetRoom_Should_ReturnOkWithRoom_WhenRoomExists()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var expectedRoom = new RoomDto 
        { 
            Id = roomId, 
            Name = "Science Lab", 
            Capacity = 20, 
            RoomType = RoomType.Lab,
            Notes = "Equipped with lab benches",
            CourseCount = 1
        };

        _mockRoomService
            .Setup(s => s.GetRoomByIdAsync(roomId))
            .Returns(Task.FromResult<RoomDto?>(expectedRoom));

        // Act
        var result = await _controller.GetRoom(roomId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedRoom);
    }

    [Fact]
    public async Task GetRoom_Should_ReturnNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _mockRoomService
            .Setup(s => s.GetRoomByIdAsync(roomId))
            .Returns(Task.FromResult<RoomDto?>(null));

        // Act
        var result = await _controller.GetRoom(roomId);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetRoomsByType_Should_ReturnOkWithFilteredRooms()
    {
        // Arrange
        var roomType = RoomType.Lab;
        var expectedRooms = new List<RoomDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Lab A", Capacity = 20, RoomType = RoomType.Lab, CourseCount = 1 },
            new() { Id = Guid.NewGuid(), Name = "Lab B", Capacity = 25, RoomType = RoomType.Lab, CourseCount = 2 }
        };

        _mockRoomService
            .Setup(s => s.GetRoomsByTypeAsync(roomType))
            .Returns(Task.FromResult<IEnumerable<RoomDto>>(expectedRooms));

        // Act
        var result = await _controller.GetRoomsByType(roomType);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedRooms);
    }

    [Fact]
    public async Task CreateRoom_Should_ReturnCreatedAtAction_WhenValidDto()
    {
        // Arrange
        var createDto = new CreateRoomDto
        {
            Name = "New Classroom",
            Capacity = 25,
            RoomType = RoomType.Classroom,
            Notes = "Brand new classroom"
        };

        var expectedRoom = new RoomDto
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Capacity = createDto.Capacity,
            RoomType = createDto.RoomType,
            Notes = createDto.Notes,
            CourseCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockRoomService
            .Setup(s => s.CreateRoomAsync(createDto))
            .Returns(Task.FromResult(expectedRoom));

        // Act
        var result = await _controller.CreateRoom(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = actionResult as CreatedAtActionResult;
        createdResult!.Value.Should().BeEquivalentTo(expectedRoom);
        createdResult.ActionName.Should().Be(nameof(RoomsController.GetRoom));
        createdResult.RouteValues!["id"].Should().Be(expectedRoom.Id);
    }

    [Fact]
    public async Task CreateRoom_Should_ReturnBadRequest_WhenServiceThrowsInvalidOperationException()
    {
        // Arrange
        var createDto = new CreateRoomDto
        {
            Name = "Duplicate Room",
            Capacity = 25,
            RoomType = RoomType.Classroom
        };

        var errorMessage = "A room with this name already exists";
        _mockRoomService
            .Setup(s => s.CreateRoomAsync(createDto))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.CreateRoom(createDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = actionResult as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(errorMessage);
    }

    [Fact]
    public async Task UpdateRoom_Should_ReturnOkWithUpdatedRoom_WhenValidDto()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var updateDto = new UpdateRoomDto
        {
            Name = "Updated Room",
            Capacity = 30,
            RoomType = RoomType.Lab,
            Notes = "Updated notes"
        };

        var expectedRoom = new RoomDto
        {
            Id = roomId,
            Name = updateDto.Name,
            Capacity = updateDto.Capacity,
            RoomType = updateDto.RoomType,
            Notes = updateDto.Notes,
            CourseCount = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        _mockRoomService
            .Setup(s => s.UpdateRoomAsync(roomId, updateDto))
            .Returns(Task.FromResult<RoomDto?>(expectedRoom));

        // Act
        var result = await _controller.UpdateRoom(roomId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedRoom);
    }

    [Fact]
    public async Task UpdateRoom_Should_ReturnNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var updateDto = new UpdateRoomDto
        {
            Name = "Non-existent Room",
            Capacity = 25,
            RoomType = RoomType.Classroom
        };

        _mockRoomService
            .Setup(s => s.UpdateRoomAsync(roomId, updateDto))
            .Returns(Task.FromResult<RoomDto?>(null));

        // Act
        var result = await _controller.UpdateRoom(roomId, updateDto);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task UpdateRoom_Should_ReturnBadRequest_WhenServiceThrowsInvalidOperationException()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var updateDto = new UpdateRoomDto
        {
            Name = "Duplicate Room",
            Capacity = 25,
            RoomType = RoomType.Classroom
        };

        var errorMessage = "A room with this name already exists";
        _mockRoomService
            .Setup(s => s.UpdateRoomAsync(roomId, updateDto))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.UpdateRoom(roomId, updateDto);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = actionResult as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(errorMessage);
    }

    [Fact]
    public async Task DeleteRoom_Should_ReturnNoContent_WhenRoomDeletedSuccessfully()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _mockRoomService
            .Setup(s => s.DeleteRoomAsync(roomId))
            .Returns(Task.FromResult(true));

        // Act
        var result = await _controller.DeleteRoom(roomId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteRoom_Should_ReturnNotFound_WhenRoomDoesNotExist()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        _mockRoomService
            .Setup(s => s.DeleteRoomAsync(roomId))
            .Returns(Task.FromResult(false));

        // Act
        var result = await _controller.DeleteRoom(roomId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteRoom_Should_ReturnBadRequest_WhenRoomIsInUse()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var errorMessage = "Cannot delete room because it is currently assigned to one or more courses";
        _mockRoomService
            .Setup(s => s.DeleteRoomAsync(roomId))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.DeleteRoom(roomId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().Be(errorMessage);
    }

    [Fact]
    public async Task IsRoomInUse_Should_ReturnOkWithBoolean()
    {
        // Arrange
        var roomId = Guid.NewGuid();
        var expectedResult = true;
        _mockRoomService
            .Setup(s => s.IsRoomInUseAsync(roomId))
            .Returns(Task.FromResult(expectedResult));

        // Act
        var result = await _controller.IsRoomInUse(roomId);

        // Assert
        var actionResult = result.Result;
        actionResult.Should().BeOfType<OkObjectResult>();
        var okResult = actionResult as OkObjectResult;
        okResult!.Value.Should().Be(expectedResult);
    }
}
