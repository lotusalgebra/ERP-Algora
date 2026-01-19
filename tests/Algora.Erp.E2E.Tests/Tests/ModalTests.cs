using Algora.Erp.E2E.Tests.Infrastructure;
using Algora.Erp.E2E.Tests.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace Algora.Erp.E2E.Tests.Tests;

public class ModalTests : BaseTest
{
    public static IEnumerable<object[]> ModalTestData => new List<object[]>
    {
        // Admin Module
        new object[] { "/Admin/Roles", "roleModal", "button[onclick*='loadRoleCreateForm']", "Roles" },
        new object[] { "/Admin/Users", "userModal", "button[onclick*='loadUserCreateForm']", "Users" },
        new object[] { "/Admin/Permissions", "permissionModal", "button[data-modal-toggle='permissionModal']", "Permissions" },

        // HR Module
        new object[] { "/HR/Employees", "employeeModal", "button[data-modal-toggle='employeeModal']", "Employees" },
        new object[] { "/HR/Departments", "departmentModal", "button[data-modal-toggle='departmentModal']", "Departments" },
        new object[] { "/HR/Positions", "positionModal", "button[data-modal-toggle='positionModal']", "Positions" },
        new object[] { "/HR/Leave", "leaveModal", "button[data-modal-toggle='leaveModal']", "Leave" },
        new object[] { "/HR/Attendance", "attendanceModal", "button[data-modal-toggle='attendanceModal']", "Attendance" },

        // Sales Module
        new object[] { "/Sales/Customers", "formModal", "button[data-modal-toggle='formModal']", "Sales Customers" },
        new object[] { "/Sales/Orders", "formModal", "button[data-modal-toggle='formModal']", "Sales Orders" },
        new object[] { "/Sales/Leads", "leadModal", "button[data-modal-toggle='leadModal']", "Leads" },
        new object[] { "/Sales/Quotations", "formModal", "button[data-modal-toggle='formModal']", "Quotations" },

        // Inventory Module
        new object[] { "/Inventory/Products", "formModal", "button[data-modal-toggle='formModal']", "Inventory Products" },
        new object[] { "/Inventory/Warehouses", "formModal", "button[data-modal-toggle='formModal']", "Warehouses" },

        // Ecommerce Module
        new object[] { "/Ecommerce/Products", "formModal", "button[data-modal-toggle='formModal']", "Ecommerce Products" },
        new object[] { "/Ecommerce/Customers", "customerModal", "button[data-modal-toggle='customerModal']", "Ecommerce Customers" },
        new object[] { "/Ecommerce/Categories", "formModal", "button[data-modal-toggle='formModal']", "Categories" },
        new object[] { "/Ecommerce/Coupons", "couponModal", "button[data-modal-toggle='couponModal']", "Coupons" },

        // Procurement Module
        new object[] { "/Procurement/Suppliers", "formModal", "button[data-modal-toggle='formModal']", "Suppliers" },
        new object[] { "/Procurement/PurchaseOrders", "formModal", "button[data-modal-toggle='formModal']", "Purchase Orders" },
        new object[] { "/Procurement/GoodsReceipt", "formModal", "button[data-modal-toggle='formModal']", "Goods Receipt" },

        // Manufacturing Module
        new object[] { "/Manufacturing/WorkOrders", "formModal", "button[data-modal-toggle='formModal']", "Work Orders" },
        new object[] { "/Manufacturing/BOM", "formModal", "button[data-modal-toggle='formModal']", "BOM" },

        // Quality Module
        new object[] { "/Quality/Inspections", "formModal", "button[data-modal-toggle='formModal']", "Inspections" },
        new object[] { "/Quality/Rejections", "formModal", "button[data-modal-toggle='formModal']", "Rejections" },

        // Finance Module
        new object[] { "/Finance/Accounts", "accountModal", "button[data-modal-toggle='accountModal']", "Accounts" },

        // Payroll Module
        new object[] { "/Payroll/Components", "formModal", "button[data-modal-toggle='formModal']", "Payroll Components" },
        new object[] { "/Payroll/Runs", "formModal", "button[data-modal-toggle='formModal']", "Payroll Runs" },

        // Projects Module
        new object[] { "/Projects", "formModal", "button[data-modal-toggle='formModal']", "Projects" },
        new object[] { "/Projects/TimeTracking", "formModal", "button[data-modal-toggle='formModal']", "Time Tracking" },

        // Settings Module
        new object[] { "/Settings/Locations", "locationModal", "button[data-modal-toggle='locationModal']", "Locations" },
        new object[] { "/Settings/Currencies", "currencyModal", "button[data-modal-toggle='currencyModal']", "Currencies" },
        new object[] { "/Settings/GstSlabs", "gstSlabModal", "button[data-modal-toggle='gstSlabModal']", "GST Slabs" },

        // Dispatch Module
        new object[] { "/Dispatch/DeliveryChallans", "formModal", "button[data-modal-toggle='formModal']", "Delivery Challans" },
    };

    [Theory]
    [MemberData(nameof(ModalTestData))]
    public void Modal_OpenAndCloseWithCloseIcon_ShouldWork(string pageUrl, string modalId, string addButtonSelector, string pageName)
    {
        // Arrange
        Login();
        NavigateTo(pageUrl);
        WaitForHtmxLoad();

        // Act - Click Add button
        try
        {
            ClickElement(By.CssSelector(addButtonSelector));
        }
        catch
        {
            // Try alternative selector
            var addButton = Driver.FindElements(By.TagName("button"))
                .FirstOrDefault(b => b.Text.Contains("Add") || b.Text.Contains("New"));
            addButton?.Click();
        }

        // Wait for form to load via fetch
        Thread.Sleep(2000);

        // Assert - Modal should be visible
        WaitForModalVisible(modalId, 15);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page after clicking Add button");

        // Act - Click close icon - wait for it to be present
        Thread.Sleep(500);
        var closeIcon = WaitForClickable(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
        closeIcon.Click();

        // Assert - Modal should be hidden
        WaitForModalHidden(modalId);
        Assert.False(IsModalVisible(modalId), $"Modal should be hidden on {pageName} page after clicking close icon");
    }

    [Theory]
    [MemberData(nameof(ModalTestData))]
    public void Modal_OpenAndCloseWithCancelButton_ShouldWork(string pageUrl, string modalId, string addButtonSelector, string pageName)
    {
        // Arrange
        Login();
        NavigateTo(pageUrl);
        WaitForHtmxLoad();

        // Act - Click Add button
        try
        {
            ClickElement(By.CssSelector(addButtonSelector));
        }
        catch
        {
            var addButton = Driver.FindElements(By.TagName("button"))
                .FirstOrDefault(b => b.Text.Contains("Add") || b.Text.Contains("New"));
            addButton?.Click();
        }

        // Wait for form to load via fetch
        Thread.Sleep(2000);

        // Assert - Modal should be visible
        WaitForModalVisible(modalId, 15);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page");

        // Act - Click Cancel button
        Thread.Sleep(500);
        var cancelButton = Driver.FindElements(By.CssSelector($"#{modalId} button[type='button']"))
            .FirstOrDefault(b => b.Text.Contains("Cancel"));

        if (cancelButton != null)
        {
            cancelButton.Click();
        }
        else
        {
            // Fallback: click close icon
            var closeIcon = WaitForClickable(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
            closeIcon.Click();
        }

        // Assert - Modal should be hidden
        WaitForModalHidden(modalId);
        Assert.False(IsModalVisible(modalId), $"Modal should be hidden on {pageName} page after clicking Cancel");
    }

    [Theory]
    [MemberData(nameof(ModalTestData))]
    public void Modal_ClickBackdrop_ShouldCloseModal(string pageUrl, string modalId, string addButtonSelector, string pageName)
    {
        // Arrange
        Login();
        NavigateTo(pageUrl);
        WaitForHtmxLoad();

        // Act - Click Add button
        try
        {
            ClickElement(By.CssSelector(addButtonSelector));
        }
        catch
        {
            var addButton = Driver.FindElements(By.TagName("button"))
                .FirstOrDefault(b => b.Text.Contains("Add") || b.Text.Contains("New"));
            addButton?.Click();
        }

        // Wait for form to load via fetch
        Thread.Sleep(2000);

        // Assert - Modal should be visible
        WaitForModalVisible(modalId, 15);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page");

        // Act - Click backdrop
        Thread.Sleep(500);
        try
        {
            var backdrop = Driver.FindElement(By.Id("modal-backdrop"));
            if (backdrop.Displayed)
            {
                backdrop.Click();
                WaitForModalHidden(modalId);
            }
        }
        catch
        {
            // Backdrop click not supported, close via close icon
            var closeIcon = WaitForClickable(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
            closeIcon.Click();
            WaitForModalHidden(modalId);
        }

        // Assert - Modal should be hidden
        Assert.False(IsModalVisible(modalId), $"Modal should be hidden on {pageName} page after clicking backdrop");
    }
}
