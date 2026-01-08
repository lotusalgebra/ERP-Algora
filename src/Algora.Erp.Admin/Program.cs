using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddRazorPages();

// Database
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AdminDb")));

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Admin", policy => policy.RequireRole("SuperAdmin", "Admin"));
    options.AddPolicy("Support", policy => policy.RequireRole("SuperAdmin", "Admin", "Support"));
});

// Application Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IPlanService, PlanService>();

// HTMX support
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Seed default data on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AdminDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed default roles if not exist
        if (!await context.AdminRoles.AnyAsync())
        {
            var roles = new[]
            {
                new Algora.Erp.Admin.Entities.AdminRole
                {
                    Name = "SuperAdmin",
                    Description = "Full system access",
                    Permissions = "[\"*\"]"
                },
                new Algora.Erp.Admin.Entities.AdminRole
                {
                    Name = "Admin",
                    Description = "Administrative access",
                    Permissions = "[\"tenants.view\", \"tenants.create\", \"tenants.edit\", \"plans.view\", \"users.view\"]"
                },
                new Algora.Erp.Admin.Entities.AdminRole
                {
                    Name = "Support",
                    Description = "Support team access",
                    Permissions = "[\"tenants.view\", \"plans.view\"]"
                }
            };
            context.AdminRoles.AddRange(roles);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default admin roles");
        }

        // Seed default admin user if not exist
        if (!await context.AdminUsers.AnyAsync())
        {
            var superAdminRole = await context.AdminRoles.FirstAsync(r => r.Name == "SuperAdmin");
            var adminUser = new Algora.Erp.Admin.Entities.AdminUser
            {
                Email = "admin@algora.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                RoleId = superAdminRole.Id,
                IsActive = true,
                EmailConfirmed = true
            };
            context.AdminUsers.Add(adminUser);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default admin user: admin@algora.com");
        }

        // Seed default billing plans if not exist
        if (!await context.BillingPlans.AnyAsync())
        {
            var plans = new[]
            {
                new Algora.Erp.Admin.Entities.BillingPlan
                {
                    Code = "FREE",
                    Name = "Free",
                    Description = "Get started with basic features",
                    MonthlyPrice = 0,
                    AnnualPrice = 0,
                    MaxUsers = 2,
                    MaxWarehouses = 1,
                    MaxProducts = 100,
                    MaxTransactionsPerMonth = 50,
                    StorageLimitMB = 500,
                    Features = "[\"Basic Inventory\", \"Simple Invoicing\", \"Email Support\"]",
                    IncludedModules = "[\"inventory\", \"sales\", \"customers\"]",
                    IsActive = true,
                    DisplayOrder = 1
                },
                new Algora.Erp.Admin.Entities.BillingPlan
                {
                    Code = "STARTER",
                    Name = "Starter",
                    Description = "For small businesses getting started",
                    MonthlyPrice = 49,
                    AnnualPrice = 490,
                    MaxUsers = 5,
                    MaxWarehouses = 2,
                    MaxProducts = 500,
                    MaxTransactionsPerMonth = 200,
                    StorageLimitMB = 2000,
                    Features = "[\"Full Inventory\", \"Invoicing & Payments\", \"Basic Reports\", \"Email Support\"]",
                    IncludedModules = "[\"inventory\", \"sales\", \"customers\", \"finance\", \"reports\"]",
                    IsActive = true,
                    DisplayOrder = 2
                },
                new Algora.Erp.Admin.Entities.BillingPlan
                {
                    Code = "PROFESSIONAL",
                    Name = "Professional",
                    Description = "For growing businesses",
                    MonthlyPrice = 149,
                    AnnualPrice = 1490,
                    MaxUsers = 15,
                    MaxWarehouses = 5,
                    MaxProducts = 2000,
                    MaxTransactionsPerMonth = 1000,
                    StorageLimitMB = 10000,
                    Features = "[\"Full Inventory\", \"Multi-Warehouse\", \"Advanced Reports\", \"Payroll\", \"Priority Support\"]",
                    IncludedModules = "[\"inventory\", \"sales\", \"customers\", \"finance\", \"hr\", \"payroll\", \"reports\", \"procurement\"]",
                    IsActive = true,
                    IsPopular = true,
                    DisplayOrder = 3
                },
                new Algora.Erp.Admin.Entities.BillingPlan
                {
                    Code = "ENTERPRISE",
                    Name = "Enterprise",
                    Description = "For large organizations",
                    MonthlyPrice = 399,
                    AnnualPrice = 3990,
                    MaxUsers = -1, // Unlimited
                    MaxWarehouses = -1,
                    MaxProducts = -1,
                    MaxTransactionsPerMonth = -1,
                    StorageLimitMB = -1,
                    Features = "[\"Unlimited Everything\", \"Manufacturing\", \"Projects\", \"Custom Reports\", \"API Access\", \"Dedicated Support\", \"SLA\"]",
                    IncludedModules = "[\"*\"]",
                    IsActive = true,
                    DisplayOrder = 4
                }
            };
            context.BillingPlans.AddRange(plans);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded default billing plans");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database seeding");
    }
}

app.Run();
