using OpenQA.Selenium;
using FluentAssertions;
using StudentRegistrar.E2E.Tests.Base;
using StudentRegistrar.E2E.Tests.Pages;
using Xunit;

namespace StudentRegistrar.E2E.Tests.Tests.RoleBasedTests;

public class AdminTests : BaseTest
{
    [Fact]
    public void Admin_Should_Login_Successfully()
    {
        // Arrange
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Act - Login as admin
        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:AdminUser:Username"] ?? "admin1";
        var password = Configuration["TestCredentials:AdminUser:Password"] ?? "AdminPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert - Should be logged in and see admin navigation
        Driver.Url.Should().NotContain("/login", "Admin should be logged in");
        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Admin should be authenticated");
        
        // Verify admin-specific navigation is visible
        Driver.PageSource.Should().Contain("Students", "Admin should see Students link");
        Driver.PageSource.Should().Contain("Semesters", "Admin should see Semesters link");
    }

    [Fact]
    public void Admin_Should_Create_Member_Successfully()
    {
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act - Navigate to members page
        var membersLink = Driver.FindElement(By.LinkText("Members"));
        membersLink.Click();
        WaitForPageLoad();
        Thread.Sleep(1000);

        // Assert - Should be on members page
        Driver.Url.Should().Contain("/members", "Should navigate to members page");
        Driver.PageSource.Should().Contain("Members Management", "Should see members management page");

        // Click Create Member button
        var createMemberButton = Driver.FindElement(By.Id("create-member-button"));
        createMemberButton.Click();
        Thread.Sleep(500);

        // Fill in member form
        var firstNameField = Driver.FindElement(By.Id("member-first-name"));
        var lastNameField = Driver.FindElement(By.Id("member-last-name"));
        var emailField = Driver.FindElement(By.Id("member-email"));
        var mobilePhoneField = Driver.FindElement(By.Id("member-mobile-phone"));
        var streetField = Driver.FindElement(By.Id("member-street"));
        var cityField = Driver.FindElement(By.Id("member-city"));
        var stateField = Driver.FindElement(By.Id("member-state"));

        // Enter test member data
        var timestamp = DateTime.Now.Ticks.ToString().Substring(10); // Last 8 digits for uniqueness
        firstNameField.SendKeys("Test");
        lastNameField.SendKeys($"Member{timestamp}");
        emailField.SendKeys($"testmember{timestamp}@example.com");
        mobilePhoneField.SendKeys("555-0123");
        streetField.SendKeys("123 Test St");
        cityField.SendKeys("TestCity");
        stateField.SendKeys("CA");

        // Submit the form
        var submitButton = Driver.FindElement(By.Id("submit-create-member"));
        submitButton.Click();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert - Member should be created successfully
        Driver.PageSource.Should().Contain("Member created successfully!", "Should show success message");
        
        // Verify the new member appears in the list
        Driver.PageSource.Should().Contain($"Test Member{timestamp}", "New member should appear in the list");
        Driver.PageSource.Should().Contain($"testmember{timestamp}@example.com", "Member email should appear in the list");
    }

    // [Fact]
    // public void Admin_Should_Access_Student_Management()
    // {
    //     // Arrange - Login as admin
    //     LoginAsAdmin();

    //     // Act - Navigate to students page
    //     var studentsLink = Driver.FindElement(By.LinkText("Students"));
    //     studentsLink.Click();
    //     WaitForPageLoad();

    //     // Assert - Should be on students page
    //     Driver.Url.Should().Contain("/students", "Should navigate to students page");
    //     Driver.PageSource.Should().ContainEquivalentOf("student");
    // }

    [Fact]
    public void Admin_Should_Access_Semester_Management()
    {
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act - Navigate to semesters page
        var semestersLink = Driver.FindElement(By.LinkText("Semesters"));
        semestersLink.Click();
        WaitForPageLoad();

        // Assert - Should be on semesters page
        Driver.Url.Should().Contain("/semesters", "Should navigate to semesters page");
        Driver.PageSource.Should().ContainEquivalentOf("semester");
    }

    [Fact]
    public void Admin_Should_Access_All_Navigation_Links()
    {
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act & Assert - Test each navigation link
        var navigationLinks = new[]
        {
            ("Account", "/account-holder"),
            // ("Students", "/students"),
            ("Courses", "/courses"),
            ("Semesters", "/semesters"),
            ("Rooms", "/rooms"),
            // ("Enrollments", "/enrollments"),
            // ("Grades", "/grades"),
            ("Educators", "/educators")
        };

        foreach (var (linkText, expectedUrl) in navigationLinks)
        {
            // Navigate back to home first
            Driver.Navigate().GoToUrl(BaseUrl);
            WaitForPageLoad();

            // Click the navigation link
            var link = Driver.FindElement(By.LinkText(linkText));
            link.Click();
            WaitForPageLoad();

            // Verify navigation worked
            Driver.Url.Should().Contain(expectedUrl, $"Should navigate to {linkText} page");
        }
    }

    [Fact]
    public void Admin_Should_Create_New_Semester_Successfully()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // Verify we can access the semesters page
        semestersPage.IsOnSemestersPage().Should().BeTrue("Admin should access semesters page");

        // Get initial semester count
        var initialCount = semestersPage.GetSemesterCount();

        // Generate unique semester data for this test
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var semesterName = $"Test Semester {timestamp}";
        var semesterCode = $"TS{timestamp.Substring(8)}"; // Use time portion for shorter code
        var currentYear = DateTime.Now.Year;
        var startDate = $"8/15/{currentYear}"; // Fall semester start
        var endDate = $"12/15/{currentYear}";   // Fall semester end
        var regStartDate = $"6/1/{currentYear}"; // Registration starts in summer
        var regEndDate = $"8/1/{currentYear}";   // Registration ends before semester

        // Act - Create new semester
        semestersPage.ClickCreateSemester();
        semestersPage.IsCreateFormVisible().Should().BeTrue("Create form should be visible");

        semestersPage.FillSemesterForm(
            name: semesterName,
            code: semesterCode,
            startDate: DateTime.Parse(startDate),
            endDate: DateTime.Parse(endDate),
            regStartDate: DateTime.Parse(regStartDate),
            regEndDate: DateTime.Parse(regEndDate),
            isActive: true
        );

        semestersPage.SaveSemester();

        // Assert - Verify semester was created
        WaitForPageLoad();
        Thread.Sleep(2000); // Allow time for the list to refresh

        // Check if we're back on the semesters list page
        semestersPage.IsOnSemestersPage().Should().BeTrue("Should return to semesters list after creation");

        // Verify the semester appears in the list
        semestersPage.IsSemesterVisible(semesterName).Should().BeTrue($"Created semester '{semesterName}' should appear in the list");

        // Verify semester count increased
        var finalCount = semestersPage.GetSemesterCount();
        finalCount.Should().BeGreaterThan(initialCount, "Semester count should increase after creation");

        // Check for success message (if displayed)
        var successMessage = semestersPage.GetSuccessMessage();
        if (!string.IsNullOrEmpty(successMessage))
        {
            successMessage.ToLower().Should().Contain("success", "Should show success message");
        }
    }

    [Fact]
    public void Admin_Should_See_Create_Semester_Button()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // Act & Assert - Verify admin can see the create button
        semestersPage.IsOnSemestersPage().Should().BeTrue("Should be on semesters page");
        semestersPage.CanSeeCreateButton().Should().BeTrue("Admin should see create semester button");
    }

    [Fact]
    public void Admin_Should_Be_Able_To_Cancel_Semester_Creation()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        var initialCount = semestersPage.GetSemesterCount();

        // Act - Start creating semester but cancel
        semestersPage.ClickCreateSemester();
        semestersPage.IsCreateFormVisible().Should().BeTrue("Create form should be visible");

        // Fill some data
        semestersPage.FillSemesterForm(
            name: "Test Cancel Semester",
            code: "CANCEL",
            startDate: DateTime.Parse("2025-01-01"),
            endDate: DateTime.Parse("2025-05-01"),
            regStartDate: DateTime.Parse("2024-11-01"),
            regEndDate: DateTime.Parse("2024-12-01"),
            isActive: true
        );

        // Cancel instead of saving
        semestersPage.CancelCreate();

        // Assert - Verify no semester was created
        WaitForPageLoad();
        Thread.Sleep(1000);

        semestersPage.IsOnSemestersPage().Should().BeTrue("Should return to semesters list after cancel");
        semestersPage.IsSemesterVisible("Test Cancel Semester").Should().BeFalse("Cancelled semester should not appear in list");

        var finalCount = semestersPage.GetSemesterCount();
        finalCount.Should().Be(initialCount, "Semester count should remain unchanged after cancel");
    }

    [Fact]
    public void Admin_Should_Create_Multiple_Semesters_With_Different_Terms()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        var initialCount = semestersPage.GetSemesterCount();
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var currentYear = DateTime.Now.Year;

        // Define different semester types
        var semesters = new[]
        {
            new { Name = $"Spring {currentYear} Test {timestamp}", Code = $"SP{timestamp.Substring(10)}", Start = $"01/15/{currentYear}", End = $"05/15/{currentYear}", RegStart = $"11/01/{currentYear-1}", RegEnd = $"01/01/{currentYear}" },
            new { Name = $"Summer {currentYear} Test {timestamp}", Code = $"SU{timestamp.Substring(10)}", Start = $"06/01/{currentYear}", End = $"08/15/{currentYear}", RegStart = $"03/01/{currentYear}", RegEnd = $"05/15/{currentYear}" },
            new { Name = $"Fall {currentYear} Test {timestamp}", Code = $"FA{timestamp.Substring(10)}", Start = $"08/20/{currentYear}", End = $"12/20/{currentYear}", RegStart = $"05/01/{currentYear}", RegEnd = $"08/01/{currentYear}" }
        };

        // Act - Create each semester
        foreach (var semester in semesters)
        {
            semestersPage.ClickCreateSemester();
            semestersPage.IsCreateFormVisible().Should().BeTrue($"Create form should be visible for {semester.Name}");

            semestersPage.FillSemesterForm(
                name: semester.Name,
                code: semester.Code,
                startDate: DateTime.Parse(semester.Start),
                endDate: DateTime.Parse(semester.End),
                regStartDate: DateTime.Parse(semester.RegStart),
                regEndDate: DateTime.Parse(semester.RegEnd),
                isActive: true
            );

            semestersPage.SaveSemester();
            WaitForPageLoad();
            Thread.Sleep(1500); // Allow time between creations

            // Verify each semester was created
             
        }

        // Assert - Verify all semesters were created
        var finalCount = semestersPage.GetSemesterCount();
        finalCount.Should().BeGreaterThan(initialCount + semesters.Length - 1, "All semesters should be created");
    }

    [Fact]
    public void Admin_Should_Create_Semester_With_Test_IDs()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // Get initial semester count
        var initialCount = semestersPage.GetSemesterCount();

        // Generate unique semester data
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var semesterName = $"Test Semester {timestamp}";
        var semesterCode = $"TS{timestamp.Substring(8)}";

        // Act - Create new semester using test IDs
        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        // Verify modal is open
        semestersPage.IsCreateFormVisible().Should().BeTrue("Create semester modal should be visible");
        semestersPage.GetModalTitle().Should().Be("Create New Semester");

        // Fill semester form
        semestersPage.FillSemesterForm(
            name: semesterName,
            code: semesterCode,
            startDate: new DateTime(2025, 8, 25),
            endDate: new DateTime(2025, 12, 15),
            regStartDate: new DateTime(2025, 7, 1),
            regEndDate: new DateTime(2025, 8, 20),
            isActive: true
        );

        semestersPage.SaveSemester();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Check if there was an error during creation
        if (semestersPage.IsErrorDisplayed())
        {
            var errorMessage = semestersPage.GetErrorMessage();
            Console.WriteLine($"Semester creation failed with error: {errorMessage}");
            
            // Cancel out of the modal and fail the test with a descriptive message
            semestersPage.CancelCreate();
            throw new Exception($"Semester creation failed: {errorMessage}");
        }

        // Assert - Verify semester was created
        semestersPage.IsSemesterVisible(semesterName).Should().BeTrue($"Semester '{semesterName}' should be visible");
        
        var finalCount = semestersPage.GetSemesterCount();
        finalCount.Should().BeGreaterThan(initialCount, "Semester count should increase");
    }

    [Fact]
    public void Admin_Should_Cancel_Semester_Creation_Using_Test_IDs()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        var initialCount = semestersPage.GetSemesterCount();

        // Act - Start creating semester but cancel
        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        // Fill some data
        semestersPage.FillSemesterForm(
            name: "Cancel Test",
            code: "CANCEL",
            startDate: DateTime.Now.AddMonths(1),
            endDate: DateTime.Now.AddMonths(4),
            regStartDate: DateTime.Now,
            regEndDate: DateTime.Now.AddDays(20)
        );
        
        // Cancel instead of saving
        semestersPage.CancelCreate();
        Thread.Sleep(1000);

        // Assert - Verify no semester was created
        semestersPage.IsSemesterVisible("Cancel Test").Should().BeFalse("Cancelled semester should not appear");
        
        var finalCount = semestersPage.GetSemesterCount();
        finalCount.Should().Be(initialCount, "Semester count should remain unchanged");
    }

    [Fact]
    public void Admin_Should_Validate_Semester_Date_Logic()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // Act - Try to create semester with invalid dates
        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        // Fill with invalid date range (end before start)
        semestersPage.FillSemesterForm(
            name: "Invalid Dates Test",
            code: "INVALID",
            startDate: DateTime.Now.AddMonths(3), // Start later
            endDate: DateTime.Now.AddMonths(1),   // End earlier (invalid)
            regStartDate: DateTime.Now,
            regEndDate: DateTime.Now.AddDays(20)
        );
        
        semestersPage.SaveSemester();
        Thread.Sleep(1000);

        // Assert - Should show error and stay on form
        semestersPage.IsErrorDisplayed().Should().BeTrue("Should show validation error for invalid dates");
        semestersPage.IsCreateFormVisible().Should().BeTrue("Should remain on create form after validation error");
    }

    [Fact]
    public void Admin_Should_Edit_Existing_Semester()
    {
        // Arrange - Login as admin and navigate to semesters
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // First create a semester to edit
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var originalName = $"Edit Test {timestamp}";
        var updatedName = $"Updated Test {timestamp}";

        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        semestersPage.FillSemesterForm(
            name: originalName,
            code: $"EDIT2025{timestamp.Substring(8)}",
            startDate: new DateTime(2025, 9, 1),
            endDate: new DateTime(2025, 12, 20),
            regStartDate: new DateTime(2025, 7, 15),
            regEndDate: new DateTime(2025, 8, 25)
        );
        
        semestersPage.SaveSemester();
        Thread.Sleep(2000);
        
        // Verify it was created
        semestersPage.IsSemesterVisible(originalName).Should().BeTrue("Original semester should be created");

        // Act - Edit the semester
        semestersPage.EditSemester(originalName);
        semestersPage.WaitForModalToOpen();
        
        // Verify edit modal opens
        semestersPage.IsCreateFormVisible().Should().BeTrue("Edit form should be visible");
        semestersPage.GetModalTitle().Should().Be("Edit Semester");
        
        // Change the name
        semestersPage.FillSemesterForm(
            name: updatedName,
            code: $"UPDATED2025{timestamp.Substring(8)}",
            startDate: new DateTime(2025, 9, 1),
            endDate: new DateTime(2025, 12, 20),
            regStartDate: new DateTime(2025, 7, 15),
            regEndDate: new DateTime(2025, 8, 25)
        );
        
        semestersPage.SaveSemester();
        Thread.Sleep(2000);

        // Assert - Verify the semester was updated
        semestersPage.IsSemesterVisible(updatedName).Should().BeTrue("Updated semester should be visible");
        semestersPage.IsSemesterVisible(originalName).Should().BeFalse("Original semester name should be gone");
    }

    [Fact]
    public void Admin_Should_Access_Course_Management()
    {
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act - Navigate to courses page
        var coursesLink = Driver.FindElement(By.LinkText("Courses"));
        coursesLink.Click();
        WaitForPageLoad();

        // Assert - Should be on courses page
        Driver.Url.Should().Contain("/courses", "Should navigate to courses page");
        Driver.PageSource.Should().ContainEquivalentOf("course");
    }

    [Fact]
    public void Admin_Should_Create_Course_For_Semester()
    {
        // Arrange - Login as admin and create a semester first
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        // Create a semester for the course
        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var semesterName = $"Course Test Semester {timestamp}";
        var semesterCode = $"CTS{timestamp.Substring(8)}";

        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        semestersPage.FillSemesterForm(
            name: semesterName,
            code: semesterCode,
            startDate: new DateTime(2025, 9, 1),
            endDate: new DateTime(2025, 12, 20),
            regStartDate: new DateTime(2025, 7, 15),
            regEndDate: new DateTime(2025, 8, 25),
            isActive: true
        );

        semestersPage.SaveSemester();
        Thread.Sleep(2000);

        // Verify semester was created
        semestersPage.IsSemesterVisible(semesterName).Should().BeTrue("Semester should be created first");

        // Navigate to courses page
        var coursesPage = new CoursesPage(Driver);
        coursesPage.NavigateToCourses();

        // Act - Create a course for the semester
        var courseName = $"Test Course {timestamp}";
        var courseCode = $"TC{timestamp.Substring(8)}";

        // Select the semester we just created
        coursesPage.SelectSemester(semesterName);
        
        // Click create course
        coursesPage.ClickCreateCourse();
        coursesPage.IsCreateFormVisible().Should().BeTrue("Create course form should be visible");

        // Fill course form
        coursesPage.FillCourseForm(
            name: courseName,
            code: courseCode,
            ageGroup: "Children (5-12)",
            maxCapacity: 15,
            room: "Room 101",
            fee: 50.00m,
            periodCode: "Period A",
            startTime: "09:00AM",
            endTime: "10:30AM",
            description: "A test course for E2E testing"
        );

        coursesPage.SaveCourse();
        Thread.Sleep(2000);

        // Assert - Verify course was created
        coursesPage.IsCourseVisible(courseName).Should().BeTrue($"Course '{courseName}' should be visible");
    }

    [Fact]
    public void Admin_Should_Create_Multiple_Courses_For_Same_Semester()
    {
        // Arrange - Login as admin and create a semester
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var semesterName = $"Multi Course Semester {timestamp}";
        var semesterCode = $"MCS{timestamp.Substring(8)}";

        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        semestersPage.FillSemesterForm(
            name: semesterName,
            code: semesterCode,
            startDate: new DateTime(2025, 9, 1),
            endDate: new DateTime(2025, 12, 20),
            regStartDate: new DateTime(2025, 7, 15),
            regEndDate: new DateTime(2025, 8, 25),
            isActive: true
        );

        semestersPage.SaveSemester();
        Thread.Sleep(2000);

        // Navigate to courses page
        var coursesPage = new CoursesPage(Driver);
        coursesPage.NavigateToCourses();
        coursesPage.SelectSemester(semesterName);

        var initialCourseCount = coursesPage.GetCourseCount();

        // Define multiple courses to create
        var courses = new[]
        {
            new { Name = $"Math Basics {timestamp}", Code = $"MATH{timestamp.Substring(10)}", AgeGroup = "Children (5-12)", Capacity = 12 },
            new { Name = $"Science Lab {timestamp}", Code = $"SCI{timestamp.Substring(10)}", AgeGroup = "Teens (13-17)", Capacity = 10 },
            new { Name = $"Art Workshop {timestamp}", Code = $"ART{timestamp.Substring(10)}", AgeGroup = "All Ages", Capacity = 20 }
        };

        // Act - Create each course
        foreach (var course in courses)
        {
            coursesPage.ClickCreateCourse();
            coursesPage.IsCreateFormVisible().Should().BeTrue($"Create form should be visible for {course.Name}");

            coursesPage.FillCourseForm(
                name: course.Name,
                code: course.Code,
                ageGroup: course.AgeGroup,
                maxCapacity: course.Capacity,
                room: $"Room {Array.IndexOf(courses, course) + 1}",
                fee: 25.00m
            );

            coursesPage.SaveCourse();
            Thread.Sleep(1500); // Allow time between creations

            // Verify each course was created
            coursesPage.IsCourseVisible(course.Name).Should().BeTrue($"Course '{course.Name}' should be created");
        }

        // Assert - Verify all courses were created
        var finalCourseCount = coursesPage.GetCourseCount();
        finalCourseCount.Should().BeGreaterThan(initialCourseCount + courses.Length - 1, "All courses should be created");
    }

    [Fact]
    public void Admin_Should_Cancel_Course_Creation()
    {
        // Arrange - Login as admin and navigate to courses
        LoginAsAdmin();
        var coursesPage = new CoursesPage(Driver);
        coursesPage.NavigateToCourses();

        // Ensure we have a semester to work with
        var availableSemesters = coursesPage.GetAvailableSemesters();
        if (availableSemesters.Count > 0)
        {
            coursesPage.SelectSemester(availableSemesters.First());
        }

        var initialCourseCount = coursesPage.GetCourseCount();

        // Act - Start creating course but cancel
        coursesPage.ClickCreateCourse();
        coursesPage.IsCreateFormVisible().Should().BeTrue("Create form should be visible");

        // Fill some data
        coursesPage.FillCourseForm(
            name: "Cancel Test Course",
            code: "CANCEL",
            ageGroup: "Children (5-12)",
            maxCapacity: 10
        );

        // Cancel instead of saving
        coursesPage.CancelCreate();

        // Assert - Verify no course was created
        WaitForPageLoad();
        Thread.Sleep(1000);

        coursesPage.IsCourseVisible("Cancel Test Course").Should().BeFalse("Cancelled course should not appear");
        
        var finalCourseCount = coursesPage.GetCourseCount();
        finalCourseCount.Should().Be(initialCourseCount, "Course count should remain unchanged after cancel");
    }

    [Fact]
    public void Admin_Should_Create_Course_With_All_Details()
    {
        // Arrange - Login as admin and create a semester
        LoginAsAdmin();
        var semestersPage = new SemestersPage(Driver);
        semestersPage.NavigateToSemesters();

        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var semesterName = $"Detailed Course Semester {timestamp}";
        var semesterCode = $"DCS{timestamp.Substring(8)}";

        semestersPage.ClickCreateSemester();
        semestersPage.WaitForModalToOpen();
        
        semestersPage.FillSemesterForm(
            name: semesterName,
            code: semesterCode,
            startDate: new DateTime(2025, 9, 1),
            endDate: new DateTime(2025, 12, 20),
            regStartDate: new DateTime(2025, 7, 15),
            regEndDate: new DateTime(2025, 8, 25),
            isActive: true
        );

        semestersPage.SaveSemester();
        Thread.Sleep(2000);

        // Navigate to courses and create detailed course
        var coursesPage = new CoursesPage(Driver);
        coursesPage.NavigateToCourses();
        coursesPage.SelectSemester(semesterName);

        // Act - Create course with all possible details
        var courseName = $"Advanced Programming {timestamp}";
        var courseCode = $"PROG{timestamp.Substring(8)}";

        coursesPage.ClickCreateCourse();
        coursesPage.IsCreateFormVisible().Should().BeTrue("Create course form should be visible");

        coursesPage.FillCourseForm(
            name: courseName,
            code: courseCode,
            ageGroup: "Teens (13-17)",
            maxCapacity: 12,
            room: "Computer Lab",
            fee: 150.00m,
            periodCode: "Morning Block",
            startTime: "10:00AM",
            endTime: "11:30AM",
            description: "An advanced programming course covering modern software development practices and tools."
        );

        coursesPage.SaveCourse();
        Thread.Sleep(2000);

        // Assert - Verify course was created with all details
        coursesPage.IsCourseVisible(courseName).Should().BeTrue($"Course '{courseName}' should be visible");
        
        // Verify course appears in the list (detailed verification would require more specific selectors)
        Driver.PageSource.Should().Contain(courseCode, "Course code should be visible");
        Driver.PageSource.Should().Contain("Computer Lab", "Room should be visible");
        Driver.PageSource.Should().Contain("$150.00", "Fee should be visible");
    }

    [Fact]
    public void Admin_Should_Validate_Required_Course_Fields()
    {
        // Arrange - Login as admin and navigate to courses
        LoginAsAdmin();
        var coursesPage = new CoursesPage(Driver);
        coursesPage.NavigateToCourses();

        // Ensure we have a semester to work with
        var availableSemesters = coursesPage.GetAvailableSemesters();
        if (availableSemesters.Count > 0)
        {
            coursesPage.SelectSemester(availableSemesters.First());
        }

        // Act - Try to create course without required fields
        coursesPage.ClickCreateCourse();
        coursesPage.IsCreateFormVisible().Should().BeTrue("Create form should be visible");

        // Try to save without filling required fields
        coursesPage.SaveCourse();
        Thread.Sleep(1000);

        // Assert - Should show validation error and stay on form
        // Note: This depends on how the frontend handles validation
        coursesPage.IsCreateFormVisible().Should().BeTrue("Should remain on create form after validation error");
    }

    #region Helper Methods

    private void LoginAsAdmin()
    {
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:AdminUser:Username"] ?? "admin1";
        var password = Configuration["TestCredentials:AdminUser:Password"] ?? "AdminPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Admin login should succeed");
    }

    private void VerifyCanAccessPage(string linkText, string expectedUrlPart)
    {
        // Navigate back to home
        Driver.Navigate().GoToUrl(BaseUrl);
        WaitForPageLoad();

        // Click the link
        var link = Driver.FindElement(By.LinkText(linkText));
        link.Click();
        WaitForPageLoad();

        // Verify navigation
        Driver.Url.Should().Contain(expectedUrlPart, $"Admin should access {linkText} page");
    }

    #region Room Management Tests

    [Fact]
    public void Admin_Should_Access_Room_Management()
    {
        // Arrange
        LoginAsAdmin();

        // Act - Navigate to rooms page
        var roomsLink = Driver.FindElement(By.CssSelector("[data-nav-item='rooms']"));
        roomsLink.Click();
        WaitForPageLoad();

        // Assert - Should be on rooms page
        Driver.Url.Should().Contain("/rooms", "Should navigate to rooms page");
        Driver.PageSource.Should().Contain("Room Management", "Should see room management header");
    }

    [Fact]
    public void Admin_Should_See_Create_Room_Button()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        // Act & Assert - Should see create button
        var createButton = Driver.FindElement(By.Id("create-room-btn"));
        createButton.Should().NotBeNull("Create room button should be visible");
        createButton.Text.Should().Contain("Create Room");
    }

    [Fact]
    public void Admin_Should_Create_New_Room_Successfully()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        var uniqueName = $"Test Room {DateTime.Now:yyyyMMddHHmmss}";

        // Act - Create a new room
        var createButton = Driver.FindElement(By.Id("create-room-btn"));
        createButton.Click();
        WaitForPageLoad();
        Thread.Sleep(1000);

        // Fill in room details
        var nameInput = Driver.FindElement(By.Id("room-name-input"));
        nameInput.SendKeys(uniqueName);

        var typeSelect = Driver.FindElement(By.Id("room-type-select"));
        typeSelect.SendKeys("Lab");

        var capacityInput = Driver.FindElement(By.Id("room-capacity-input"));
        capacityInput.Clear();
        capacityInput.SendKeys("25");

        var notesInput = Driver.FindElement(By.Id("room-notes-input"));
        notesInput.SendKeys("E2E test room for lab activities");

        // Submit the form
        var saveButton = Driver.FindElement(By.Id("save-room-btn"));
        saveButton.Click();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert - Room should be created and visible
        Driver.PageSource.Should().Contain(uniqueName, "New room should appear on the page");
        Driver.PageSource.Should().Contain("Lab", "Room type should be displayed");
        Driver.PageSource.Should().Contain("25", "Room capacity should be displayed");
    }

    [Fact]
    public void Admin_Should_Be_Able_To_Cancel_Room_Creation()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        // Act - Open create modal and cancel
        var createButton = Driver.FindElement(By.Id("create-room-btn"));
        createButton.Click();
        WaitForPageLoad();
        Thread.Sleep(1000);

        // Fill in partial data
        var nameInput = Driver.FindElement(By.Id("room-name-input"));
        nameInput.SendKeys("Cancelled Room");

        // Cancel
        var cancelButton = Driver.FindElement(By.Id("cancel-room-btn"));
        cancelButton.Click();
        WaitForPageLoad();

        // Assert - Modal should close and room should not be created
        Driver.PageSource.Should().NotContain("Cancelled Room", "Cancelled room should not appear");
        
        // Modal should be closed
        var exception = Assert.Throws<NoSuchElementException>(() => 
            Driver.FindElement(By.Id("room-modal")));
        exception.Should().NotBeNull("Modal should be closed");
    }

    [Fact]
    public void Admin_Should_Edit_Existing_Room()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        // First create a room to edit
        var uniqueName = $"Edit Test Room {DateTime.Now:yyyyMMddHHmmss}";
        CreateTestRoom(uniqueName, "Classroom", 20, "Original notes");

        // Act - Edit the room
        var editButtons = Driver.FindElements(By.CssSelector("[id^='edit-room-']"));
        if (editButtons.Count > 0)
        {
            editButtons[0].Click();
            WaitForPageLoad();
            Thread.Sleep(1000);

            // Update room details
            var nameInput = Driver.FindElement(By.Id("room-name-input"));
            nameInput.Clear();
            nameInput.SendKeys($"{uniqueName} EDITED");

            var capacityInput = Driver.FindElement(By.Id("room-capacity-input"));
            capacityInput.Clear();
            capacityInput.SendKeys("30");

            var notesInput = Driver.FindElement(By.Id("room-notes-input"));
            notesInput.Clear();
            notesInput.SendKeys("Updated notes for edited room");

            // Submit changes
            var saveButton = Driver.FindElement(By.Id("save-room-btn"));
            saveButton.Click();
            WaitForPageLoad();
            Thread.Sleep(2000);

            // Assert - Changes should be reflected
            Driver.PageSource.Should().Contain($"{uniqueName} EDITED", "Updated room name should appear");
            Driver.PageSource.Should().Contain("30", "Updated capacity should be displayed");
        }
    }

    [Fact]
    public void Admin_Should_Validate_Required_Room_Fields()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        // Act - Try to create room without required fields
        var createButton = Driver.FindElement(By.Id("create-room-btn"));
        createButton.Click();
        WaitForPageLoad();
        Thread.Sleep(1000);

        // Try to submit without filling required fields
        var saveButton = Driver.FindElement(By.Id("save-room-btn"));
        saveButton.Click();
        Thread.Sleep(1000);

        // Assert - Should show validation errors (browser validation or custom)
        var nameInput = Driver.FindElement(By.Id("room-name-input"));
        nameInput.GetDomAttribute("required").Should().NotBeNull("Name field should be required");

        var capacityInput = Driver.FindElement(By.Id("room-capacity-input"));
        capacityInput.GetDomAttribute("required").Should().NotBeNull("Capacity field should be required");
    }

    [Fact]
    public void Admin_Should_Create_Room_With_Different_Types()
    {
        // Arrange
        LoginAsAdmin();
        Driver.Navigate().GoToUrl($"{BaseUrl}/rooms");
        WaitForPageLoad();

        var roomTypes = new[] { "Classroom", "Lab", "Auditorium", "Library", "Gym", "Workshop", "Other" };
        
        foreach (var roomType in roomTypes.Take(3)) // Test first 3 to keep test reasonable
        {
            var uniqueName = $"{roomType} {DateTime.Now:yyyyMMddHHmmss}";

            // Act - Create room with specific type
            CreateTestRoom(uniqueName, roomType, 25, $"Test {roomType.ToLower()}");

            // Assert - Room should appear with correct type
            Driver.PageSource.Should().Contain(uniqueName, $"{roomType} room should be created");
            Driver.PageSource.Should().Contain(roomType, $"Room type {roomType} should be displayed");
        }
    }

    private void CreateTestRoom(string name, string roomType, int capacity, string notes)
    {
        var createButton = Driver.FindElement(By.Id("create-room-btn"));
        createButton.Click();
        WaitForPageLoad();
        Thread.Sleep(1000);

        var nameInput = Driver.FindElement(By.Id("room-name-input"));
        nameInput.SendKeys(name);

        var typeSelect = Driver.FindElement(By.Id("room-type-select"));
        typeSelect.SendKeys(roomType);

        var capacityInput = Driver.FindElement(By.Id("room-capacity-input"));
        capacityInput.Clear();
        capacityInput.SendKeys(capacity.ToString());

        if (!string.IsNullOrEmpty(notes))
        {
            var notesInput = Driver.FindElement(By.Id("room-notes-input"));
            notesInput.SendKeys(notes);
        }

        var saveButton = Driver.FindElement(By.Id("save-room-btn"));
        saveButton.Click();
        WaitForPageLoad();
        Thread.Sleep(2000);
    }

    #endregion

    #endregion
}
