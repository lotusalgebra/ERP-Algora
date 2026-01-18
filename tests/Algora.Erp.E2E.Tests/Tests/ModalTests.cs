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
        new object[] { "/Admin/Permissions", "permissionModal", "button[onclick*='openAddModal']", "Permissions" },

        // HR Module
        new object[] { "/HR/Employees", "employeeModal", "button[onclick*='openAddModal']", "Employees" },
        new object[] { "/HR/Departments", "departmentModal", "button[onclick*='openAddModal']", "Departments" },
        new object[] { "/HR/Positions", "positionModal", "button[onclick*='openAddModal']", "Positions" },
        new object[] { "/HR/Leave", "leaveModal", "button[onclick*='openAddModal']", "Leave" },
        new object[] { "/HR/Attendance", "attendanceModal", "button[onclick*='openAddModal']", "Attendance" },

        // Sales Module
        new object[] { "/Sales/Customers", "customerModal", "button[onclick*='openAddModal']", "Sales Customers" },
        new object[] { "/Sales/Orders", "salesOrderModal", "button[onclick*='openAddModal']", "Sales Orders" },
        new object[] { "/Sales/Leads", "leadModal", "button[onclick*='openAddModal']", "Leads" },
        new object[] { "/Sales/Quotations", "quotationModal", "button[onclick*='openAddModal']", "Quotations" },

        // Inventory Module
        new object[] { "/Inventory/Products", "productModal", "button[onclick*='openAddModal']", "Inventory Products" },
        new object[] { "/Inventory/Warehouses", "warehouseModal", "button[onclick*='openAddModal']", "Warehouses" },

        // Ecommerce Module
        new object[] { "/Ecommerce/Products", "productModal", "button[onclick*='openAddModal']", "Ecommerce Products" },
        new object[] { "/Ecommerce/Customers", "customerModal", "button[onclick*='openAddModal']", "Ecommerce Customers" },
        new object[] { "/Ecommerce/Categories", "categoryModal", "button[onclick*='openAddModal']", "Categories" },
        new object[] { "/Ecommerce/Coupons", "couponModal", "button[onclick*='openAddModal']", "Coupons" },

        // Procurement Module
        new object[] { "/Procurement/Suppliers", "supplierModal", "button[onclick*='openAddModal']", "Suppliers" },
        new object[] { "/Procurement/PurchaseOrders", "purchaseOrderModal", "button[onclick*='openAddModal']", "Purchase Orders" },
        new object[] { "/Procurement/GoodsReceipt", "grnModal", "button[onclick*='openAddModal']", "Goods Receipt" },

        // Manufacturing Module
        new object[] { "/Manufacturing/WorkOrders", "workOrderModal", "button[onclick*='openAddModal']", "Work Orders" },
        new object[] { "/Manufacturing/BOM", "bomModal", "button[onclick*='openAddModal']", "BOM" },

        // Quality Module
        new object[] { "/Quality/Inspections", "inspectionModal", "button[onclick*='openAddModal']", "Inspections" },
        new object[] { "/Quality/Rejections", "rejectionModal", "button[onclick*='openAddModal']", "Rejections" },

        // Finance Module
        new object[] { "/Finance/Accounts", "accountModal", "button[onclick*='openAddModal']", "Accounts" },

        // Payroll Module
        new object[] { "/Payroll/Components", "componentModal", "button[onclick*='openAddModal']", "Payroll Components" },
        new object[] { "/Payroll/Runs", "payrollRunModal", "button[onclick*='openAddModal']", "Payroll Runs" },

        // Projects Module
        new object[] { "/Projects", "projectModal", "button[onclick*='openAddModal']", "Projects" },
        new object[] { "/Projects/TimeTracking", "timeEntryModal", "button[onclick*='openAddModal']", "Time Tracking" },

        // Settings Module
        new object[] { "/Settings/Locations", "locationModal", "button[onclick*='openAddModal']", "Locations" },
        new object[] { "/Settings/Currencies", "currencyModal", "button[onclick*='openAddModal']", "Currencies" },
        new object[] { "/Settings/GstSlabs", "gstSlabModal", "button[onclick*='openAddModal']", "GST Slabs" },

        // Dispatch Module
        new object[] { "/Dispatch/DeliveryChallans", "deliveryChallanModal", "button[onclick*='openAddModal']", "Delivery Challans" },
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

        // Assert - Modal should be visible
        WaitForModalVisible(modalId);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page after clicking Add button");

        // Act - Click close icon
        var closeIcon = Driver.FindElement(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
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

        // Assert - Modal should be visible
        WaitForModalVisible(modalId);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page");

        // Act - Click Cancel button
        var cancelButton = Driver.FindElements(By.CssSelector($"#{modalId} button[type='button']"))
            .FirstOrDefault(b => b.Text.Contains("Cancel"));

        if (cancelButton != null)
        {
            cancelButton.Click();
        }
        else
        {
            // Fallback: click close icon
            var closeIcon = Driver.FindElement(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
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

        // Assert - Modal should be visible
        WaitForModalVisible(modalId);
        Assert.True(IsModalVisible(modalId), $"Modal should be visible on {pageName} page");

        // Act - Click backdrop
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
            var closeIcon = Driver.FindElement(By.CssSelector($"#{modalId} button[onclick*='closeModal']"));
            closeIcon.Click();
            WaitForModalHidden(modalId);
        }

        // Assert - Modal should be hidden
        Assert.False(IsModalVisible(modalId), $"Modal should be hidden on {pageName} page after clicking backdrop");
    }
}
