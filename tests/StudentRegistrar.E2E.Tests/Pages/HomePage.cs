using OpenQA.Selenium;

namespace StudentRegistrar.E2E.Tests.Pages;

public class HomePage
{
    private readonly IWebDriver _driver;

    public HomePage(IWebDriver driver)
    {
        _driver = driver;
    }

    // Page elements
    public IWebElement LoginButton => _driver.FindElement(By.CssSelector("[data-testid='login-button']"));
    public IWebElement PageTitle => _driver.FindElement(By.TagName("h1"));
    public IWebElement NavigationBar => _driver.FindElement(By.TagName("nav"));

    // Page actions
    public void ClickLogin()
    {
        LoginButton.Click();
    }

    // Page validations
    public bool IsLoaded()
    {
        try
        {
            return PageTitle.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public string GetPageTitle()
    {
        return PageTitle.Text;
    }

    public bool IsLoggedIn()
    {
        try
        {
            // Look for user menu or logout button to determine if logged in
            var userMenu = _driver.FindElement(By.CssSelector("[data-testid='user-menu']"));
            return userMenu.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}
