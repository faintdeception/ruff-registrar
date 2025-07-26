using FluentAssertions;
using StudentRegistrar.Models;
using System.Text.Json;
using Xunit;

namespace StudentRegistrar.Models.Tests;

public class StudentTests
{
    [Fact]
    public void Student_Should_HaveDefaultValues()
    {
        // Act
        var student = new Student();

        // Assert
        student.Id.Should().NotBeEmpty();
        student.AccountHolderId.Should().BeEmpty();
        student.FirstName.Should().BeEmpty();
        student.LastName.Should().BeEmpty();
        student.Grade.Should().BeNull();
        student.DateOfBirth.Should().BeNull();
        student.StudentInfoJson.Should().Be("{}");
        student.Notes.Should().BeNull();
        student.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        student.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        student.Enrollments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void FullName_Should_CombineFirstAndLastName()
    {
        // Arrange
        var student = new Student
        {
            FirstName = "Alice",
            LastName = "Johnson"
        };

        // Act & Assert
        student.FullName.Should().Be("Alice Johnson");
    }

    [Fact]
    public void GetStudentInfo_Should_ReturnValidInfo_WhenJsonIsValid()
    {
        // Arrange
        var student = new Student();
        var studentInfo = new StudentInfo
        {
            SpecialConditions = new List<string> { "ADHD", "Needs extra time" },
            LearningDisabilities = new List<string> { "Dyslexia" },
            Allergies = new List<string> { "Peanuts", "Shellfish" },
            Medications = new List<string> { "Inhaler" },
            PreferredName = "Al",
            ParentNotes = "Please call if issues arise",
            TeacherNotes = "Very bright student"
        };
        student.SetStudentInfo(studentInfo);

        // Act
        var retrievedInfo = student.GetStudentInfo();

        // Assert
        retrievedInfo.Should().NotBeNull();
        retrievedInfo.SpecialConditions.Should().HaveCount(2);
        retrievedInfo.SpecialConditions.Should().Contain("ADHD");
        retrievedInfo.SpecialConditions.Should().Contain("Needs extra time");
        retrievedInfo.LearningDisabilities.Should().Contain("Dyslexia");
        retrievedInfo.Allergies.Should().HaveCount(2);
        retrievedInfo.Allergies.Should().Contain("Peanuts");
        retrievedInfo.Allergies.Should().Contain("Shellfish");
        retrievedInfo.Medications.Should().Contain("Inhaler");
        retrievedInfo.PreferredName.Should().Be("Al");
        retrievedInfo.ParentNotes.Should().Be("Please call if issues arise");
        retrievedInfo.TeacherNotes.Should().Be("Very bright student");
    }

    [Fact]
    public void GetStudentInfo_Should_ReturnEmptyInfo_WhenJsonIsInvalid()
    {
        // Arrange
        var student = new Student
        {
            StudentInfoJson = "invalid json"
        };

        // Act
        var info = student.GetStudentInfo();

        // Assert
        info.Should().NotBeNull();
        info.SpecialConditions.Should().NotBeNull().And.BeEmpty();
        info.LearningDisabilities.Should().NotBeNull().And.BeEmpty();
        info.Allergies.Should().NotBeNull().And.BeEmpty();
        info.Medications.Should().NotBeNull().And.BeEmpty();
        info.PreferredName.Should().BeNull();
        info.ParentNotes.Should().BeNull();
        info.TeacherNotes.Should().BeNull();
    }

    [Fact]
    public void SetStudentInfo_Should_SerializeCorrectly()
    {
        // Arrange
        var student = new Student();
        var studentInfo = new StudentInfo
        {
            SpecialConditions = new List<string> { "Test condition" },
            PreferredName = "TestName"
        };

        // Act
        student.SetStudentInfo(studentInfo);

        // Assert
        student.StudentInfoJson.Should().NotBe("{}");
        
        // Verify we can deserialize it back
        var deserializedInfo = JsonSerializer.Deserialize<StudentInfo>(student.StudentInfoJson);
        deserializedInfo.Should().NotBeNull();
        deserializedInfo!.SpecialConditions.Should().Contain("Test condition");
        deserializedInfo.PreferredName.Should().Be("TestName");
    }

    [Theory]
    [InlineData("", "", " ")]
    [InlineData("Alice", "", "Alice ")]
    [InlineData("", "Johnson", " Johnson")]
    [InlineData("Alice", "Johnson", "Alice Johnson")]
    public void FullName_Should_HandleVariousNameCombinations(string firstName, string lastName, string expected)
    {
        // Arrange
        var student = new Student
        {
            FirstName = firstName,
            LastName = lastName
        };

        // Act & Assert
        student.FullName.Should().Be(expected);
    }

    [Fact]
    public void Student_Should_AllowValidGrades()
    {
        // Arrange
        var student = new Student();
        var validGrades = new[] { "K", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };

        // Act & Assert
        foreach (var grade in validGrades)
        {
            student.Grade = grade;
            student.Grade.Should().Be(grade);
        }
    }

    [Fact]
    public void StudentInfo_Should_InitializeEmptyCollections()
    {
        // Act
        var studentInfo = new StudentInfo();

        // Assert
        studentInfo.SpecialConditions.Should().NotBeNull().And.BeEmpty();
        studentInfo.LearningDisabilities.Should().NotBeNull().And.BeEmpty();
        studentInfo.Allergies.Should().NotBeNull().And.BeEmpty();
        studentInfo.Medications.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Student_Should_SupportDateOfBirth()
    {
        // Arrange
        var student = new Student();
        var birthDate = new DateTime(2010, 5, 15);

        // Act
        student.DateOfBirth = birthDate;

        // Assert
        student.DateOfBirth.Should().Be(birthDate);
    }

    [Fact]
    public void Student_Should_SupportNotes()
    {
        // Arrange
        var student = new Student();
        var notes = "This student excels in mathematics and enjoys reading.";

        // Act
        student.Notes = notes;

        // Assert
        student.Notes.Should().Be(notes);
    }

    [Fact]
    public void Student_Should_RequireAccountHolderId()
    {
        // Arrange
        var student = new Student();
        var accountHolderId = Guid.NewGuid();

        // Act
        student.AccountHolderId = accountHolderId;

        // Assert
        student.AccountHolderId.Should().Be(accountHolderId);
        student.AccountHolderId.Should().NotBeEmpty();
    }
}
