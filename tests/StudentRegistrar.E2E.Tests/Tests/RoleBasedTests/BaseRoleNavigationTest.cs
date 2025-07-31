using FluentAssertions;
using StudentRegistrar.E2E.Tests.Base;
using StudentRegistrar.E2E.Tests.Pages;

namespace StudentRegistrar.E2E.Tests.Tests.RoleBasedTests;

/// <summary>
/// Base class for role-based navigation tests.
/// Provides common methods for testing navigation permissions.
/// </summary>
public abstract class BaseRoleNavigationTest : BaseTest
{
    protected NavigationPage NavigationPage => new(Driver);

    /// <summary>
    /// Verifies that a user can navigate to a specific page using the navigation menu
    /// </summary>
    protected void VerifyCanNavigateToPage(string navItem, string expectedUrlPart, string description = null)
    {
        description ??= $"Should be able to navigate to {navItem}";
        
        // Go back to home first to ensure clean navigation
        Driver.Navigate().GoToUrl(BaseUrl);
        WaitForPageLoad();
        
        // Use navigation page to click the nav item
        NavigationPage.ClickNavItem(navItem);
        WaitForPageLoad();
        
        // Verify navigation succeeded
        Driver.Url.Should().Contain(expectedUrlPart, description);
    }

    /// <summary>
    /// Verifies that a navigation item is visible to the current user
    /// </summary>
    protected void VerifyNavItemVisible(string navItem, string description = null)
    {
        description ??= $"{navItem} navigation should be visible";
        NavigationPage.IsNavItemVisible(navItem).Should().BeTrue(description);
    }

    /// <summary>
    /// Verifies that a navigation item is NOT visible to the current user
    /// </summary>
    protected void VerifyNavItemNotVisible(string navItem, string description = null)
    {
        description ??= $"{navItem} navigation should NOT be visible";
        NavigationPage.IsNavItemVisible(navItem).Should().BeFalse(description);
    }

    /// <summary>
    /// Verifies that a navigation item is NOT present in the DOM (not just hidden)
    /// </summary>
    protected void VerifyNavItemNotPresent(string navItem, string description = null)
    {
        description ??= $"{navItem} navigation should NOT be present in DOM";
        NavigationPage.IsNavItemPresent(navItem).Should().BeFalse(description);
    }

    /// <summary>
    /// Verifies user role information in the navigation
    /// </summary>
    protected void VerifyUserRole(string expectedRole, string description = null)
    {
        description ??= $"User should have {expectedRole} role";
        var userRoles = NavigationPage.GetUserRoles();
        userRoles.Should().Contain(expectedRole, description);
    }

    /// <summary>
    /// Verifies user does NOT have a specific role
    /// </summary>
    protected void VerifyUserDoesNotHaveRole(string unexpectedRole, string description = null)
    {
        description ??= $"User should NOT have {unexpectedRole} role";
        var userRoles = NavigationPage.GetUserRoles();
        userRoles.Should().NotContain(unexpectedRole, description);
    }
}
