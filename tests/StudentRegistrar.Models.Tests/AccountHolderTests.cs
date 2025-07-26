using FluentAssertions;
using StudentRegistrar.Models;
using System.Text.Json;
using Xunit;

namespace StudentRegistrar.Models.Tests;

public class AccountHolderTests
{
    [Fact]
    public void AccountHolder_Should_HaveDefaultValues()
    {
        // Act
        var accountHolder = new AccountHolder();

        // Assert
        accountHolder.Id.Should().NotBeEmpty();
        accountHolder.FirstName.Should().BeEmpty();
        accountHolder.LastName.Should().BeEmpty();
        accountHolder.EmailAddress.Should().BeEmpty();
        accountHolder.KeycloakUserId.Should().BeEmpty();
        accountHolder.MembershipDuesOwed.Should().Be(0);
        accountHolder.MembershipDuesReceived.Should().Be(0);
        accountHolder.AddressJson.Should().Be("{}");
        accountHolder.EmergencyContactJson.Should().Be("{}");
        accountHolder.MemberSince.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        accountHolder.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        accountHolder.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        accountHolder.Students.Should().NotBeNull().And.BeEmpty();
        accountHolder.Payments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void FullName_Should_CombineFirstAndLastName()
    {
        // Arrange
        var accountHolder = new AccountHolder
        {
            FirstName = "John",
            LastName = "Doe"
        };

        // Act & Assert
        accountHolder.FullName.Should().Be("John Doe");
    }

    [Fact]
    public void MembershipDuesBalance_Should_CalculateCorrectly()
    {
        // Arrange
        var accountHolder = new AccountHolder
        {
            MembershipDuesOwed = 100.50m,
            MembershipDuesReceived = 75.25m
        };

        // Act & Assert
        accountHolder.MembershipDuesBalance.Should().Be(25.25m);
    }

    [Fact]
    public void GetAddress_Should_ReturnValidAddress_WhenJsonIsValid()
    {
        // Arrange
        var accountHolder = new AccountHolder();
        var address = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            PostalCode = "62701",
            Country = "US"
        };
        accountHolder.SetAddress(address);

        // Act
        var retrievedAddress = accountHolder.GetAddress();

        // Assert
        retrievedAddress.Should().NotBeNull();
        retrievedAddress.Street.Should().Be("123 Main St");
        retrievedAddress.City.Should().Be("Springfield");
        retrievedAddress.State.Should().Be("IL");
        retrievedAddress.PostalCode.Should().Be("62701");
        retrievedAddress.Country.Should().Be("US");
    }

    [Fact]
    public void GetAddress_Should_ReturnEmptyAddress_WhenJsonIsInvalid()
    {
        // Arrange
        var accountHolder = new AccountHolder
        {
            AddressJson = "invalid json"
        };

        // Act
        var address = accountHolder.GetAddress();

        // Assert
        address.Should().NotBeNull();
        address.Street.Should().BeEmpty();
        address.City.Should().BeEmpty();
        address.State.Should().BeEmpty();
        address.PostalCode.Should().BeEmpty();
        address.Country.Should().Be("US");
    }

    [Fact]
    public void SetAddress_Should_SerializeCorrectly()
    {
        // Arrange
        var accountHolder = new AccountHolder();
        var address = new Address
        {
            Street = "456 Oak Ave",
            City = "Chicago",
            State = "IL",
            PostalCode = "60601"
        };

        // Act
        accountHolder.SetAddress(address);

        // Assert
        accountHolder.AddressJson.Should().NotBe("{}");
        
        // Verify we can deserialize it back
        var deserializedAddress = JsonSerializer.Deserialize<Address>(accountHolder.AddressJson);
        deserializedAddress.Should().NotBeNull();
        deserializedAddress!.Street.Should().Be("456 Oak Ave");
        deserializedAddress.City.Should().Be("Chicago");
    }

    [Fact]
    public void GetEmergencyContact_Should_ReturnValidContact_WhenJsonIsValid()
    {
        // Arrange
        var accountHolder = new AccountHolder();
        var emergencyContact = new EmergencyContact
        {
            FirstName = "Jane",
            LastName = "Smith",
            MobilePhone = "555-0123",
            Email = "jane.smith@example.com",
            Address = new Address { Street = "789 Pine St", City = "Springfield" }
        };
        accountHolder.SetEmergencyContact(emergencyContact);

        // Act
        var retrievedContact = accountHolder.GetEmergencyContact();

        // Assert
        retrievedContact.Should().NotBeNull();
        retrievedContact.FirstName.Should().Be("Jane");
        retrievedContact.LastName.Should().Be("Smith");
        retrievedContact.MobilePhone.Should().Be("555-0123");
        retrievedContact.Email.Should().Be("jane.smith@example.com");
        retrievedContact.FullName.Should().Be("Jane Smith");
        retrievedContact.Address.Should().NotBeNull();
        retrievedContact.Address.Street.Should().Be("789 Pine St");
    }

    [Fact]
    public void GetEmergencyContact_Should_ReturnEmptyContact_WhenJsonIsInvalid()
    {
        // Arrange
        var accountHolder = new AccountHolder
        {
            EmergencyContactJson = "invalid json"
        };

        // Act
        var contact = accountHolder.GetEmergencyContact();

        // Assert
        contact.Should().NotBeNull();
        contact.FirstName.Should().BeEmpty();
        contact.LastName.Should().BeEmpty();
        contact.FullName.Should().Be(" ");
        contact.Address.Should().NotBeNull();
    }

    [Theory]
    [InlineData("", "", " ")]
    [InlineData("John", "", "John ")]
    [InlineData("", "Doe", " Doe")]
    [InlineData("John", "Doe", "John Doe")]
    public void FullName_Should_HandleVariousNameCombinations(string firstName, string lastName, string expected)
    {
        // Arrange
        var accountHolder = new AccountHolder
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act & Assert
        accountHolder.FullName.Should().Be(expected);
    }

    [Fact]
    public void Address_DefaultCountry_Should_BeUS()
    {
        // Arrange & Act
        var address = new Address();

        // Assert
        address.Country.Should().Be("US");
    }

    [Fact]
    public void EmergencyContact_FullName_Should_CombineNames()
    {
        // Arrange
        var contact = new EmergencyContact
        {
            FirstName = "Emergency",
            LastName = "Contact"
        };

        // Act & Assert
        contact.FullName.Should().Be("Emergency Contact");
    }

    [Fact]
    public void EmergencyContact_Should_HaveDefaultAddress()
    {
        // Arrange & Act
        var contact = new EmergencyContact();

        // Assert
        contact.Address.Should().NotBeNull();
        contact.Address.Country.Should().Be("US");
    }
}
