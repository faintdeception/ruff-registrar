using FluentAssertions;
using StudentRegistrar.Models;
using System.Text.Json;
using Xunit;

namespace StudentRegistrar.Models.Tests;

public class CourseTests
{
    [Fact]
    public void Course_Should_HaveDefaultValues()
    {
        // Act
        var course = new Course();

        // Assert
        course.Id.Should().NotBeEmpty();
        course.SemesterId.Should().BeEmpty();
        course.Name.Should().BeEmpty();
        course.Code.Should().BeNull();
        course.Description.Should().BeNull();
        course.Room.Should().BeNull();
        course.MaxCapacity.Should().Be(0);
        course.Fee.Should().Be(0);
        course.PeriodCode.Should().BeNull();
        course.StartTime.Should().BeNull();
        course.EndTime.Should().BeNull();
        course.CourseConfigJson.Should().Be("{}");
        course.AgeGroup.Should().BeEmpty();
        course.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        course.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        course.CourseInstructors.Should().NotBeNull().And.BeEmpty();
        course.Enrollments.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void CurrentEnrollment_Should_CountOnlyEnrolledStudents()
    {
        // Arrange
        var course = new Course();
        var enrollments = new List<Enrollment>
        {
            new() { EnrollmentType = EnrollmentType.Enrolled },
            new() { EnrollmentType = EnrollmentType.Enrolled },
            new() { EnrollmentType = EnrollmentType.Waitlisted },
            new() { EnrollmentType = EnrollmentType.Withdrawn }
        };
        
        // Mock the enrollments collection behavior
        course.GetType().GetProperty(nameof(course.Enrollments))!
            .SetValue(course, enrollments);

        // Act & Assert
        course.CurrentEnrollment.Should().Be(2);
    }

    [Fact]
    public void AvailableSpots_Should_CalculateCorrectly()
    {
        // Arrange
        var course = new Course { MaxCapacity = 20 };
        var enrollments = new List<Enrollment>
        {
            new() { EnrollmentType = EnrollmentType.Enrolled },
            new() { EnrollmentType = EnrollmentType.Enrolled },
            new() { EnrollmentType = EnrollmentType.Enrolled }
        };
        
        course.GetType().GetProperty(nameof(course.Enrollments))!
            .SetValue(course, enrollments);

        // Act & Assert
        course.AvailableSpots.Should().Be(17);
    }

    [Fact]
    public void IsFull_Should_ReturnTrue_WhenAtCapacity()
    {
        // Arrange
        var course = new Course { MaxCapacity = 2 };
        var enrollments = new List<Enrollment>
        {
            new() { EnrollmentType = EnrollmentType.Enrolled },
            new() { EnrollmentType = EnrollmentType.Enrolled }
        };
        
        course.GetType().GetProperty(nameof(course.Enrollments))!
            .SetValue(course, enrollments);

        // Act & Assert
        course.IsFull.Should().BeTrue();
    }

    [Fact]
    public void IsFull_Should_ReturnFalse_WhenBelowCapacity()
    {
        // Arrange
        var course = new Course { MaxCapacity = 3 };
        var enrollments = new List<Enrollment>
        {
            new() { EnrollmentType = EnrollmentType.Enrolled }
        };
        
        course.GetType().GetProperty(nameof(course.Enrollments))!
            .SetValue(course, enrollments);

        // Act & Assert
        course.IsFull.Should().BeFalse();
    }

    [Fact]
    public void TimeSlot_Should_FormatCorrectly_WhenTimesAreSet()
    {
        // Arrange
        var course = new Course
        {
            StartTime = new TimeSpan(9, 0, 0),  // 9:00 AM
            EndTime = new TimeSpan(10, 30, 0)   // 10:30 AM
        };

        // Act & Assert
        course.TimeSlot.Should().Be("09:00 - 10:30");
    }

    [Fact]
    public void TimeSlot_Should_ShowTBD_WhenTimesAreNotSet()
    {
        // Arrange
        var course = new Course();

        // Act & Assert
        course.TimeSlot.Should().Be("Time TBD");
    }

    [Fact]
    public void TimeSlot_Should_ShowTBD_WhenOnlyOneTimeIsSet()
    {
        // Arrange
        var course = new Course
        {
            StartTime = new TimeSpan(9, 0, 0)
            // EndTime is null
        };

        // Act & Assert
        course.TimeSlot.Should().Be("Time TBD");
    }

    [Fact]
    public void GetCourseConfiguration_Should_ReturnValidConfig_WhenJsonIsValid()
    {
        // Arrange
        var course = new Course();
        var config = new CourseConfiguration
        {
            Prerequisites = new List<string> { "Basic Math", "Reading" },
            Materials = new List<string> { "Textbook", "Calculator" },
            DaysOfWeek = new List<string> { "Monday", "Wednesday", "Friday" },
            GradeRange = "6-8",
            CustomFields = new Dictionary<string, string> { { "SpecialNote", "Outdoor class" } }
        };
        course.SetCourseConfiguration(config);

        // Act
        var retrievedConfig = course.GetCourseConfiguration();

        // Assert
        retrievedConfig.Should().NotBeNull();
        retrievedConfig.Prerequisites.Should().HaveCount(2);
        retrievedConfig.Prerequisites.Should().Contain("Basic Math");
        retrievedConfig.Prerequisites.Should().Contain("Reading");
        retrievedConfig.Materials.Should().Contain("Textbook");
        retrievedConfig.Materials.Should().Contain("Calculator");
        retrievedConfig.DaysOfWeek.Should().HaveCount(3);
        retrievedConfig.DaysOfWeek.Should().Contain("Monday");
        retrievedConfig.GradeRange.Should().Be("6-8");
        retrievedConfig.CustomFields.Should().ContainKey("SpecialNote");
        retrievedConfig.CustomFields["SpecialNote"].Should().Be("Outdoor class");
    }

    [Fact]
    public void GetCourseConfiguration_Should_ReturnEmptyConfig_WhenJsonIsInvalid()
    {
        // Arrange
        var course = new Course
        {
            CourseConfigJson = "invalid json"
        };

        // Act
        var config = course.GetCourseConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.Prerequisites.Should().NotBeNull().And.BeEmpty();
        config.Materials.Should().NotBeNull().And.BeEmpty();
        config.DaysOfWeek.Should().NotBeNull().And.BeEmpty();
        config.GradeRange.Should().BeNull();
        config.CustomFields.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void SetCourseConfiguration_Should_SerializeCorrectly()
    {
        // Arrange
        var course = new Course();
        var config = new CourseConfiguration
        {
            Prerequisites = new List<string> { "Test Prerequisite" },
            GradeRange = "K-2"
        };

        // Act
        course.SetCourseConfiguration(config);

        // Assert
        course.CourseConfigJson.Should().NotBe("{}");
        
        // Verify we can deserialize it back
        var deserializedConfig = JsonSerializer.Deserialize<CourseConfiguration>(course.CourseConfigJson);
        deserializedConfig.Should().NotBeNull();
        deserializedConfig!.Prerequisites.Should().Contain("Test Prerequisite");
        deserializedConfig.GradeRange.Should().Be("K-2");
    }

    [Fact]
    public void CourseConfiguration_Should_InitializeEmptyCollections()
    {
        // Act
        var config = new CourseConfiguration();

        // Assert
        config.Prerequisites.Should().NotBeNull().And.BeEmpty();
        config.Materials.Should().NotBeNull().And.BeEmpty();
        config.DaysOfWeek.Should().NotBeNull().And.BeEmpty();
        config.CustomFields.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Course_Should_SupportDecimalFee()
    {
        // Arrange
        var course = new Course();
        var fee = 125.50m;

        // Act
        course.Fee = fee;

        // Assert
        course.Fee.Should().Be(fee);
    }

    [Fact]
    public void Course_Should_RequireSemesterId()
    {
        // Arrange
        var course = new Course();
        var semesterId = Guid.NewGuid();

        // Act
        course.SemesterId = semesterId;

        // Assert
        course.SemesterId.Should().Be(semesterId);
        course.SemesterId.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("Elementary")]
    [InlineData("Middle School")]
    [InlineData("High School")]
    [InlineData("Adult")]
    [InlineData("All Ages")]
    public void Course_Should_SupportVariousAgeGroups(string ageGroup)
    {
        // Arrange
        var course = new Course();

        // Act
        course.AgeGroup = ageGroup;

        // Assert
        course.AgeGroup.Should().Be(ageGroup);
    }

    [Fact]
    public void Course_Should_HandleLargeCapacity()
    {
        // Arrange
        var course = new Course();
        var largeCapacity = 1000;

        // Act
        course.MaxCapacity = largeCapacity;

        // Assert
        course.MaxCapacity.Should().Be(largeCapacity);
    }

    [Fact]
    public void Course_Should_HandleZeroCapacity()
    {
        // Arrange
        var course = new Course { MaxCapacity = 0 };

        // Act & Assert
        course.MaxCapacity.Should().Be(0);
        course.AvailableSpots.Should().Be(0);
        course.IsFull.Should().BeTrue(); // 0 enrollments in 0 capacity means "full" (can't enroll more)
    }
}
