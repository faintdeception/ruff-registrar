using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class AccountHolderServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly AccountHolderService _service;

    public AccountHolderServiceTests()
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

        _service = new AccountHolderService(_context, _mapper);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var accountHolders = new List<AccountHolder>
        {
            new AccountHolder
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Smith",
                EmailAddress = "john.smith@example.com",
                HomePhone = "555-123-4567",
                MobilePhone = "555-987-6543",
                KeycloakUserId = "keycloak-user-1",
                MembershipDuesOwed = 100.00m,
                MembershipDuesReceived = 50.00m,
                MemberSince = DateTime.UtcNow.AddMonths(-12),
                CreatedAt = DateTime.UtcNow.AddMonths(-12),
                LastEdit = DateTime.UtcNow.AddDays(-5),
                AddressJson = """{"Street":"123 Main St","City":"Anytown","State":"CA","PostalCode":"12345","Country":"US"}""",
                EmergencyContactJson = """{"FirstName":"Jane","LastName":"Smith","HomePhone":"555-111-2222","Email":"jane.smith@example.com"}"""
            },
            new AccountHolder
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Alice",
                LastName = "Johnson",
                EmailAddress = "alice.johnson@example.com",
                HomePhone = "555-555-5555",
                KeycloakUserId = "keycloak-user-2",
                MembershipDuesOwed = 75.00m,
                MembershipDuesReceived = 75.00m,
                MemberSince = DateTime.UtcNow.AddMonths(-6),
                CreatedAt = DateTime.UtcNow.AddMonths(-6),
                LastEdit = DateTime.UtcNow.AddDays(-10),
                AddressJson = """{"Street":"456 Oak Ave","City":"Somewhere","State":"NY","PostalCode":"54321","Country":"US"}""",
                EmergencyContactJson = """{"FirstName":"Bob","LastName":"Johnson","MobilePhone":"555-333-4444","Email":"bob.johnson@example.com"}"""
            }
        };

        _context.AccountHolders.AddRange(accountHolders);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAccountHolderByUserIdAsync_Should_ReturnAccountHolder_WhenUserExists()
    {
        // Act
        var result = await _service.GetAccountHolderByUserIdAsync("keycloak-user-1");

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.EmailAddress.Should().Be("john.smith@example.com");
        result.HomePhone.Should().Be("555-123-4567");
        result.MobilePhone.Should().Be("555-987-6543");
    }

    [Fact]
    public async Task GetAccountHolderByUserIdAsync_Should_ReturnNull_WhenUserDoesNotExist()
    {
        // Act
        var result = await _service.GetAccountHolderByUserIdAsync("non-existent-user");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAccountHolderByIdAsync_Should_ReturnAccountHolder_WhenAccountHolderExists()
    {
        // Arrange
        var accountHolderId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // Act
        var result = await _service.GetAccountHolderByIdAsync(accountHolderId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountHolderId.ToString());
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Smith");
        result.EmailAddress.Should().Be("john.smith@example.com");
        result.MembershipDuesOwed.Should().Be(100.00m);
        result.MembershipDuesReceived.Should().Be(50.00m);
    }

    [Fact]
    public async Task GetAccountHolderByIdAsync_Should_ReturnNull_WhenAccountHolderDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        // Act
        var result = await _service.GetAccountHolderByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAccountHolderAsync_Should_CreateAndReturnAccountHolder()
    {
        // Arrange
        var createDto = new CreateAccountHolderDto
        {
            FirstName = "Michael",
            LastName = "Davis",
            EmailAddress = "michael.davis@example.com",
            HomePhone = "555-777-8888",
            MobilePhone = "555-999-0000",
            AddressJson = new AddressInfo
            {
                Street = "789 Pine St",
                City = "Newtown",
                State = "TX",
                PostalCode = "78901",
                Country = "US"
            },
            EmergencyContactJson = new EmergencyContactInfo
            {
                FirstName = "Sarah",
                LastName = "Davis",
                MobilePhone = "555-111-0000",
                Email = "sarah.davis@example.com"
            }
        };

        // Act
        var result = await _service.CreateAccountHolderAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Michael");
        result.LastName.Should().Be("Davis");
        result.EmailAddress.Should().Be("michael.davis@example.com");
        result.HomePhone.Should().Be("555-777-8888");
        result.Id.Should().NotBeEmpty();

        // Verify it was saved to database
        var savedAccountHolder = await _context.AccountHolders.FindAsync(Guid.Parse(result.Id));
        savedAccountHolder.Should().NotBeNull();
        savedAccountHolder!.FirstName.Should().Be("Michael");
    }

    [Fact]
    public async Task UpdateAccountHolderAsync_Should_UpdateAndReturnAccountHolder_WhenAccountHolderExists()
    {
        // Arrange
        var accountHolderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var updateDto = new UpdateAccountHolderDto
        {
            FirstName = "Jonathan",
            LastName = "Smith-Updated",
            EmailAddress = "jonathan.smith@example.com",
            HomePhone = "555-123-9999",
            MobilePhone = "555-987-0000",
            AddressJson = new AddressInfo
            {
                Street = "123 Main St Updated",
                City = "Anytown",
                State = "CA",
                PostalCode = "12345",
                Country = "US"
            }
        };

        // Act
        var result = await _service.UpdateAccountHolderAsync(accountHolderId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(accountHolderId.ToString());
        result.FirstName.Should().Be("Jonathan");
        result.LastName.Should().Be("Smith-Updated");
        result.EmailAddress.Should().Be("jonathan.smith@example.com");
        result.HomePhone.Should().Be("555-123-9999");

        // Verify it was updated in database
        var updatedAccountHolder = await _context.AccountHolders.FindAsync(accountHolderId);
        updatedAccountHolder.Should().NotBeNull();
        updatedAccountHolder!.FirstName.Should().Be("Jonathan");
        updatedAccountHolder.LastName.Should().Be("Smith-Updated");
    }

    [Fact]
    public async Task UpdateAccountHolderAsync_Should_ReturnNull_WhenAccountHolderDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.Parse("99999999-9999-9999-9999-999999999999");
        var updateDto = new UpdateAccountHolderDto
        {
            FirstName = "NonExistent",
            LastName = "User"
        };

        // Act
        var result = await _service.UpdateAccountHolderAsync(nonExistentId, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAccountHolderAsync_Should_OnlyUpdateProvidedFields()
    {
        // Arrange
        var accountHolderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var originalAccountHolder = await _context.AccountHolders.FindAsync(accountHolderId);
        var originalEmail = originalAccountHolder!.EmailAddress;
        var originalHomePhone = originalAccountHolder.HomePhone;

        var updateDto = new UpdateAccountHolderDto
        {
            FirstName = "UpdatedFirstName",
            // LastName, EmailAddress, HomePhone not provided - should remain unchanged
            MobilePhone = "555-NEW-MOBILE"
        };

        // Act
        var result = await _service.UpdateAccountHolderAsync(accountHolderId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("UpdatedFirstName");
        result.LastName.Should().Be("Smith"); // Should remain unchanged
        result.EmailAddress.Should().Be(originalEmail); // Should remain unchanged
        result.HomePhone.Should().Be(originalHomePhone); // Should remain unchanged
        result.MobilePhone.Should().Be("555-NEW-MOBILE");
    }

    [Fact]
    public async Task CreateAccountHolderAsync_Should_SetCreatedAtAndLastEdit()
    {
        // Arrange
        var beforeCreate = DateTime.UtcNow;
        var createDto = new CreateAccountHolderDto
        {
            FirstName = "Test",
            LastName = "User",
            EmailAddress = "test.user@example.com"
        };

        // Act
        var result = await _service.CreateAccountHolderAsync(createDto);
        var afterCreate = DateTime.UtcNow;

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamps in database
        var savedAccountHolder = await _context.AccountHolders.FindAsync(Guid.Parse(result.Id));
        savedAccountHolder.Should().NotBeNull();
        savedAccountHolder!.CreatedAt.Should().BeOnOrAfter(beforeCreate);
        savedAccountHolder.CreatedAt.Should().BeOnOrBefore(afterCreate);
        savedAccountHolder.LastEdit.Should().BeOnOrAfter(beforeCreate);
        savedAccountHolder.LastEdit.Should().BeOnOrBefore(afterCreate);
    }

    [Fact]
    public async Task UpdateAccountHolderAsync_Should_UpdateLastEditTimestamp()
    {
        // Arrange
        var accountHolderId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var originalAccountHolder = await _context.AccountHolders.FindAsync(accountHolderId);
        var originalLastEdit = originalAccountHolder!.LastEdit;
        
        // Wait a bit to ensure timestamp difference
        await Task.Delay(10);
        
        var updateDto = new UpdateAccountHolderDto
        {
            FirstName = "Updated Name"
        };

        // Act
        var result = await _service.UpdateAccountHolderAsync(accountHolderId, updateDto);

        // Assert
        result.Should().NotBeNull();
        
        // Verify timestamp was updated
        var updatedAccountHolder = await _context.AccountHolders.FindAsync(accountHolderId);
        updatedAccountHolder.Should().NotBeNull();
        updatedAccountHolder!.LastEdit.Should().BeAfter(originalLastEdit);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
