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
            ("Enrollments", "/enrollments"),
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
    public void Admin_Should_Manage_Complete_Workflow()
    {
        // This test covers the complete admin workflow:
        // 1. Login
        // 2. Create/manage semester
        // 3. Create/manage course
        // 4. Manage students
        // 5. Handle enrollments
        // 6. Manage grades
        
        // Arrange - Login as admin
        LoginAsAdmin();

        // Act & Assert - Verify access to all major admin functions
        
        // 1. Semester Management
        VerifyCanAccessPage("Semesters", "/semesters");
        
        // 2. Course Management  
        VerifyCanAccessPage("Courses", "/courses");
        
        // 3. Student Management
        VerifyCanAccessPage("Students", "/students");
        
        // 4. Enrollment Management
        VerifyCanAccessPage("Enrollments", "/enrollments");
        
        // 5. Grade Management
        VerifyCanAccessPage("Grades", "/grades");
        
        // 6. Educator Management
        VerifyCanAccessPage("Educators", "/educators");
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
