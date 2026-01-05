using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Manufacturing;
using Algora.Erp.Domain.Entities.Payroll;
using Algora.Erp.Domain.Entities.Procurement;
using Algora.Erp.Domain.Entities.Projects;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Interface for the tenant-specific database context
/// </summary>
public interface IApplicationDbContext
{
    // Administration
    DbSet<User> Users { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<AuditLog> AuditLogs { get; }

    // HR
    DbSet<Employee> Employees { get; }
    DbSet<Department> Departments { get; }
    DbSet<Position> Positions { get; }
    DbSet<Attendance> Attendances { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<LeaveBalance> LeaveBalances { get; }

    // Finance
    DbSet<Account> Accounts { get; }
    DbSet<JournalEntry> JournalEntries { get; }
    DbSet<JournalEntryLine> JournalEntryLines { get; }
    DbSet<Invoice> Invoices { get; }
    DbSet<InvoiceLine> InvoiceLines { get; }
    DbSet<InvoicePayment> InvoicePayments { get; }
    DbSet<RecurringInvoice> RecurringInvoices { get; }
    DbSet<RecurringInvoiceLine> RecurringInvoiceLines { get; }

    // Inventory
    DbSet<Product> Products { get; }
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Warehouse> Warehouses { get; }
    DbSet<WarehouseLocation> WarehouseLocations { get; }
    DbSet<StockLevel> StockLevels { get; }
    DbSet<StockMovement> StockMovements { get; }

    // Procurement
    DbSet<Supplier> Suppliers { get; }
    DbSet<PurchaseOrder> PurchaseOrders { get; }
    DbSet<PurchaseOrderLine> PurchaseOrderLines { get; }

    // Sales
    DbSet<Customer> Customers { get; }
    DbSet<SalesOrder> SalesOrders { get; }
    DbSet<SalesOrderLine> SalesOrderLines { get; }

    // Payroll
    DbSet<SalaryComponent> SalaryComponents { get; }
    DbSet<SalaryStructure> SalaryStructures { get; }
    DbSet<SalaryStructureLine> SalaryStructureLines { get; }
    DbSet<PayrollRun> PayrollRuns { get; }
    DbSet<Payslip> Payslips { get; }
    DbSet<PayslipLine> PayslipLines { get; }

    // Manufacturing
    DbSet<BillOfMaterial> BillOfMaterials { get; }
    DbSet<BomLine> BomLines { get; }
    DbSet<WorkOrder> WorkOrders { get; }
    DbSet<WorkOrderOperation> WorkOrderOperations { get; }
    DbSet<WorkOrderMaterial> WorkOrderMaterials { get; }

    // Projects
    DbSet<Project> Projects { get; }
    DbSet<ProjectTask> ProjectTasks { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<TimeEntry> TimeEntries { get; }
    DbSet<ProjectMilestone> ProjectMilestones { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
