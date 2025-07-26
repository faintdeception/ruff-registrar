using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using StudentRegistrar.Api.DTOs;
using StudentRegistrar.Api.Services;
using StudentRegistrar.Data;
using StudentRegistrar.Models;
using Xunit;

namespace StudentRegistrar.Api.Tests.Services;

public class CourseServiceTests : IDisposable
{
    private readonly StudentRegistrarDbContext _context;
    private readonly IMapper _mapper;
    private readonly CourseService _service;

    public CourseServiceTests()
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

        _service = new CourseService(_context, _mapper);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var courses = new List<LegacyCourse>
        {
            new LegacyCourse
            {
                Id = 1,
                Name = "Math 101",
                Code = "MATH101",
                Description = "Basic Mathematics",
                CreditHours = 3,
                Instructor = "Prof. Johnson",
                AcademicYear = "2024-25",
                Semester = "Fall"
            },
            new LegacyCourse
            {
                Id = 2,
                Name = "English 101",
                Code = "ENG101",
                Description = "Basic English",
                CreditHours = 3,
                Instructor = "Prof. Williams",
                AcademicYear = "2024-25",
                Semester = "Fall"
            },
            new LegacyCourse
            {
                Id = 3,
                Name = "Physics 101",
                Code = "PHYS101",
                Description = "Basic Physics",
                CreditHours = 4,
                Instructor = "Prof. Brown",
                AcademicYear = "2025-26",
                Semester = "Spring"
            }
        };

        var students = new List<LegacyStudent>
        {
            new LegacyStudent
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "123-456-7890",
                DateOfBirth = new DateOnly(2000, 1, 15),
                Address = "123 Main St",
                City = "Anytown",
                State = "CA",
                ZipCode = "12345",
                EmergencyContactName = "Bob Doe",
                EmergencyContactPhone = "123-456-7892",
                CreatedAt = DateTime.Now.AddMonths(-6),
                UpdatedAt = DateTime.Now.AddMonths(-6)
            },
            new LegacyStudent
            {
                Id = 2,
                FirstName = "Alice",
                LastName = "Smith",
                Email = "alice.smith@example.com",
                PhoneNumber = "987-654-3210",
                DateOfBirth = new DateOnly(1999, 5, 20),
                Address = "456 Oak Ave",
                City = "Somewhere",
                State = "NY",
                ZipCode = "54321",
                EmergencyContactName = "Mary Smith",
                EmergencyContactPhone = "987-654-3212",
                CreatedAt = DateTime.Now.AddMonths(-3),
                UpdatedAt = DateTime.Now.AddMonths(-3)
            }
        };

        var enrollments = new List<LegacyEnrollment>
        {
            new LegacyEnrollment
            {
                Id = 1,
                StudentId = 1,
                CourseId = 1,
                EnrollmentDate = DateTime.Now.AddMonths(-2),
                Status = "Active"
            },
            new LegacyEnrollment
            {
                Id = 2,
                StudentId = 2,
                CourseId = 1,
                EnrollmentDate = DateTime.Now.AddMonths(-1),
                Status = "Active"
            },
            new LegacyEnrollment
            {
                Id = 3,
                StudentId = 1,
                CourseId = 2,
                EnrollmentDate = DateTime.Now.AddMonths(-1),
                Status = "Completed"
            }
        };

        var grades = new List<GradeRecord>
        {
            new GradeRecord
            {
                Id = 1,
                StudentId = 1,
                CourseId = 1,
                LetterGrade = "A",
                GradeDate = DateTime.Now.AddDays(-14),
                Comments = "Excellent work"
            },
            new GradeRecord
            {
                Id = 2,
                StudentId = 2,
                CourseId = 1,
                LetterGrade = "B+",
                GradeDate = DateTime.Now.AddDays(-7),
                Comments = "Good progress"
            }
        };

        _context.Courses.AddRange(courses);
        _context.Students.AddRange(students);
        _context.Enrollments.AddRange(enrollments);
        _context.GradeRecords.AddRange(grades);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCoursesAsync_Should_ReturnAllCourses_OrderedByAcademicYearThenSemesterThenCode()
    {
        // Act
        var result = await _service.GetAllCoursesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
        
        var courseList = result.ToList();
        courseList[0].AcademicYear.Should().Be("2024-25");
        courseList[1].AcademicYear.Should().Be("2024-25");
        courseList[2].AcademicYear.Should().Be("2025-26");
        
        // Within same academic year, should be ordered by code
        courseList[0].Code.Should().Be("ENG101");
        courseList[1].Code.Should().Be("MATH101");
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_ReturnCourse_WhenCourseExists()
    {
        // Act
        var result = await _service.GetCourseByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Math 101");
        result.Code.Should().Be("MATH101");
        result.CreditHours.Should().Be(3);
        result.Instructor.Should().Be("Prof. Johnson");
    }

    [Fact]
    public async Task GetCourseByIdAsync_Should_ReturnNull_WhenCourseDoesNotExist()
    {
        // Act
        var result = await _service.GetCourseByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateCourseAsync_Should_CreateAndReturnCourse()
    {
        // Arrange
        var createDto = new CreateCourseDto
        {
            Name = "Chemistry 101",
            Code = "CHEM101",
            Description = "Basic Chemistry",
            CreditHours = 4,
            Instructor = "Dr. Wilson",
            AcademicYear = "2024-25",
            Semester = "Spring"
        };

        // Act
        var result = await _service.CreateCourseAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Chemistry 101");
        result.Code.Should().Be("CHEM101");
        result.CreditHours.Should().Be(4);
        result.Instructor.Should().Be("Dr. Wilson");

        // Verify it was saved to database
        var savedCourse = await _context.Courses.FindAsync(result.Id);
        savedCourse.Should().NotBeNull();
        savedCourse!.Name.Should().Be("Chemistry 101");
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_UpdateAndReturnCourse_WhenCourseExists()
    {
        // Arrange
        var updateDto = new UpdateCourseDto
        {
            Name = "Advanced Mathematics",
            Code = "MATH101",
            Description = "Advanced Mathematical Concepts",
            CreditHours = 4,
            Instructor = "Prof. Johnson",
            AcademicYear = "2024-25",
            Semester = "Fall"
        };

        // Act
        var result = await _service.UpdateCourseAsync(1, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Advanced Mathematics");
        result.Description.Should().Be("Advanced Mathematical Concepts");
        result.CreditHours.Should().Be(4);

        // Verify it was updated in database
        var updatedCourse = await _context.Courses.FindAsync(1);
        updatedCourse.Should().NotBeNull();
        updatedCourse!.Name.Should().Be("Advanced Mathematics");
    }

    [Fact]
    public async Task UpdateCourseAsync_Should_ReturnNull_WhenCourseDoesNotExist()
    {
        // Arrange
        var updateDto = new UpdateCourseDto
        {
            Name = "NonExistent Course",
            Code = "NONE999",
            Description = "This course doesn't exist"
        };

        // Act
        var result = await _service.UpdateCourseAsync(999, updateDto);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_ReturnTrue_WhenCourseExists()
    {
        // Act
        var result = await _service.DeleteCourseAsync(3);

        // Assert
        result.Should().BeTrue();

        // Verify course was deleted
        var deletedCourse = await _context.Courses.FindAsync(3);
        deletedCourse.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCourseAsync_Should_ReturnFalse_WhenCourseDoesNotExist()
    {
        // Act
        var result = await _service.DeleteCourseAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_Should_ReturnCourseEnrollments()
    {
        // Act
        var result = await _service.GetCourseEnrollmentsAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var enrollmentList = result.ToList();
        enrollmentList.Should().OnlyContain(e => e.CourseId == 1);
        enrollmentList.Should().Contain(e => e.StudentId == 1);
        enrollmentList.Should().Contain(e => e.StudentId == 2);
    }

    [Fact]
    public async Task GetCourseEnrollmentsAsync_Should_ReturnEmptyList_WhenCourseHasNoEnrollments()
    {
        // Act
        var result = await _service.GetCourseEnrollmentsAsync(3);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCourseGradesAsync_Should_ReturnCourseGrades_OrderedByStudentName()
    {
        // Act
        var result = await _service.GetCourseGradesAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        
        var gradesList = result.ToList();
        gradesList.Should().OnlyContain(g => g.CourseId == 1);
        
        // Should be ordered by student last name, then first name
        gradesList[0].StudentId.Should().Be(1); // John Doe
        gradesList[1].StudentId.Should().Be(2); // Alice Smith
    }

    [Fact]
    public async Task GetCourseGradesAsync_Should_ReturnEmptyList_WhenCourseHasNoGrades()
    {
        // Act
        var result = await _service.GetCourseGradesAsync(2);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
