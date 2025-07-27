# Student Registrar E2E Tests

This project contains end-to-end (E2E) tests using Selenium WebDriver to test the Student Registrar application.

## Prerequisites

1. **Chrome Browser**: Tests are configured to use Chrome WebDriver
2. **Running Application**: The Student Registrar application should be running locally
3. **.NET 9.0**: Required for running the test project

## Setup

### 1. Install Chrome Driver

The project includes the `Selenium.WebDriver.ChromeDriver` package which should automatically download the correct Chrome driver. However, ensure you have Chrome browser installed.

### 2. Configure Test Settings

Update `appsettings.json` to match your environment:

```json
{
  "SeleniumSettings": {
    "BaseUrl": "http://localhost:3001",  // Your frontend URL
    "ImplicitWaitSeconds": 10,
    "PageLoadTimeoutSeconds": 30,
    "Headless": false  // Set to true for CI/CD
  },
  "TestCredentials": {
    "ValidUser": {
      "Username": "testuser",
      "Password": "testpass"
    }
  }
}
```

### 3. Start the Application

Before running tests, ensure the Student Registrar application is running:

```bash
# From the root directory
dotnet run --project src/StudentRegistrar.AppHost
```

## Running Tests

### Command Line

```bash
# Run all E2E tests
dotnet test tests/StudentRegistrar.E2E.Tests/

# Run specific test
dotnet test tests/StudentRegistrar.E2E.Tests/ --filter "Should_Navigate_To_Home_Page_Successfully"

# Run with detailed output
dotnet test tests/StudentRegistrar.E2E.Tests/ --logger "console;verbosity=detailed"
```

### Visual Studio

1. Open the solution in Visual Studio
2. Open Test Explorer (Test > Test Explorer)
3. Build the solution
4. Run tests from Test Explorer

## Test Structure

### Base Classes

- **`BaseTest`**: Base class for all E2E tests that handles WebDriver setup and common utilities
- **`WebDriverFactory`**: Factory for creating and configuring WebDriver instances

### Page Objects

- **`HomePage`**: Represents the application home page
- **`LoginPage`**: Represents the Keycloak login page

### Test Categories

- **`LoginTests`**: Tests related to authentication and login flow

## Test Cases

### Current Tests

1. **Should_Navigate_To_Home_Page_Successfully**: Verifies the home page loads
2. **Should_Display_Login_Button_On_Home_Page**: Checks for login elements on home page
3. **Should_Redirect_To_Keycloak_When_Login_Clicked**: Tests navigation to authentication
4. **Should_Show_Keycloak_Login_Form**: Verifies Keycloak login form appears
5. **Should_Handle_Invalid_Login_Credentials**: Tests error handling for bad credentials

### Future Test Ideas

- **Semester Management**: Create, edit, delete semesters
- **Course Management**: Add courses to semesters, manage instructors
- **Student Enrollment**: Add students, enroll in courses
- **Payment Processing**: Test payment workflows
- **Admin Functions**: User management, system settings

## Configuration Options

### Headless Mode

For CI/CD environments, set `"Headless": true` in `appsettings.json` to run tests without opening browser windows.

### Timeouts

Adjust timeout settings based on your environment:

- `ImplicitWaitSeconds`: How long to wait for elements to appear
- `PageLoadTimeoutSeconds`: Maximum time to wait for page loads

### Test Data

Update `TestCredentials` section with valid test user accounts from your Keycloak setup.

## Troubleshooting

### Chrome Driver Issues

If you encounter Chrome driver compatibility issues:

1. Update the `Selenium.WebDriver.ChromeDriver` package
2. Ensure Chrome browser is up to date
3. Check Chrome version compatibility with driver version

### Test Timeouts

If tests are timing out:

1. Increase timeout values in `appsettings.json`
2. Check if the application is running and accessible
3. Verify network connectivity

### Element Not Found

If tests fail due to missing elements:

1. Check if the application UI has changed
2. Update element selectors in page objects
3. Add appropriate waits for dynamic content

## Contributing

When adding new tests:

1. Follow the Page Object Model pattern
2. Use meaningful test names that describe the behavior
3. Add appropriate assertions using FluentAssertions
4. Include error handling for flaky scenarios
5. Update this README with new test descriptions

## CI/CD Integration

For automated testing in CI/CD pipelines:

1. Set `"Headless": true` in configuration
2. Ensure Chrome is installed in the CI environment
3. Consider using Docker containers with Chrome pre-installed
4. Run tests after application deployment but before production release
