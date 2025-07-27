using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace StudentRegistrar.E2E.Tests.Infrastructure;

public class WebDriverFactory : IDisposable
{
    private IWebDriver? _driver;
    private readonly IConfiguration _configuration;

    public WebDriverFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IWebDriver CreateDriver()
    {
        if (_driver != null)
            return _driver;

        var options = new ChromeOptions();
        
        // Configure Chrome options based on settings
        var headless = bool.Parse(_configuration["SeleniumSettings:Headless"] ?? "false");
        if (headless)
        {
            options.AddArgument("--headless");
        }

        // Add additional Chrome options for better stability
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        _driver = new ChromeDriver(options);

        // Configure timeouts
        var implicitWait = int.Parse(_configuration["SeleniumSettings:ImplicitWaitSeconds"] ?? "10");
        var pageLoadTimeout = int.Parse(_configuration["SeleniumSettings:PageLoadTimeoutSeconds"] ?? "30");

        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
        _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(pageLoadTimeout);

        return _driver;
    }

    public void Dispose()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }
}
