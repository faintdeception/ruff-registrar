using OpenQA.Selenium;
using FluentAssertions;
using StudentRegistrar.E2E.Tests.Base;
using StudentRegistrar.E2E.Tests.Pages;
using Xunit;

namespace StudentRegistrar.E2E.Tests.Tests.RoleBasedTests;

public class EducatorTests : BaseTest
{
    [Fact]
    public void Educator_Should_Login_Successfully()
    {
        // Arrange
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Act - Login as educator
        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:EducatorUser:Username"] ?? "educator1";
        var password = Configuration["TestCredentials:EducatorUser:Password"] ?? "EducatorPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        // Assert - Should be logged in
        Driver.Url.Should().NotContain("/login", "Educator should be logged in");
        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Educator should be authenticated");
    }

    [Fact]
    public void Educator_Should_Access_Family_Management()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Act - Navigate to account/family management
        var accountLink = Driver.FindElement(By.LinkText("Account"));
        accountLink.Click();
        WaitForPageLoad();

        // Assert - Should access family management
        Driver.Url.Should().Contain("/account", "Should navigate to account page");
        Driver.PageSource.Should().ContainEquivalentOf("account");
    }

    [Fact]
    public void Educator_Should_Access_Course_Management()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Act - Navigate to courses page
        var coursesLink = Driver.FindElement(By.LinkText("Courses"));
        coursesLink.Click();
        WaitForPageLoad();

        // Assert - Should access courses (to create/manage their own)
        Driver.Url.Should().Contain("/courses", "Should navigate to courses page");
        Driver.PageSource.Should().ContainEquivalentOf("course");
    }

    [Fact]
    public void Educator_Should_Access_Enrollment_Management()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Act - Navigate to enrollments page
        var enrollmentsLink = Driver.FindElement(By.LinkText("Enrollments"));
        enrollmentsLink.Click();
        WaitForPageLoad();

        // Assert - Should access enrollments (for their courses and family)
        Driver.Url.Should().Contain("/enrollments", "Should navigate to enrollments page");
        Driver.PageSource.Should().ContainEquivalentOf("enrollment");
    }

    [Fact]
    public void Educator_Should_Access_Grade_Management()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Act - Navigate to grades page
        var gradesLink = Driver.FindElement(By.LinkText("Grades"));
        gradesLink.Click();
        WaitForPageLoad();

        // Assert - Should access grades (for their courses and children)
        Driver.Url.Should().Contain("/grades", "Should navigate to grades page");
        Driver.PageSource.Should().ContainEquivalentOf("grade");
    }

    [Fact]
    public void Educator_Should_Access_Educator_Section()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Act - Navigate to educators page
        var educatorsLink = Driver.FindElement(By.LinkText("Educators"));
        educatorsLink.Click();
        WaitForPageLoad();

        // Assert - Should access educators section
        Driver.Url.Should().Contain("/educators", "Should navigate to educators page");
        Driver.PageSource.Should().ContainEquivalentOf("educator");
    }

    [Fact]
    public void Educator_Should_NOT_Access_Admin_Features()
    {
        // Arrange - Login as educator
        LoginAsEducator();

        // Assert - Should NOT see admin-only features
        Driver.PageSource.Should().NotContain("Students", "Educator should not see Students admin link");
        Driver.PageSource.Should().NotContain("Semesters", "Educator should not see Semesters admin link");
    }

    [Fact]
    public void Educator_Should_Manage_Teaching_And_Family_Workflow()
    {
        // This test covers the educator workflow:
        // 1. Login
        // 2. Manage own family/children
        // 3. Create/manage own courses
        // 4. Manage enrollments (own courses + own children)
        // 5. Manage grades (own courses + view children's grades)
        
        // Arrange - Login as educator
        LoginAsEducator();

        // Act & Assert - Verify access to educator functions
        
        // 1. Family Management
        VerifyCanAccessPage("Account", "/account");
        
        // 2. Course Management (create own courses)
        VerifyCanAccessPage("Courses", "/courses");
        
        // 3. Enrollment Management (own courses + children)
        VerifyCanAccessPage("Enrollments", "/enrollments");
        
        // 4. Grade Management (own courses + children's grades)
        VerifyCanAccessPage("Grades", "/grades");
        
        // 5. Educator Section
        VerifyCanAccessPage("Educators", "/educators");

        // Verify NO access to admin-only features
        var navigationElements = Driver.FindElements(By.CssSelector("nav a"));
        var navigationText = string.Join(" ", navigationElements.Select(e => e.Text));
        
        navigationText.Should().NotContain("Students", "Educators should not see admin Students link");
        navigationText.Should().NotContain("Semesters", "Educators should not see admin Semesters link");
    }

    #region Helper Methods

    private void LoginAsEducator()
    {
        NavigateToHome();
        WaitForPageLoad();
        Thread.Sleep(2000);

        var loginPage = new LoginPage(Driver);
        var username = Configuration["TestCredentials:EducatorUser:Username"] ?? "educator1";
        var password = Configuration["TestCredentials:EducatorUser:Password"] ?? "EducatorPass123!";
        
        loginPage.Login(username, password);
        WaitForPageLoad();
        Thread.Sleep(2000);

        var homePage = new HomePage(Driver);
        homePage.IsLoggedIn().Should().BeTrue("Educator login should succeed");
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
        Driver.Url.Should().Contain(expectedUrlPart, $"Educator should access {linkText} page");
    }

    #endregion
}
