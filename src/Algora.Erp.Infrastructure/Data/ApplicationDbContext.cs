using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Dispatch;
using Algora.Erp.Domain.Entities.Ecommerce;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Manufacturing;
using Algora.Erp.Domain.Entities.Payroll;
using Algora.Erp.Domain.Entities.Procurement;
using Algora.Erp.Domain.Entities.Projects;
using Algora.Erp.Domain.Entities.Quality;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Data;

/// <summary>
/// Tenant-specific database context
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    // Administration
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // HR
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    // Finance
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<InvoicePayment> InvoicePayments => Set<InvoicePayment>();
    public DbSet<RecurringInvoice> RecurringInvoices => Set<RecurringInvoice>();
    public DbSet<RecurringInvoiceLine> RecurringInvoiceLines => Set<RecurringInvoiceLine>();

    // Inventory
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<StockLevel> StockLevels => Set<StockLevel>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Procurement
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    // Dispatch
    public DbSet<DeliveryChallan> DeliveryChallans => Set<DeliveryChallan>();
    public DbSet<DeliveryChallanLine> DeliveryChallanLines => Set<DeliveryChallanLine>();

    // Quality
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<QualityParameter> QualityParameters => Set<QualityParameter>();
    public DbSet<RejectionNote> RejectionNotes => Set<RejectionNote>();

    // Common
    public DbSet<CancellationLog> CancellationLogs => Set<CancellationLog>();

    // Sales
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
    public DbSet<Lead> Leads => Set<Lead>();

    // Payroll
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryStructureLine> SalaryStructureLines => Set<SalaryStructureLine>();
    public DbSet<PayrollRun> PayrollRuns => Set<PayrollRun>();
    public DbSet<Payslip> Payslips => Set<Payslip>();
    public DbSet<PayslipLine> PayslipLines => Set<PayslipLine>();

    // Manufacturing
    public DbSet<BillOfMaterial> BillOfMaterials => Set<BillOfMaterial>();
    public DbSet<BomLine> BomLines => Set<BomLine>();
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderOperation> WorkOrderOperations => Set<WorkOrderOperation>();
    public DbSet<WorkOrderMaterial> WorkOrderMaterials => Set<WorkOrderMaterial>();

    // Projects
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectTask> ProjectTasks => Set<ProjectTask>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TimeEntry> TimeEntries => Set<TimeEntry>();
    public DbSet<ProjectMilestone> ProjectMilestones => Set<ProjectMilestone>();

    // Ecommerce
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<WebCategory> WebCategories => Set<WebCategory>();
    public DbSet<EcommerceProduct> EcommerceProducts => Set<EcommerceProduct>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<WebCustomer> WebCustomers => Set<WebCustomer>();
    public DbSet<CustomerAddress> CustomerAddresses => Set<CustomerAddress>();
    public DbSet<ShoppingCart> ShoppingCarts => Set<ShoppingCart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WebOrder> WebOrders => Set<WebOrder>();
    public DbSet<WebOrderItem> WebOrderItems => Set<WebOrderItem>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<ProductReview> ProductReviews => Set<ProductReview>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<ShippingMethod> ShippingMethods => Set<ShippingMethod>();
    public DbSet<WebPaymentMethod> WebPaymentMethods => Set<WebPaymentMethod>();
    public DbSet<Banner> Banners => Set<Banner>();

    // Settings
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<IndianState> IndianStates => Set<IndianState>();
    public DbSet<GstSlab> GstSlabs => Set<GstSlab>();
    public DbSet<OfficeLocation> OfficeLocations => Set<OfficeLocation>();

    // Tax Configuration (Multi-country support)
    public DbSet<TaxConfiguration> TaxConfigurations => Set<TaxConfiguration>();
    public DbSet<TaxSlab> TaxSlabs => Set<TaxSlab>();
    public DbSet<TaxRegion> TaxRegions => Set<TaxRegion>();

    // Tenant Settings
    public DbSet<TenantSettings> TenantSettings => Set<TenantSettings>();

    // Integration Settings
    public DbSet<IntegrationSettings> IntegrationSettings => Set<IntegrationSettings>();

    // CRM Integration Mappings
    public DbSet<CrmIntegrationMapping> CrmIntegrationMappings => Set<CrmIntegrationMapping>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUserService.UserId;
                    entry.Entity.ModifiedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is AuditableEntity auditableEntity)
                    {
                        entry.State = EntityState.Modified;
                        auditableEntity.IsDeleted = true;
                        auditableEntity.DeletedBy = _currentUserService.UserId;
                        auditableEntity.DeletedAt = _dateTime.UtcNow;
                    }
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filter for soft delete
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Role>().HasQueryFilter(e => !e.IsDeleted);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(100);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
        });

        // UserRole configuration
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.RoleId }).IsUnique();

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Permission configuration
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Module).HasMaxLength(100).IsRequired();
        });

        // RolePermission configuration
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.RoleId, e.PermissionId }).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(e => e.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AuditLog configuration
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId });
            entity.Property(e => e.EntityType).HasMaxLength(200).IsRequired();
            entity.Property(e => e.EntityId).HasMaxLength(50);
            entity.Property(e => e.UserEmail).HasMaxLength(255);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });

        // =============================================
        // HR ENTITIES
        // =============================================

        // Employee configuration
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employees");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.EmployeeCode).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.EmployeeCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.NationalId).HasMaxLength(50);
            entity.Property(e => e.PassportNumber).HasMaxLength(50);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.Avatar).HasMaxLength(500);
            entity.Property(e => e.SalaryCurrency).HasMaxLength(3);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
            entity.Property(e => e.EmergencyContactName).HasMaxLength(100);
            entity.Property(e => e.EmergencyContactPhone).HasMaxLength(50);
            entity.Property(e => e.EmergencyContactRelation).HasMaxLength(50);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Position)
                .WithMany(p => p.Employees)
                .HasForeignKey(e => e.PositionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Manager)
                .WithMany(m => m.DirectReports)
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Department configuration
        modelBuilder.Entity<Department>(entity =>
        {
            entity.ToTable("Departments");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.ParentDepartment)
                .WithMany(d => d.SubDepartments)
                .HasForeignKey(e => e.ParentDepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Manager)
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Position configuration
        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Positions");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.MinSalary).HasPrecision(18, 2);
            entity.Property(e => e.MaxSalary).HasPrecision(18, 2);

            entity.HasOne(e => e.Department)
                .WithMany()
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Attendance configuration
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("Attendances");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.EmployeeId, e.Date }).IsUnique();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.CheckInLocation).HasMaxLength(200);
            entity.Property(e => e.CheckOutLocation).HasMaxLength(200);
            entity.Property(e => e.CheckInIpAddress).HasMaxLength(50);
            entity.Property(e => e.CheckOutIpAddress).HasMaxLength(50);
            entity.Property(e => e.CheckInLatitude).HasPrecision(10, 7);
            entity.Property(e => e.CheckInLongitude).HasPrecision(10, 7);
            entity.Property(e => e.CheckOutLatitude).HasPrecision(10, 7);
            entity.Property(e => e.CheckOutLongitude).HasPrecision(10, 7);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LeaveRequest configuration
        modelBuilder.Entity<LeaveRequest>(entity =>
        {
            entity.ToTable("LeaveRequests");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.EmployeeId, e.StartDate, e.EndDate });
            entity.Property(e => e.Reason).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.EmergencyContact).HasMaxLength(100);
            entity.Property(e => e.HandoverNotes).HasMaxLength(1000);
            entity.Property(e => e.TotalDays).HasPrecision(5, 1);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Approver)
                .WithMany()
                .HasForeignKey(e => e.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // LeaveBalance configuration
        modelBuilder.Entity<LeaveBalance>(entity =>
        {
            entity.ToTable("LeaveBalances");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.EmployeeId, e.Year, e.LeaveType }).IsUnique();
            entity.Property(e => e.TotalEntitlement).HasPrecision(5, 1);
            entity.Property(e => e.Used).HasPrecision(5, 1);
            entity.Property(e => e.Pending).HasPrecision(5, 1);
            entity.Property(e => e.CarriedForward).HasPrecision(5, 1);
            entity.Property(e => e.Adjustments).HasPrecision(5, 1);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================
        // FINANCE ENTITIES
        // =============================================

        // Account configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.OpeningBalance).HasPrecision(18, 2);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);

            entity.HasOne(e => e.ParentAccount)
                .WithMany(a => a.ChildAccounts)
                .HasForeignKey(e => e.ParentAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntries");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.EntryNumber).IsUnique();
            entity.HasIndex(e => e.EntryDate);
            entity.Property(e => e.EntryNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.TotalDebit).HasPrecision(18, 2);
            entity.Property(e => e.TotalCredit).HasPrecision(18, 2);
        });

        // JournalEntryLine configuration
        modelBuilder.Entity<JournalEntryLine>(entity =>
        {
            entity.ToTable("JournalEntryLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DebitAmount).HasPrecision(18, 2);
            entity.Property(e => e.CreditAmount).HasPrecision(18, 2);
            entity.Property(e => e.SourceDocumentType).HasMaxLength(100);

            entity.HasOne(e => e.JournalEntry)
                .WithMany(j => j.Lines)
                .HasForeignKey(e => e.JournalEntryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Account)
                .WithMany(a => a.JournalEntryLines)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.ToTable("Invoices");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.InvoiceDate);
            entity.HasIndex(e => e.DueDate);
            entity.Property(e => e.InvoiceNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.PaymentTerms).HasMaxLength(100);
            entity.Property(e => e.BillingName).HasMaxLength(200);
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(100);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entity.Property(e => e.BillingCountry).HasMaxLength(100);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasMaxLength(1000);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceDue).HasPrecision(18, 2);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.SalesOrder)
                .WithMany()
                .HasForeignKey(e => e.SalesOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.RecurringInvoice)
                .WithMany(r => r.GeneratedInvoices)
                .HasForeignKey(e => e.RecurringInvoiceId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // InvoiceLine configuration
        modelBuilder.Entity<InvoiceLine>(entity =>
        {
            entity.ToTable("InvoiceLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // InvoicePayment configuration
        modelBuilder.Entity<InvoicePayment>(entity =>
        {
            entity.ToTable("InvoicePayments");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.PaymentDate);
            entity.Property(e => e.PaymentNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RecurringInvoice configuration
        modelBuilder.Entity<RecurringInvoice>(entity =>
        {
            entity.ToTable("RecurringInvoices");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.NextGenerationDate);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.PaymentTerms).HasMaxLength(50);
            entity.Property(e => e.BillingName).HasMaxLength(200);
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(100);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entity.Property(e => e.BillingCountry).HasMaxLength(100);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasMaxLength(1000);
            entity.Property(e => e.EmailRecipients).HasMaxLength(500);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RecurringInvoiceLine configuration
        modelBuilder.Entity<RecurringInvoiceLine>(entity =>
        {
            entity.ToTable("RecurringInvoiceLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.RecurringInvoiceId);
            entity.Property(e => e.ProductCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);

            entity.HasOne(e => e.RecurringInvoice)
                .WithMany(r => r.Lines)
                .HasForeignKey(e => e.RecurringInvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================
        // INVENTORY ENTITIES
        // =============================================

        // ProductCategory configuration
        modelBuilder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);

            entity.HasOne(e => e.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(e => e.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Sku).IsUnique();
            entity.HasIndex(e => e.Barcode);
            entity.Property(e => e.Sku).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Barcode).HasMaxLength(100);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.SellingPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.Weight).HasPrecision(10, 3);
            entity.Property(e => e.Length).HasPrecision(10, 3);
            entity.Property(e => e.Width).HasPrecision(10, 3);
            entity.Property(e => e.Height).HasPrecision(10, 3);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Warehouse configuration
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.ToTable("Warehouses");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.ManagerName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Email).HasMaxLength(255);
        });

        // WarehouseLocation configuration
        modelBuilder.Entity<WarehouseLocation>(entity =>
        {
            entity.ToTable("WarehouseLocations");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.WarehouseId, e.Code }).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Zone).HasMaxLength(50);
            entity.Property(e => e.Aisle).HasMaxLength(50);
            entity.Property(e => e.Rack).HasMaxLength(50);
            entity.Property(e => e.Shelf).HasMaxLength(50);
            entity.Property(e => e.Bin).HasMaxLength(50);

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.Locations)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // StockLevel configuration
        modelBuilder.Entity<StockLevel>(entity =>
        {
            entity.ToTable("StockLevels");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.ProductId, e.WarehouseId, e.LocationId }).IsUnique();
            entity.Property(e => e.QuantityOnHand).HasPrecision(18, 4);
            entity.Property(e => e.QuantityReserved).HasPrecision(18, 4);
            entity.Property(e => e.QuantityOnOrder).HasPrecision(18, 4);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockLevels)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Warehouse)
                .WithMany(w => w.StockLevels)
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Location)
                .WithMany(l => l.StockLevels)
                .HasForeignKey(e => e.LocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // StockMovement configuration
        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("StockMovements");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.MovementDate);
            entity.HasIndex(e => new { e.SourceDocumentType, e.SourceDocumentId });
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.SourceDocumentType).HasMaxLength(50);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.StockMovements)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.FromLocation)
                .WithMany()
                .HasForeignKey(e => e.FromLocationId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ToLocation)
                .WithMany()
                .HasForeignKey(e => e.ToLocationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =============================================
        // PROCUREMENT ENTITIES
        // =============================================

        // Supplier configuration
        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("Suppliers");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Fax).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankRoutingNumber).HasMaxLength(50);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.MinimumOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // PurchaseOrder configuration
        modelBuilder.Entity<PurchaseOrder>(entity =>
        {
            entity.ToTable("PurchaseOrders");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.OrderDate);
            entity.Property(e => e.OrderNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingMethod).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);

            entity.HasOne(e => e.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PurchaseOrderLine configuration
        modelBuilder.Entity<PurchaseOrderLine>(entity =>
        {
            entity.ToTable("PurchaseOrderLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.QuantityReceived).HasPrecision(18, 4);
            entity.Property(e => e.QuantityReturned).HasPrecision(18, 4);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany(p => p.Lines)
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // GoodsReceiptNote configuration
        modelBuilder.Entity<GoodsReceiptNote>(entity =>
        {
            entity.ToTable("GoodsReceiptNotes");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.GrnNumber).IsUnique();
            entity.HasIndex(e => e.GrnDate);
            entity.Property(e => e.GrnNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SupplierInvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.VehicleNumber).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.TransporterName).HasMaxLength(200);
            entity.Property(e => e.LrNumber).HasMaxLength(50);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);

            entity.HasOne(e => e.PurchaseOrder)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // GoodsReceiptLine configuration
        modelBuilder.Entity<GoodsReceiptLine>(entity =>
        {
            entity.ToTable("GoodsReceiptLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.SerialNumbers).HasMaxLength(1000);
            entity.Property(e => e.RejectionReason).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.OrderedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.ReceivedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.AcceptedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.RejectedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.GoodsReceiptNote)
                .WithMany(g => g.Lines)
                .HasForeignKey(e => e.GoodsReceiptNoteId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.PurchaseOrderLine)
                .WithMany()
                .HasForeignKey(e => e.PurchaseOrderLineId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =============================================
        // DISPATCH ENTITIES
        // =============================================

        // DeliveryChallan configuration
        modelBuilder.Entity<DeliveryChallan>(entity =>
        {
            entity.ToTable("DeliveryChallans");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ChallanNumber).IsUnique();
            entity.HasIndex(e => e.ChallanDate);
            entity.Property(e => e.ChallanNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.VehicleNumber).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(100);
            entity.Property(e => e.DriverPhone).HasMaxLength(50);
            entity.Property(e => e.TransporterName).HasMaxLength(200);
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingState).HasMaxLength(100);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(20);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.SalesOrder)
                .WithMany()
                .HasForeignKey(e => e.SalesOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DeliveryChallanLine configuration
        modelBuilder.Entity<DeliveryChallanLine>(entity =>
        {
            entity.ToTable("DeliveryChallanLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.BatchNumber).HasMaxLength(50);
            entity.Property(e => e.SerialNumbers).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);

            entity.HasOne(e => e.DeliveryChallan)
                .WithMany(d => d.Lines)
                .HasForeignKey(e => e.DeliveryChallanId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =============================================
        // QUALITY ENTITIES
        // =============================================

        // QualityInspection configuration
        modelBuilder.Entity<QualityInspection>(entity =>
        {
            entity.ToTable("QualityInspections");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.InspectionNumber).IsUnique();
            entity.HasIndex(e => e.InspectionDate);
            entity.HasIndex(e => new { e.SourceDocumentType, e.SourceDocumentId });
            entity.Property(e => e.InspectionNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SourceDocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceDocumentNumber).HasMaxLength(50);
            entity.Property(e => e.InspectorName).HasMaxLength(100);
            entity.Property(e => e.ApproverName).HasMaxLength(100);
            entity.Property(e => e.ResultRemarks).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.TotalQuantity).HasPrecision(18, 4);
            entity.Property(e => e.SampleSize).HasPrecision(18, 4);
            entity.Property(e => e.InspectedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.PassedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.FailedQuantity).HasPrecision(18, 4);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // QualityParameter configuration
        modelBuilder.Entity<QualityParameter>(entity =>
        {
            entity.ToTable("QualityParameters");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ParameterName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ParameterCode).HasMaxLength(20);
            entity.Property(e => e.ExpectedValue).HasMaxLength(200);
            entity.Property(e => e.ActualValue).HasMaxLength(200);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Remarks).HasMaxLength(500);
            entity.Property(e => e.AttachmentUrl).HasMaxLength(500);
            entity.Property(e => e.MinValue).HasPrecision(18, 4);
            entity.Property(e => e.MaxValue).HasPrecision(18, 4);
            entity.Property(e => e.Tolerance).HasPrecision(18, 4);
            entity.Property(e => e.MeasuredValue).HasPrecision(18, 4);

            entity.HasOne(e => e.QualityInspection)
                .WithMany(q => q.Parameters)
                .HasForeignKey(e => e.QualityInspectionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RejectionNote configuration
        modelBuilder.Entity<RejectionNote>(entity =>
        {
            entity.ToTable("RejectionNotes");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.RejectionNumber).IsUnique();
            entity.HasIndex(e => e.RejectionDate);
            entity.HasIndex(e => new { e.SourceDocumentType, e.SourceDocumentId });
            entity.Property(e => e.RejectionNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SourceDocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SourceDocumentNumber).HasMaxLength(50);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.RejectionReason).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DefectDescription).HasMaxLength(1000);
            entity.Property(e => e.DispositionAction).HasMaxLength(200);
            entity.Property(e => e.DisposerName).HasMaxLength(100);
            entity.Property(e => e.DebitNoteNumber).HasMaxLength(50);
            entity.Property(e => e.ReturnReference).HasMaxLength(100);
            entity.Property(e => e.ScrapReference).HasMaxLength(100);
            entity.Property(e => e.ReworkInstructions).HasMaxLength(1000);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.AttachmentUrls).HasMaxLength(2000);
            entity.Property(e => e.RejectedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalValue).HasPrecision(18, 2);
            entity.Property(e => e.ScrapValue).HasPrecision(18, 2);
            entity.Property(e => e.ReworkCost).HasPrecision(18, 2);

            entity.HasOne(e => e.QualityInspection)
                .WithMany()
                .HasForeignKey(e => e.QualityInspectionId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Supplier)
                .WithMany()
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =============================================
        // COMMON ENTITIES
        // =============================================

        // CancellationLog configuration
        modelBuilder.Entity<CancellationLog>(entity =>
        {
            entity.ToTable("CancellationLogs");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.DocumentType, e.DocumentId });
            entity.HasIndex(e => e.CancelledAt);
            entity.Property(e => e.DocumentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DocumentNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CancelledByName).HasMaxLength(100);
            entity.Property(e => e.CancellationReason).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.ApprovalReference).HasMaxLength(100);
        });

        // =============================================
        // SALES ENTITIES
        // =============================================

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("Customers");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Mobile).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.BillingAddress).HasMaxLength(500);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(100);
            entity.Property(e => e.BillingCountry).HasMaxLength(100);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingState).HasMaxLength(100);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(20);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.CurrentBalance).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
        });

        // Lead configuration
        modelBuilder.Entity<Lead>(entity =>
        {
            entity.ToTable("Leads");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Email);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Website).HasMaxLength(500);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.AssignedToName).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.EstimatedValue).HasPrecision(18, 2);
        });

        // SalesOrder configuration
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.ToTable("SalesOrders");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.OrderDate);
            entity.Property(e => e.OrderNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ShippingAddress).HasMaxLength(500);
            entity.Property(e => e.ShippingMethod).HasMaxLength(100);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Reference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.SalesOrders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // SalesOrderLine configuration
        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.ToTable("SalesOrderLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ProductName).HasMaxLength(200);
            entity.Property(e => e.ProductSku).HasMaxLength(50);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxPercent).HasPrecision(5, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.Property(e => e.QuantityShipped).HasPrecision(18, 4);
            entity.Property(e => e.QuantityReturned).HasPrecision(18, 4);

            entity.HasOne(e => e.SalesOrder)
                .WithMany(s => s.Lines)
                .HasForeignKey(e => e.SalesOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================
        // PAYROLL ENTITIES
        // =============================================

        // SalaryComponent configuration
        modelBuilder.Entity<SalaryComponent>(entity =>
        {
            entity.ToTable("SalaryComponents");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DefaultValue).HasPrecision(18, 2);
            entity.Property(e => e.MinValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxValue).HasPrecision(18, 2);
        });

        // SalaryStructure configuration
        modelBuilder.Entity<SalaryStructure>(entity =>
        {
            entity.ToTable("SalaryStructures");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.BaseSalary).HasPrecision(18, 2);
        });

        // SalaryStructureLine configuration
        modelBuilder.Entity<SalaryStructureLine>(entity =>
        {
            entity.ToTable("SalaryStructureLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.SalaryStructureId, e.SalaryComponentId }).IsUnique();
            entity.Property(e => e.Value).HasPrecision(18, 4);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.SalaryStructure)
                .WithMany(s => s.Lines)
                .HasForeignKey(e => e.SalaryStructureId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Component)
                .WithMany(c => c.SalaryStructureLines)
                .HasForeignKey(e => e.SalaryComponentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayrollRun configuration
        modelBuilder.Entity<PayrollRun>(entity =>
        {
            entity.ToTable("PayrollRuns");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.RunNumber).IsUnique();
            entity.HasIndex(e => new { e.PeriodStart, e.PeriodEnd });
            entity.Property(e => e.RunNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.TotalGrossPay).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            entity.Property(e => e.TotalNetPay).HasPrecision(18, 2);
            entity.Property(e => e.TotalEmployerCosts).HasPrecision(18, 2);
        });

        // Payslip configuration
        modelBuilder.Entity<Payslip>(entity =>
        {
            entity.ToTable("Payslips");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.PayslipNumber).IsUnique();
            entity.HasIndex(e => new { e.PayrollRunId, e.EmployeeId }).IsUnique();
            entity.Property(e => e.PayslipNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.BankAccountNumber).HasMaxLength(50);
            entity.Property(e => e.BankName).HasMaxLength(100);
            entity.Property(e => e.TransactionReference).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.WorkingDays).HasPrecision(5, 1);
            entity.Property(e => e.DaysWorked).HasPrecision(5, 1);
            entity.Property(e => e.LeavesTaken).HasPrecision(5, 1);
            entity.Property(e => e.Overtime).HasPrecision(5, 1);
            entity.Property(e => e.BasicSalary).HasPrecision(18, 2);
            entity.Property(e => e.GrossPay).HasPrecision(18, 2);
            entity.Property(e => e.TotalEarnings).HasPrecision(18, 2);
            entity.Property(e => e.TotalDeductions).HasPrecision(18, 2);
            entity.Property(e => e.TaxableAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.NetPay).HasPrecision(18, 2);

            entity.HasOne(e => e.PayrollRun)
                .WithMany(p => p.Payslips)
                .HasForeignKey(e => e.PayrollRunId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // PayslipLine configuration
        modelBuilder.Entity<PayslipLine>(entity =>
        {
            entity.ToTable("PayslipLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.ComponentCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.ComponentName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.Value).HasPrecision(18, 4);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.Payslip)
                .WithMany(p => p.Lines)
                .HasForeignKey(e => e.PayslipId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.SalaryComponent)
                .WithMany()
                .HasForeignKey(e => e.SalaryComponentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // =============================================
        // MANUFACTURING ENTITIES
        // =============================================

        // BillOfMaterial configuration
        modelBuilder.Entity<BillOfMaterial>(entity =>
        {
            entity.ToTable("BillOfMaterials");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.BomNumber).IsUnique();
            entity.Property(e => e.BomNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // BomLine configuration
        modelBuilder.Entity<BomLine>(entity =>
        {
            entity.ToTable("BomLines");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.Property(e => e.TotalCost).HasPrecision(18, 2);
            entity.Property(e => e.WastagePercent).HasPrecision(5, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.BillOfMaterial)
                .WithMany(b => b.Lines)
                .HasForeignKey(e => e.BillOfMaterialId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // WorkOrder configuration
        modelBuilder.Entity<WorkOrder>(entity =>
        {
            entity.ToTable("WorkOrders");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.WorkOrderNumber).IsUnique();
            entity.HasIndex(e => e.PlannedStartDate);
            entity.Property(e => e.WorkOrderNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.PlannedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.CompletedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.ScrapQuantity).HasPrecision(18, 4);
            entity.Property(e => e.EstimatedCost).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.BillOfMaterial)
                .WithMany()
                .HasForeignKey(e => e.BillOfMaterialId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Warehouse)
                .WithMany()
                .HasForeignKey(e => e.WarehouseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WorkOrderOperation configuration
        modelBuilder.Entity<WorkOrderOperation>(entity =>
        {
            entity.ToTable("WorkOrderOperations");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Workstation).HasMaxLength(100);
            entity.Property(e => e.PlannedHours).HasPrecision(10, 2);
            entity.Property(e => e.ActualHours).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.WorkOrder)
                .WithMany(w => w.Operations)
                .HasForeignKey(e => e.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WorkOrderMaterial configuration
        modelBuilder.Entity<WorkOrderMaterial>(entity =>
        {
            entity.ToTable("WorkOrderMaterials");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.UnitOfMeasure).HasMaxLength(20);
            entity.Property(e => e.RequiredQuantity).HasPrecision(18, 4);
            entity.Property(e => e.IssuedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.ReturnedQuantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitCost).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.WorkOrder)
                .WithMany(w => w.Materials)
                .HasForeignKey(e => e.WorkOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =============================================
        // PROJECT ENTITIES
        // =============================================

        // Project configuration
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ProjectCode).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.ProjectCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.BudgetAmount).HasPrecision(18, 2);
            entity.Property(e => e.ActualCost).HasPrecision(18, 2);
            entity.Property(e => e.Progress).HasPrecision(5, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ProjectManager)
                .WithMany()
                .HasForeignKey(e => e.ProjectManagerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProjectTask configuration
        modelBuilder.Entity<ProjectTask>(entity =>
        {
            entity.ToTable("ProjectTasks");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.AssigneeId);
            entity.Property(e => e.TaskNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.EstimatedHours).HasPrecision(10, 2);
            entity.Property(e => e.ActualHours).HasPrecision(10, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Tasks)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ParentTask)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(e => e.ParentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Assignee)
                .WithMany()
                .HasForeignKey(e => e.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProjectMember configuration
        modelBuilder.Entity<ProjectMember>(entity =>
        {
            entity.ToTable("ProjectMembers");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => new { e.ProjectId, e.EmployeeId }).IsUnique();
            entity.Property(e => e.HourlyRate).HasPrecision(18, 2);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TimeEntry configuration
        modelBuilder.Entity<TimeEntry>(entity =>
        {
            entity.ToTable("TimeEntries");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ProjectId);
            entity.HasIndex(e => e.EmployeeId);
            entity.HasIndex(e => e.Date);
            entity.Property(e => e.Hours).HasPrecision(5, 2);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.TimeEntries)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Task)
                .WithMany(t => t.TimeEntries)
                .HasForeignKey(e => e.TaskId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Employee)
                .WithMany()
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProjectMilestone configuration
        modelBuilder.Entity<ProjectMilestone>(entity =>
        {
            entity.ToTable("ProjectMilestones");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ProjectId);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(e => e.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =============================================
        // ECOMMERCE ENTITIES
        // =============================================

        // Store configuration
        modelBuilder.Entity<Store>(entity =>
        {
            entity.ToTable("Stores");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Tagline).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.FaviconUrl).HasMaxLength(500);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.FacebookUrl).HasMaxLength(500);
            entity.Property(e => e.TwitterUrl).HasMaxLength(500);
            entity.Property(e => e.InstagramUrl).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(3);
            entity.Property(e => e.CurrencySymbol).HasMaxLength(10);
            entity.Property(e => e.TaxRate).HasPrecision(5, 2);
            entity.Property(e => e.MetaTitle).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.Property(e => e.MetaKeywords).HasMaxLength(500);
        });

        // WebCategory configuration
        modelBuilder.Entity<WebCategory>(entity =>
        {
            entity.ToTable("WebCategories");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.MetaTitle).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);

            entity.HasOne(e => e.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // EcommerceProduct configuration
        modelBuilder.Entity<EcommerceProduct>(entity =>
        {
            entity.ToTable("EcommerceProducts");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.Sku);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.ShortDescription).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(10000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.CompareAtPrice).HasPrecision(18, 2);
            entity.Property(e => e.CostPrice).HasPrecision(18, 2);
            entity.Property(e => e.Brand).HasMaxLength(100);
            entity.Property(e => e.Vendor).HasMaxLength(100);
            entity.Property(e => e.Weight).HasPrecision(10, 3);
            entity.Property(e => e.WeightUnit).HasMaxLength(10);
            entity.Property(e => e.MetaTitle).HasMaxLength(200);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.Property(e => e.Tags).HasMaxLength(500);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.InventoryProduct)
                .WithMany()
                .HasForeignKey(e => e.InventoryProductId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ProductImage configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("ProductImages");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId);
            entity.Property(e => e.Url).HasMaxLength(500).IsRequired();
            entity.Property(e => e.AltText).HasMaxLength(200);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ProductVariant configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.ToTable("ProductVariants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.Sku);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Option1Name).HasMaxLength(50);
            entity.Property(e => e.Option1Value).HasMaxLength(100);
            entity.Property(e => e.Option2Name).HasMaxLength(50);
            entity.Property(e => e.Option2Value).HasMaxLength(100);
            entity.Property(e => e.Option3Name).HasMaxLength(50);
            entity.Property(e => e.Option3Value).HasMaxLength(100);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.CompareAtPrice).HasPrecision(18, 2);
            entity.Property(e => e.Weight).HasPrecision(10, 3);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // WebCustomer configuration
        modelBuilder.Entity<WebCustomer>(entity =>
        {
            entity.ToTable("WebCustomers");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.TotalSpent).HasPrecision(18, 2);
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // CustomerAddress configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.ToTable("CustomerAddresses");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CustomerId);
            entity.Property(e => e.Label).HasMaxLength(50);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Company).HasMaxLength(200);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address1).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Address2).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PostalCode).HasMaxLength(20);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Addresses)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ShoppingCart configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<ShoppingCart>(entity =>
        {
            entity.ToTable("ShoppingCarts");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.SessionId);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // CartItem configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CartId);
            entity.Property(e => e.ProductName).HasMaxLength(300);
            entity.Property(e => e.VariantName).HasMaxLength(200);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(e => e.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Variant)
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WebOrder configuration
        modelBuilder.Entity<WebOrder>(entity =>
        {
            entity.ToTable("WebOrders");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.Subtotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.ShippingAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.Total).HasPrecision(18, 2);
            entity.Property(e => e.RefundedAmount).HasPrecision(18, 2);
            entity.Property(e => e.BillingFirstName).HasMaxLength(100);
            entity.Property(e => e.BillingLastName).HasMaxLength(100);
            entity.Property(e => e.BillingCompany).HasMaxLength(200);
            entity.Property(e => e.BillingAddress1).HasMaxLength(500);
            entity.Property(e => e.BillingAddress2).HasMaxLength(500);
            entity.Property(e => e.BillingCity).HasMaxLength(100);
            entity.Property(e => e.BillingState).HasMaxLength(100);
            entity.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entity.Property(e => e.BillingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingFirstName).HasMaxLength(100);
            entity.Property(e => e.ShippingLastName).HasMaxLength(100);
            entity.Property(e => e.ShippingCompany).HasMaxLength(200);
            entity.Property(e => e.ShippingAddress1).HasMaxLength(500);
            entity.Property(e => e.ShippingAddress2).HasMaxLength(500);
            entity.Property(e => e.ShippingCity).HasMaxLength(100);
            entity.Property(e => e.ShippingState).HasMaxLength(100);
            entity.Property(e => e.ShippingPostalCode).HasMaxLength(20);
            entity.Property(e => e.ShippingCountry).HasMaxLength(100);
            entity.Property(e => e.ShippingMethod).HasMaxLength(100);
            entity.Property(e => e.ShippingCarrier).HasMaxLength(100);
            entity.Property(e => e.PaymentMethod).HasMaxLength(100);
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(100);
            entity.Property(e => e.TrackingNumber).HasMaxLength(100);
            entity.Property(e => e.TrackingUrl).HasMaxLength(500);
            entity.Property(e => e.CustomerNotes).HasMaxLength(1000);
            entity.Property(e => e.InternalNotes).HasMaxLength(1000);
            entity.Property(e => e.CouponCode).HasMaxLength(50);
            entity.Property(e => e.RefundReason).HasMaxLength(1000);
            entity.Property(e => e.IpAddress).HasMaxLength(50);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Source).HasMaxLength(50);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WebOrderItem configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<WebOrderItem>(entity =>
        {
            entity.ToTable("WebOrderItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.Property(e => e.ProductName).HasMaxLength(300).IsRequired();
            entity.Property(e => e.VariantName).HasMaxLength(200);
            entity.Property(e => e.Sku).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);

            entity.HasOne(e => e.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Variant)
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Coupon configuration
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.ToTable("Coupons");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountValue).HasPrecision(18, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(18, 2);
            entity.Property(e => e.ApplicableProductIds).HasMaxLength(2000);
            entity.Property(e => e.ApplicableCategoryIds).HasMaxLength(2000);
            entity.Property(e => e.ExcludedProductIds).HasMaxLength(2000);
            entity.Property(e => e.CustomerIds).HasMaxLength(2000);
        });

        // ProductReview configuration
        modelBuilder.Entity<ProductReview>(entity =>
        {
            entity.ToTable("ProductReviews");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.ProductId);
            entity.HasIndex(e => e.CustomerId);
            entity.HasIndex(e => e.Status);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Content).HasMaxLength(5000);
            entity.Property(e => e.Pros).HasMaxLength(1000);
            entity.Property(e => e.Cons).HasMaxLength(1000);
            entity.Property(e => e.CustomerName).HasMaxLength(100);
            entity.Property(e => e.CustomerEmail).HasMaxLength(255);
            entity.Property(e => e.ImageUrls).HasMaxLength(2000);
            entity.Property(e => e.AdminResponse).HasMaxLength(2000);

            entity.HasOne(e => e.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Customer)
                .WithMany()
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // WishlistItem configuration (BaseEntity - no IsDeleted)
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.ToTable("WishlistItems");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.CustomerId, e.ProductId }).IsUnique();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Customer)
                .WithMany(c => c.Wishlist)
                .HasForeignKey(e => e.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Product)
                .WithMany()
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Variant)
                .WithMany()
                .HasForeignKey(e => e.VariantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ShippingMethod configuration
        modelBuilder.Entity<ShippingMethod>(entity =>
        {
            entity.ToTable("ShippingMethods");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Carrier).HasMaxLength(50);
            entity.Property(e => e.Rate).HasPrecision(18, 2);
            entity.Property(e => e.FreeShippingThreshold).HasPrecision(18, 2);
            entity.Property(e => e.RatePerKg).HasPrecision(18, 2);
            entity.Property(e => e.MinWeight).HasPrecision(10, 3);
            entity.Property(e => e.MaxWeight).HasPrecision(10, 3);
            entity.Property(e => e.AllowedCountries).HasMaxLength(1000);
            entity.Property(e => e.ExcludedCountries).HasMaxLength(1000);
        });

        // WebPaymentMethod configuration
        modelBuilder.Entity<WebPaymentMethod>(entity =>
        {
            entity.ToTable("WebPaymentMethods");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Instructions).HasMaxLength(2000);
            entity.Property(e => e.TransactionFeePercent).HasPrecision(5, 2);
            entity.Property(e => e.TransactionFeeFixed).HasPrecision(18, 2);
            entity.Property(e => e.MinAmount).HasPrecision(18, 2);
            entity.Property(e => e.MaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.SecretKey).HasMaxLength(500);
            entity.Property(e => e.WebhookSecret).HasMaxLength(500);
            entity.Property(e => e.AllowedCountries).HasMaxLength(1000);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
        });

        // Banner configuration
        modelBuilder.Entity<Banner>(entity =>
        {
            entity.ToTable("Banners");
            entity.HasKey(e => e.Id);
            entity.HasQueryFilter(e => !e.IsDeleted);
            entity.HasIndex(e => e.Position);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subtitle).HasMaxLength(500);
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.Property(e => e.MobileImageUrl).HasMaxLength(500);
            entity.Property(e => e.LinkUrl).HasMaxLength(500);
            entity.Property(e => e.ButtonText).HasMaxLength(50);
        });
    }
}
