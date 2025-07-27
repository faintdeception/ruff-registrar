using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using StudentRegistrar.E2E.Tests.Infrastructure;
using Xunit;

namespace StudentRegistrar.E2E.Tests.Base;

public abstract class BaseTest : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly IConfiguration Configuration;
    protected readonly string BaseUrl;
    private readonly WebDriverFactory _driverFactory;

    protected BaseTest()
    {
        // Build configuration
        Configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        BaseUrl = Configuration["SeleniumSettings:BaseUrl"] ?? "http://localhost:3001";

        // Create driver
        _driverFactory = new WebDriverFactory(Configuration);
        Driver = _driverFactory.CreateDriver();
        
        // Set implicit wait after driver creation
        var implicitWait = int.Parse(Configuration["SeleniumSettings:ImplicitWaitSeconds"] ?? "10");
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
    }

    protected void NavigateToHome()
    {
        try
        {
            Driver.Navigate().GoToUrl(BaseUrl);
            // Wait a moment for the page to load
            WaitForPageLoad();
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to navigate to {BaseUrl}. Make sure the application is running.", ex);
        }
    }

    protected void WaitForElement(By locator, int timeoutSeconds = 10)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            wait.Until(driver => driver.FindElement(locator));
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException)
        {
            throw new Exception($"Element with locator '{locator}' was not found within {timeoutSeconds} seconds");
        }
    }

    protected void WaitForPageLoad(int timeoutSeconds = 30)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
        catch (Exception)
        {
            // If JavaScript execution fails, just wait a bit
            Thread.Sleep(2000);
        }
    }

    protected bool IsElementPresent(By locator)
    {
        try
        {
            // Temporarily reduce implicit wait for this check
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1);
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
        finally
        {
            // Restore original implicit wait
            var implicitWait = int.Parse(Configuration["SeleniumSettings:ImplicitWaitSeconds"] ?? "10");
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
        }
    }

    public virtual void Dispose()
    {
        try
        {
            _driverFactory?.Dispose();
        }
        catch (Exception)
        {
            // Ignore cleanup errors
        }
    }
}
