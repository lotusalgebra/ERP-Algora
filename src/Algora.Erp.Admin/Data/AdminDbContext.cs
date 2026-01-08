using Algora.Erp.Admin.Entities;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Data;

public class AdminDbContext : DbContext
{
    public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options)
    {
    }

    // Authentication
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<AdminRole> AdminRoles => Set<AdminRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Tenants
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    // Billing
    public DbSet<BillingPlan> BillingPlans => Set<BillingPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();
    public DbSet<TenantBillingInvoice> TenantBillingInvoices => Set<TenantBillingInvoice>();
    public DbSet<TenantBillingInvoiceLine> TenantBillingInvoiceLines => Set<TenantBillingInvoiceLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===========================================
        // AdminUser Configuration
        // ===========================================
        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.ToTable("AdminUsers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.AvatarUrl).HasMaxLength(500);
            entity.Property(e => e.SecurityStamp).HasMaxLength(100);
            entity.Property(e => e.TwoFactorSecret).HasMaxLength(200);
            entity.Property(e => e.PasswordResetToken).HasMaxLength(500);
            entity.Property(e => e.EmailConfirmationToken).HasMaxLength(500);
            entity.Property(e => e.LastLoginIp).HasMaxLength(50);

            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(e => e.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===========================================
        // AdminRole Configuration
        // ===========================================
        modelBuilder.Entity<AdminRole>(entity =>
        {
            entity.ToTable("AdminRoles");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Permissions).HasMaxLength(4000);

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ===========================================
        // RefreshToken Configuration
        // ===========================================
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.Property(e => e.CreatedByIp).HasMaxLength(50);
            entity.Property(e => e.RevokedByIp).HasMaxLength(50);
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500);
            entity.Property(e => e.RevokeReason).HasMaxLength(200);

            entity.HasIndex(e => e.Token);
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt });

            entity.HasOne(e => e.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===========================================
        // Tenant Configuration
        // ===========================================
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subdomain).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CustomDomain).HasMaxLength(200);
            entity.Property(e => e.ContactEmail).HasMaxLength(256).IsRequired();
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.CompanyName).HasMaxLength(200);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(10);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.DatabaseName).HasMaxLength(100);
            entity.Property(e => e.ConnectionString).HasMaxLength(1000);
            entity.Property(e => e.CurrencyCode).HasMaxLength(10);
            entity.Property(e => e.TimeZone).HasMaxLength(100);
            entity.Property(e => e.DeletionReason).HasMaxLength(500);
            entity.Property(e => e.SuspensionReason).HasMaxLength(500);

            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.HasIndex(e => e.ContactEmail);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsDeleted);

            // Configure CurrentSubscription relationship explicitly
            entity.HasOne(e => e.CurrentSubscription)
                .WithOne()
                .HasForeignKey<Tenant>(e => e.CurrentSubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);

            // Soft delete query filter
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // ===========================================
        // TenantUser Configuration
        // ===========================================
        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.ToTable("TenantUsers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.FullName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // ===========================================
        // BillingPlan Configuration
        // ===========================================
        modelBuilder.Entity<BillingPlan>(entity =>
        {
            entity.ToTable("BillingPlans");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.MonthlyPrice).HasPrecision(18, 2);
            entity.Property(e => e.AnnualPrice).HasPrecision(18, 2);
            entity.Property(e => e.AnnualDiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.Features).HasMaxLength(4000);
            entity.Property(e => e.IncludedModules).HasMaxLength(1000);

            entity.HasIndex(e => e.Code).IsUnique();
        });

        // ===========================================
        // TenantSubscription Configuration
        // ===========================================
        modelBuilder.Entity<TenantSubscription>(entity =>
        {
            entity.ToTable("TenantSubscriptions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethodId).HasMaxLength(100);
            entity.Property(e => e.PaymentGateway).HasMaxLength(50);
            entity.Property(e => e.ExternalSubscriptionId).HasMaxLength(200);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);

            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => e.NextBillingDate);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.PlanId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ===========================================
        // TenantBillingInvoice Configuration
        // ===========================================
        modelBuilder.Entity<TenantBillingInvoice>(entity =>
        {
            entity.ToTable("TenantBillingInvoices");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.InvoiceNumber).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Currency).HasMaxLength(10);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.AmountPaid).HasPrecision(18, 2);
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.PaymentTransactionId).HasMaxLength(200);
            entity.Property(e => e.PaymentGateway).HasMaxLength(50);
            entity.Property(e => e.PdfUrl).HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(1000);

            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => new { e.TenantId, e.Status });
            entity.HasIndex(e => e.DueDate);

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Invoices)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(e => e.Subscription)
                .WithMany(s => s.Invoices)
                .HasForeignKey(e => e.SubscriptionId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // ===========================================
        // TenantBillingInvoiceLine Configuration
        // ===========================================
        modelBuilder.Entity<TenantBillingInvoiceLine>(entity =>
        {
            entity.ToTable("TenantBillingInvoiceLines");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Description).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Quantity).HasPrecision(18, 4);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 4);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.ItemType).HasMaxLength(50);
            entity.Property(e => e.ItemCode).HasMaxLength(50);

            entity.HasOne(e => e.Invoice)
                .WithMany(i => i.Lines)
                .HasForeignKey(e => e.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
