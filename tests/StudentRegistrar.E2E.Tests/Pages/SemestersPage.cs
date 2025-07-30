using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace StudentRegistrar.E2E.Tests.Pages;

public class SemestersPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public SemestersPage(IWebDriver driver)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    // Page elements using the new test IDs
    private IWebElement CreateSemesterButton => _wait.Until(d => d.FindElement(By.Id("create-semester-btn")));
    private IWebElement CreateFirstSemesterButton => _driver.FindElement(By.Id("create-first-semester-btn"));
    
    // Modal elements
    private IWebElement SemesterModal => _wait.Until(d => d.FindElement(By.Id("semester-modal")));
    private IWebElement ModalTitle => _driver.FindElement(By.Id("modal-title"));
    private IWebElement SemesterNameInput => _driver.FindElement(By.Id("semester-name-input"));
    private IWebElement SemesterCodeInput => _driver.FindElement(By.Id("semester-code-input"));
    private IWebElement StartDateInput => _driver.FindElement(By.Id("semester-start-date-input"));
    private IWebElement EndDateInput => _driver.FindElement(By.Id("semester-end-date-input"));
    private IWebElement RegistrationStartDateInput => _driver.FindElement(By.Id("semester-reg-start-date-input"));
    private IWebElement RegistrationEndDateInput => _driver.FindElement(By.Id("semester-reg-end-date-input"));
    private IWebElement IsActiveCheckbox => _driver.FindElement(By.Id("semester-is-active-checkbox"));
    private IWebElement SaveSemesterButton => _driver.FindElement(By.Id("save-semester-btn"));
    private IWebElement CancelSemesterButton => _driver.FindElement(By.Id("cancel-semester-btn"));
    private IWebElement ErrorMessage => _driver.FindElement(By.Id("error-message"));

    // Navigation
    public void NavigateToSemesters()
    {
        var semestersLink = _driver.FindElement(By.LinkText("Semesters"));
        semestersLink.Click();
        WaitForPageLoad();
    }

    // Actions
    public void ClickCreateSemester()
    {
        try
        {
            CreateSemesterButton.Click();
        }
        catch (NoSuchElementException)
        {
            // If main create button not found, try the "create first semester" button
            CreateFirstSemesterButton.Click();
        }
        WaitForModalToOpen();
    }

    public void WaitForModalToOpen()
    {
        _wait.Until(d => SemesterModal.Displayed);
    }

    public void FillSemesterForm(string name, string code, DateTime startDate, DateTime endDate, 
                                DateTime regStartDate, DateTime regEndDate, bool isActive = false)
    {
        SemesterNameInput.Clear();
        SemesterNameInput.SendKeys(name);
        
        SemesterCodeInput.Clear();
        SemesterCodeInput.SendKeys(code);
        
        StartDateInput.Clear();
        StartDateInput.SendKeys(startDate.ToString("MM/dd/yyyy"));
        
        EndDateInput.Clear();
        EndDateInput.SendKeys(endDate.ToString("MM/dd/yyyy"));

        RegistrationStartDateInput.Clear();
        RegistrationStartDateInput.SendKeys(regStartDate.ToString("MM/dd/yyyy"));

        RegistrationEndDateInput.Clear();
        RegistrationEndDateInput.SendKeys(regEndDate.ToString("MM/dd/yyyy"));

        if (isActive != IsActiveCheckbox.Selected)
        {
            IsActiveCheckbox.Click();
        }
    }

    public void SaveSemester()
    {
        SaveSemesterButton.Click();
        
        // Wait a moment to see if there's an error first
        Thread.Sleep(1000);
        
        // Check if there's an error message
        if (IsErrorDisplayed())
        {
            // If there's an error, don't wait for modal to close
            Console.WriteLine($"Error during save: {GetErrorMessage()}");
            return;
        }
        
        // Otherwise wait for modal to close
        WaitForModalToClose();
    }

    public void CancelCreate()
    {
        try
        {
            CancelSemesterButton.Click();
        }
        catch (OpenQA.Selenium.ElementClickInterceptedException)
        {
            // If normal click is intercepted, use JavaScript click
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", CancelSemesterButton);
        }
        WaitForModalToClose();
    }

    // Verification methods
    public bool IsOnSemestersPage()
    {
        return _driver.Url.Contains("/semesters") && 
               _driver.PageSource.ToLower().Contains("semester");
    }

    public bool CanSeeCreateButton()
    {
        try
        {
            return CreateSemesterButton.Displayed;
        }
        catch (NoSuchElementException)
        {
            try
            {
                return CreateFirstSemesterButton.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }

    public bool IsCreateFormVisible()
    {
        try
        {
            return SemesterModal.Displayed && SemesterNameInput.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public bool IsSemesterVisible(string semesterName)
    {
        var testId = $"semester-{semesterName.Replace(" ", "-").ToLower()}";
        try
        {
            var element = _driver.FindElement(By.CssSelector($"[data-testid='{testId}']"));
            return element.Displayed;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    public void DeleteSemester(string semesterName)
    {
        var testId = $"semester-{semesterName.Replace(" ", "-").ToLower()}";
        var semesterCard = _driver.FindElement(By.CssSelector($"[data-testid='{testId}']"));
        var semesterId = semesterCard.GetDomAttribute("id").Replace("semester-card-", "");
        var deleteButton = _driver.FindElement(By.Id($"delete-semester-{semesterId}"));
        deleteButton.Click();
        
        // Handle confirmation dialog
        var alert = _wait.Until(d => d.SwitchTo().Alert());
        alert.Accept();
    }

    public void EditSemester(string semesterName)
    {
        var testId = $"semester-{semesterName.Replace(" ", "-").ToLower()}";
        var semesterCard = _driver.FindElement(By.CssSelector($"[data-testid='{testId}']"));
        var semesterId = semesterCard.GetDomAttribute("id").Replace("semester-card-", "");
        var editButton = _driver.FindElement(By.Id($"edit-semester-{semesterId}"));
        editButton.Click();
        WaitForModalToOpen();
    }

    public string GetModalTitle()
    {
        return ModalTitle.Text;
    }

    public bool IsErrorDisplayed()
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
        return ErrorMessage.Text;
    }

    public string GetSuccessMessage()
    {
        try
        {
            var successElements = _driver.FindElements(By.CssSelector(".alert-success, .success, .toast-success, [data-testid='success-message']"));
            return successElements.FirstOrDefault()?.Text ?? "";
        }
        catch (NoSuchElementException)
        {
            return "";
        }
    }

    public int GetSemesterCount()
    {
        try
        {
            // Count semester cards using the data-testid attribute
            var semesterCards = _driver.FindElements(By.CssSelector("[data-testid^='semester-']"));
            return semesterCards.Count;
        }
        catch (NoSuchElementException)
        {
            return 0;
        }
    }

    public void WaitForModalToClose()
    {
        try
        {
            _wait.Until(d => {
                try
                {
                    var modal = d.FindElement(By.Id("semester-modal"));
                    return !modal.Displayed;
                }
                catch (NoSuchElementException)
                {
                    return true; // Modal is gone
                }
                catch (OpenQA.Selenium.StaleElementReferenceException)
                {
                    return true; // Modal element is stale, means it's been removed
                }
            });
        }
        catch (OpenQA.Selenium.WebDriverTimeoutException)
        {
            Console.WriteLine("Modal did not close within timeout period");
            // Take a screenshot or log page source for debugging
            Console.WriteLine($"Current URL: {_driver.Url}");
            Console.WriteLine($"Page contains modal: {_driver.PageSource.Contains("semester-modal")}");
            throw;
        }
    }

    // Helper methods
    private void WaitForPageLoad()
    {
        _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        Thread.Sleep(500); // Additional wait for dynamic content
    }
}
