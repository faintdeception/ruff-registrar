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
    }

    protected void NavigateToHome()
    {
        Driver.Navigate().GoToUrl(BaseUrl);
    }

    protected void WaitForElement(By locator, int timeoutSeconds = 10)
    {
        var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(driver => driver.FindElement(locator));
    }

    protected bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public virtual void Dispose()
    {
        _driverFactory?.Dispose();
    }
}
