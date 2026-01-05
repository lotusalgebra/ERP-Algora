using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Administration;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Data;

/// <summary>
/// Master database context for tenant management
/// </summary>
public class MasterDbContext : DbContext, IMasterDbContext
{
    public MasterDbContext(DbContextOptions<MasterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Subdomain).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Subdomain).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ConnectionString).HasMaxLength(500).IsRequired();
            entity.Property(e => e.DatabaseName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.ContactEmail).HasMaxLength(255).IsRequired();
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.PrimaryColor).HasMaxLength(20);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.TaxId).HasMaxLength(50);
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.TimeZone).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<TenantUser>(entity =>
        {
            entity.ToTable("TenantUsers");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.TenantUsers)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
