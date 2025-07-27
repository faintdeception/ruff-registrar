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
            options.AddArgument("--headless=new"); // Use new headless mode
        }

        // Add Chrome options for better stability and crash prevention
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--disable-software-rasterizer");
        options.AddArgument("--disable-background-timer-throttling");
        options.AddArgument("--disable-backgrounding-occluded-windows");
        options.AddArgument("--disable-renderer-backgrounding");
        options.AddArgument("--disable-features=TranslateUI");
        options.AddArgument("--disable-ipc-flooding-protection");
        options.AddArgument("--window-size=1920,1080");
        options.AddArgument("--start-maximized");
        
        // Memory and performance optimizations
        options.AddArgument("--memory-pressure-off");
        options.AddArgument("--max_old_space_size=4096");
        
        // Disable extensions and plugins that can cause crashes
        options.AddArgument("--disable-extensions");
        options.AddArgument("--disable-plugins");
        options.AddArgument("--disable-default-apps");
        
        // Add user data directory to prevent profile conflicts
        var tempUserDataDir = Path.Combine(Path.GetTempPath(), $"ChromeTest_{Guid.NewGuid()}");
        options.AddArgument($"--user-data-dir={tempUserDataDir}");
        
        // Enable logging for debugging
        options.AddArgument("--enable-logging");
        options.AddArgument("--log-level=0");
        
        // Set page load strategy for better reliability
        options.PageLoadStrategy = PageLoadStrategy.Normal;

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
        try
        {
            // Close all windows and quit properly
            _driver?.Close();
            _driver?.Quit();
        }
        catch (Exception)
        {
            // Ignore errors during cleanup
        }
        finally
        {
            try
            {
                _driver?.Dispose();
            }
            catch (Exception)
            {
                // Ignore errors during disposal
            }
            _driver = null;
        }
    }
}
