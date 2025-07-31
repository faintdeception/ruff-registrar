using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StudentRegistrar.E2E.Tests.Pages;

public class CoursesPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public CoursesPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // Page elements
    private IWebElement SemesterSelect => _driver.FindElement(By.CssSelector("select"));
    private IWebElement CreateCourseButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Add Course') or contains(text(), 'Add First Course')]"));
    private IWebElement CourseModal => _driver.FindElement(By.CssSelector(".fixed.inset-0"));
    private IWebElement CourseNameInput => _driver.FindElement(By.Id("name"));
    private IWebElement CourseCodeInput => _driver.FindElement(By.Id("code"));
    private IWebElement AgeGroupSelect => _driver.FindElement(By.Id("ageGroup"));
    private IWebElement MaxCapacityInput => _driver.FindElement(By.Id("maxCapacity"));
    private IWebElement RoomInput => _driver.FindElement(By.Id("room"));
    private IWebElement FeeInput => _driver.FindElement(By.Id("fee"));
    private IWebElement PeriodCodeInput => _driver.FindElement(By.Id("periodCode"));
    private IWebElement StartTimeInput => _driver.FindElement(By.Id("startTime"));
    private IWebElement EndTimeInput => _driver.FindElement(By.Id("endTime"));
    private IWebElement DescriptionInput => _driver.FindElement(By.Id("description"));
    private IWebElement SaveCourseButton => _driver.FindElement(By.XPath("//button[@type='submit' and contains(text(), 'Create Course')]"));
    private IWebElement CancelCourseButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Cancel')]"));

    // Navigation
    public void NavigateToCourses()
    {
        var coursesLink = _driver.FindElement(By.LinkText("Courses"));
        coursesLink.Click();
        WaitForPageLoad();
    }

    public void WaitForPageLoad()
    {
        _wait.Until(driver => driver.Url.Contains("/courses"));
    }

    // Actions
    /// <summary>
    /// Selects a semester by matching the beginning of the display text.
    /// This is the recommended method as it handles cases where the display text 
    /// includes additional information like dates in parentheses.
    /// Example: "Detailed Course Semester 20250729210558 (Aug 31, 2025 - Dec 19, 2025)"
    /// Can be selected with: "Detailed Course Semester"
    /// </summary>
    public void SelectSemester(string semesterName)
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        
        // Find option that starts with the semester name (handles dates in parentheses)
        var option = semesterSelect.Options
            .FirstOrDefault(opt => opt.Text.StartsWith(semesterName, StringComparison.OrdinalIgnoreCase));
        
        if (option != null)
        {
            option.Click();
        }
        else
        {
            throw new NoSuchElementException($"Could not find semester option starting with: {semesterName}");
        }
        
        Thread.Sleep(1000); // Wait for courses to load
    }

    public void SelectSemesterByExactText(string exactText)
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        semesterSelect.SelectByText(exactText);
        Thread.Sleep(1000); // Wait for courses to load
    }

    public void SelectSemesterByPartialText(string partialText)
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        
        var option = semesterSelect.Options
            .FirstOrDefault(opt => opt.Text.Contains(partialText, StringComparison.OrdinalIgnoreCase));
        
        if (option != null)
        {
            option.Click();
        }
        else
        {
            throw new NoSuchElementException($"Could not find semester option containing: {partialText}");
        }
        
        Thread.Sleep(1000); // Wait for courses to load
    }

    public void SelectSemesterByValue(string value)
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        semesterSelect.SelectByValue(value);
        Thread.Sleep(1000); // Wait for courses to load
    }

    public void ClickCreateCourse()
    {
        CreateCourseButton.Click();
        WaitForModalToOpen();
    }

    public void WaitForModalToOpen()
    {
        _wait.Until(driver =>
        {
            try
            {
                return CourseModal.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        });
    }

    public void WaitForModalToClose()
    {
        _wait.Until(driver =>
        {
            try
            {
                return !CourseModal.Displayed;
            }
            catch (NoSuchElementException)
            {
                return true;
            }
        });
    }

    public void FillCourseForm(string name, string code = "", string ageGroup = "", 
                              int maxCapacity = 20, string room = "", decimal fee = 0, 
                              string periodCode = "", string startTime = "", 
                              string endTime = "", string description = "")
    {
        CourseNameInput.Clear();
        CourseNameInput.SendKeys(name);

        if (!string.IsNullOrEmpty(code))
        {
            CourseCodeInput.Clear();
            CourseCodeInput.SendKeys(code);
        }

        if (!string.IsNullOrEmpty(ageGroup))
        {
            var ageGroupSelect = new SelectElement(AgeGroupSelect);
            ageGroupSelect.SelectByText(ageGroup);
        }

        MaxCapacityInput.Clear();
        MaxCapacityInput.SendKeys(maxCapacity.ToString());

        if (!string.IsNullOrEmpty(room))
        {
            RoomInput.Clear();
            RoomInput.SendKeys(room);
        }

        FeeInput.Clear();
        FeeInput.SendKeys(fee.ToString());

        if (!string.IsNullOrEmpty(periodCode))
        {
            PeriodCodeInput.Clear();
            PeriodCodeInput.SendKeys(periodCode);
        }

        if (!string.IsNullOrEmpty(startTime))
        {
            StartTimeInput.Clear();
            StartTimeInput.SendKeys(startTime);
        }

        if (!string.IsNullOrEmpty(endTime))
        {
            EndTimeInput.Clear();
            EndTimeInput.SendKeys(endTime);
        }

        if (!string.IsNullOrEmpty(description))
        {
            DescriptionInput.Clear();
            DescriptionInput.SendKeys(description);
        }
    }

    public void SaveCourse()
    {
        SaveCourseButton.Click();
        //WaitForModalToClose();
    }

    public void CancelCreate()
    {
        CancelCourseButton.Click();
        //WaitForModalToClose();
    }

    // Verification methods
    public bool IsOnCoursesPage()
    {
        return _driver.Url.Contains("/courses") && 
               _driver.PageSource.ToLower().Contains("course");
    }

    public bool CanSeeCreateButton()
    {
        try
        {
            return CreateCourseButton.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool IsCreateFormVisible()
    {
        try
        {
            return CourseModal.Displayed && CourseNameInput.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool IsCourseVisible(string courseName)
    {
        try
        {
            var courseCard = _driver.FindElement(By.XPath($"//h3[contains(text(), '{courseName}')]"));
            return courseCard.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public int GetCourseCount()
    {
        try
        {
            var courseCards = _driver.FindElements(By.CssSelector(".bg-white.rounded-lg.shadow"));
            return courseCards.Count;
        }
        catch (NoSuchElementException)
        {
            return 0;
        }
    }

    public bool IsErrorDisplayed()
    {
        try
        {
            var errorElement = _driver.FindElement(By.CssSelector(".bg-red-50"));
            return errorElement.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public string GetErrorMessage()
    {
        try
        {
            var errorElement = _driver.FindElement(By.CssSelector(".bg-red-50 .text-red-600"));
            return errorElement.Text;
        }
        catch (NoSuchElementException)
        {
            return "";
        }
    }

    public string GetSuccessMessage()
    {
        try
        {
            var successElement = _driver.FindElement(By.CssSelector(".bg-green-50"));
            return successElement.Text;
        }
        catch (NoSuchElementException)
        {
            return "";
        }
    }

    public List<string> GetAvailableSemesters()
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        return semesterSelect.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Text) && 
                           option.Text != "Select a semester..." &&
                           !string.IsNullOrWhiteSpace(option.GetDomAttribute("value")))
            .Select(option => option.Text)
            .ToList();
    }

    public List<(string Text, string Value)> GetAvailableSemestersWithValues()
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        return semesterSelect.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Text) && 
                           option.Text != "Select a semester..." &&
                           !string.IsNullOrWhiteSpace(option.GetDomAttribute("value")))
            .Select(option => (option.Text, option.GetDomAttribute("value")))
            .ToList();
    }

    public List<string> GetAvailableSemesterNames()
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        return semesterSelect.Options
            .Where(option => !string.IsNullOrWhiteSpace(option.Text) && 
                           option.Text != "Select a semester..." &&
                           !string.IsNullOrWhiteSpace(option.GetDomAttribute("value")))
            .Select(option => {
                var text = option.Text;
                // Extract just the semester name (before the parentheses with dates)
                var parenIndex = text.IndexOf(" (");
                return parenIndex > 0 ? text.Substring(0, parenIndex) : text;
            })
            .ToList();
    }

    public string GetSelectedSemester()
    {
        var semesterSelect = new SelectElement(SemesterSelect);
        return semesterSelect.SelectedOption.Text;
    }
}
