using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Algora.Erp.E2E.Tests.PageObjects;

public class ModalPage
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;
    private readonly string _modalId;
    private readonly string _pageUrl;
    private readonly string _addButtonSelector;

    public ModalPage(IWebDriver driver, string pageUrl, string modalId, string addButtonSelector)
    {
        _driver = driver;
        _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        _pageUrl = pageUrl;
        _modalId = modalId;
        _addButtonSelector = addButtonSelector;
    }

    public IWebElement Modal => _driver.FindElement(By.Id(_modalId));
    public IWebElement AddButton => _driver.FindElement(By.CssSelector(_addButtonSelector));
    public IWebElement CloseButton => Modal.FindElement(By.CssSelector("button[onclick*='closeModal']"));
    public IWebElement CancelButton => Modal.FindElement(By.CssSelector("button[type='button'][onclick*='closeModal']"));
    public IWebElement SaveButton => Modal.FindElement(By.CssSelector("button[type='submit']"));

    public bool IsModalVisible()
    {
        try
        {
            return Modal.Displayed && !Modal.GetAttribute("class").Contains("hidden");
        }
        catch
        {
            return false;
        }
    }

    public void ClickAddButton()
    {
        var button = _wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(_addButtonSelector)));
        ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", button);
        Thread.Sleep(200);
        button.Click();
    }

    public void ClickCloseIcon()
    {
        var closeBtn = _wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector($"#{_modalId} button[onclick*='closeModal']")));
        closeBtn.Click();
    }

    public void ClickCancelButton()
    {
        var cancelBtn = Modal.FindElements(By.CssSelector("button[type='button']"))
            .FirstOrDefault(b => b.Text.Contains("Cancel") || b.GetAttribute("onclick")?.Contains("closeModal") == true);

        if (cancelBtn != null)
        {
            _wait.Until(ExpectedConditions.ElementToBeClickable(cancelBtn));
            cancelBtn.Click();
        }
    }

    public void WaitForModalVisible()
    {
        _wait.Until(d =>
        {
            try
            {
                var modal = d.FindElement(By.Id(_modalId));
                return modal.Displayed && !modal.GetAttribute("class").Contains("hidden");
            }
            catch
            {
                return false;
            }
        });
    }

    public void WaitForModalHidden()
    {
        _wait.Until(d =>
        {
            try
            {
                var modal = d.FindElement(By.Id(_modalId));
                return !modal.Displayed || modal.GetAttribute("class").Contains("hidden");
            }
            catch
            {
                return true;
            }
        });
    }
}
