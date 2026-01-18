using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Algora.Erp.E2E.Tests.Infrastructure;

public abstract class BaseTest : IDisposable
{
    protected IWebDriver Driver { get; private set; }
    protected WebDriverWait Wait { get; private set; }
    protected string BaseUrl => TestConfiguration.BaseUrl;
    private bool _isLoggedIn;

    protected BaseTest()
    {
        var options = new ChromeOptions();
        if (TestConfiguration.Headless)
        {
            options.AddArgument("--headless");
        }
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--window-size=1920,1080");

        Driver = new ChromeDriver(options);
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(TestConfiguration.DefaultTimeoutSeconds));
    }

    protected void Login()
    {
        if (_isLoggedIn) return;

        Driver.Navigate().GoToUrl($"{BaseUrl}/Account/Login");

        // ASP.NET generates IDs like Input_Email for asp-for="Input.Email"
        var emailField = Wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
        emailField.Clear();
        emailField.SendKeys(TestConfiguration.AdminEmail);

        var passwordField = Driver.FindElement(By.Id("password-input"));
        passwordField.Clear();
        passwordField.SendKeys(TestConfiguration.AdminPassword);

        var loginButton = Driver.FindElement(By.CssSelector("button[type='submit']"));
        loginButton.Click();

        // Wait for redirect to dashboard
        Wait.Until(d => !d.Url.Contains("/Account/Login"));
        _isLoggedIn = true;
    }

    protected void NavigateTo(string path)
    {
        Driver.Navigate().GoToUrl($"{BaseUrl}{path}");
        WaitForPageLoad();
    }

    protected void WaitForPageLoad()
    {
        Wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
    }

    protected void WaitForHtmxLoad()
    {
        Thread.Sleep(500); // Brief wait for HTMX to process
        Wait.Until(d => (bool)((IJavaScriptExecutor)d).ExecuteScript(
            "return typeof htmx === 'undefined' || htmx.findAll('.htmx-request').length === 0"));
    }

    protected IWebElement WaitForElement(By by, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(ExpectedConditions.ElementIsVisible(by));
    }

    protected IWebElement WaitForClickable(By by, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(ExpectedConditions.ElementToBeClickable(by));
    }

    protected bool IsModalVisible(string modalId)
    {
        try
        {
            var modal = Driver.FindElement(By.Id(modalId));
            return modal.Displayed && !modal.GetAttribute("class").Contains("hidden");
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    protected void WaitForModalVisible(string modalId, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d =>
        {
            try
            {
                var modal = d.FindElement(By.Id(modalId));
                return modal.Displayed && !modal.GetAttribute("class").Contains("hidden");
            }
            catch
            {
                return false;
            }
        });
    }

    protected void WaitForModalHidden(string modalId, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        wait.Until(d =>
        {
            try
            {
                var modal = d.FindElement(By.Id(modalId));
                return !modal.Displayed || modal.GetAttribute("class").Contains("hidden");
            }
            catch
            {
                return true;
            }
        });
    }

    protected void ClickElement(By by)
    {
        var element = WaitForClickable(by);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
        Thread.Sleep(200);
        element.Click();
    }

    protected void ExecuteScript(string script)
    {
        ((IJavaScriptExecutor)Driver).ExecuteScript(script);
    }

    public void Dispose()
    {
        Driver?.Quit();
        Driver?.Dispose();
    }
}
