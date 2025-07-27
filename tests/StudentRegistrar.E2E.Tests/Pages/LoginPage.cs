using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StudentRegistrar.E2E.Tests.Pages;

public class LoginPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public LoginPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // Page elements - These selectors will need to be updated based on your actual Keycloak login form
    public IWebElement UsernameField => _wait.Until(d => d.FindElement(By.Id("username")));
    public IWebElement PasswordField => _driver.FindElement(By.Id("password"));
    public IWebElement LoginButton => _driver.FindElement(By.CssSelector("input[type='submit'], button[type='submit']"));
    public IWebElement ErrorMessage => _driver.FindElement(By.CssSelector(".alert-error, .error-message, #kc-error-message"));

    // Page actions
    public void EnterUsername(string username)
    {
        UsernameField.Clear();
        UsernameField.SendKeys(username);
    }

    public void EnterPassword(string password)
    {
        PasswordField.Clear();
        PasswordField.SendKeys(password);
    }

    public void ClickLogin()
    {
        LoginButton.Click();
    }

    public void Login(string username, string password)
    {
        EnterUsername(username);
        EnterPassword(password);
        ClickLogin();
    }

    // Page validations
    public bool IsOnLoginPage()
    {
        try
        {
            return UsernameField.Displayed && PasswordField.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool HasErrorMessage()
    {
        try
        {
            return ErrorMessage.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public string GetErrorMessage()
    {
        return HasErrorMessage() ? ErrorMessage.Text : string.Empty;
    }
}
