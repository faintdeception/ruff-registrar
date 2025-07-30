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
    public void Admin_Should_Access_Student_Management()
    {
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act - Navigate to students page
        var studentsLink = Driver.FindElement(By.LinkText("Students"));
        studentsLink.Click();
        WaitForPageLoad();

        // Assert - Should be on students page
        Driver.Url.Should().Contain("/students", "Should navigate to students page");
        Driver.PageSource.Should().ContainEquivalentOf("student");
    }

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
            ("Students", "/students"),
            ("Courses", "/courses"),
            ("Semesters", "/semesters"),
            // ("Enrollments", "/enrollments"),
            ("Grades", "/grades"),
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
            code: "EDIT2025",
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
            code: "UPDATED2025",
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

    #endregion
}
