using OpenQA.Selenium;
using FluentAssertions;
using StudentRegistrar.E2E.Tests.Base;
using StudentRegistrar.E2E.Tests.Pages;
using Xunit;

namespace StudentRegistrar.E2E.Tests.Tests.RoleBasedTests;

public class MemberTests : BaseTest
{
    [Fact]
    public void Member_Should_Login_Successfully()
    {
        // Arrange
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Act - Login as basic member
        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:MemberUser:Username"] ?? "member1";
        var password = Configuration["TestCredentials:MemberUser:Password"] ?? "MemberPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert - Should be logged in
        Driver.Url.Should().NotContain("/login", "Member should be logged in");
        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Member should be authenticated");
    }

    [Fact]
    public void Member_Should_Access_Family_Management()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Act - Navigate to account/family management
        var accountLink = Driver.FindElement(By.LinkText("Account"));
        accountLink.Click();
        WaitForPageLoad();

        // Assert - Should access family management
        Driver.Url.Should().Contain("/account", "Should navigate to account page");
        Driver.PageSource.Should().ContainEquivalentOf("account");
    }

    [Fact]
    public void Member_Should_View_Available_Courses()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Act - Navigate to courses page (view only)
        var coursesLink = Driver.FindElement(By.LinkText("Courses"));
        coursesLink.Click();
        WaitForPageLoad();

        // Assert - Should view available courses
        Driver.Url.Should().Contain("/courses", "Should navigate to courses page");
        Driver.PageSource.Should().ContainEquivalentOf("course");
    }

    [Fact]
    public void Member_Should_Manage_Enrollments()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Act - Navigate to enrollments page
        var enrollmentsLink = Driver.FindElement(By.LinkText("Enrollments"));
        enrollmentsLink.Click();
        WaitForPageLoad();

        // Assert - Should manage enrollments for their children
        Driver.Url.Should().Contain("/enrollments", "Should navigate to enrollments page");
        Driver.PageSource.Should().ContainEquivalentOf("enrollment");
    }

    [Fact]
    public void Member_Should_View_Grades()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Act - Navigate to grades page
        var gradesLink = Driver.FindElement(By.LinkText("Grades"));
        gradesLink.Click();
        WaitForPageLoad();

        // Assert - Should view their children's grades
        Driver.Url.Should().Contain("/grades", "Should navigate to grades page");
        Driver.PageSource.Should().ContainEquivalentOf("grade");
    }

    [Fact]
    public void Member_Should_NOT_Access_Admin_Or_Educator_Features()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Assert - Should NOT see admin-only or educator-specific features
        Driver.PageSource.Should().NotContain("Students", "Member should not see Students admin link");
        Driver.PageSource.Should().NotContain("Semesters", "Member should not see Semesters admin link");
        
        // Members should see Educators link (to contact/view) but not manage
        // This depends on your business rules - adjust as needed
    }

    [Fact]
    public void Member_Should_Complete_Family_Management_Workflow()
    {
        // This test covers the basic member workflow:
        // 1. Login
        // 2. Manage family/children
        // 3. Browse available courses
        // 4. Enroll children in courses
        // 5. View children's grades and progress
        
        // Arrange - Login as member
        LoginAsMember();

        // Act & Assert - Verify access to member functions
        
        // 1. Family Management
        VerifyCanAccessPage("Account", "/account");
        
        // 2. Course Browsing
        VerifyCanAccessPage("Courses", "/courses");
        
        // 3. Enrollment Management (for children)
        VerifyCanAccessPage("Enrollments", "/enrollments");
        
        // 4. Grade Viewing (children's grades)
        VerifyCanAccessPage("Grades", "/grades");
        
        // 5. Educator Contact/Viewing
        VerifyCanAccessPage("Educators", "/educators");

        // Verify NO access to admin features
        var navigationElements = Driver.FindElements(By.CssSelector("nav a"));
        var navigationText = string.Join(" ", navigationElements.Select(e => e.Text));
        
        navigationText.Should().NotContain("Students", "Members should not see admin Students link");
        navigationText.Should().NotContain("Semesters", "Members should not see admin Semesters link");
    }

    [Fact]
    public void Member_Should_Have_Limited_Navigation_Options()
    {
        // Arrange - Login as member
        LoginAsMember();

        // Act - Get all navigation links
        var navigationElements = Driver.FindElements(By.CssSelector("nav a"));
        var availableLinks = navigationElements.Select(e => e.Text).ToList();

        // Assert - Should only see member-appropriate links
        var expectedMemberLinks = new[] { "Account", "Courses", "Enrollments", "Grades", "Educators" };
        var adminOnlyLinks = new[] { "Students", "Semesters" };

        // Should have access to these
        foreach (var expectedLink in expectedMemberLinks)
        {
            availableLinks.Should().Contain(expectedLink, $"Members should see {expectedLink} link");
        }

        // Should NOT have access to these
        foreach (var adminLink in adminOnlyLinks)
        {
            availableLinks.Should().NotContain(adminLink, $"Members should not see {adminLink} admin link");
        }
    }

    #region Helper Methods

    private void LoginAsMember()
    {
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:MemberUser:Username"] ?? "member1";
        var password = Configuration["TestCredentials:MemberUser:Password"] ?? "MemberPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Member login should succeed");
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
        Driver.Url.Should().Contain(expectedUrlPart, $"Member should access {linkText} page");
    }

    #endregion
}
