using Algora.Erp.Application;
using Algora.Erp.Auth;
using Algora.Erp.Infrastructure;
using Algora.Erp.Infrastructure.MultiTenancy;
using Algora.Erp.Integrations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCrmIntegrations(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddRazorPages();

// Add session support for shopping cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication and authorization (Cookie + JWT)
builder.Services.AddAuth(builder.Configuration);

// Configure authorization policies for module access
builder.Services.AddAuthorization(options =>
{
    // Dashboard
    options.AddPolicy("CanViewDashboard", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Dashboard.View") ||
            context.User.IsInRole("Admin")));

    // Finance module
    options.AddPolicy("CanViewFinance", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Finance.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditFinance", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Finance.Edit") ||
            context.User.IsInRole("Admin")));

    // HR module
    options.AddPolicy("CanViewHR", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "HR.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditHR", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "HR.Edit") ||
            context.User.IsInRole("Admin")));

    // Payroll module
    options.AddPolicy("CanViewPayroll", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Payroll.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditPayroll", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Payroll.Edit") ||
            context.User.IsInRole("Admin")));

    // Inventory module
    options.AddPolicy("CanViewInventory", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Inventory.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditInventory", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Inventory.Edit") ||
            context.User.IsInRole("Admin")));

    // Sales module
    options.AddPolicy("CanViewSales", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Sales.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditSales", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Sales.Edit") ||
            context.User.IsInRole("Admin")));

    // Procurement module
    options.AddPolicy("CanViewProcurement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Procurement.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditProcurement", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Procurement.Edit") ||
            context.User.IsInRole("Admin")));

    // Manufacturing module
    options.AddPolicy("CanViewManufacturing", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Manufacturing.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditManufacturing", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Manufacturing.Edit") ||
            context.User.IsInRole("Admin")));

    // Quality module
    options.AddPolicy("CanViewQuality", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Quality.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditQuality", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Quality.Edit") ||
            context.User.IsInRole("Admin")));

    // Projects module
    options.AddPolicy("CanViewProjects", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Projects.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditProjects", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Projects.Edit") ||
            context.User.IsInRole("Admin")));

    // Dispatch module
    options.AddPolicy("CanViewDispatch", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Dispatch.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditDispatch", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Dispatch.Edit") ||
            context.User.IsInRole("Admin")));

    // Ecommerce module
    options.AddPolicy("CanViewEcommerce", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Ecommerce.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditEcommerce", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Ecommerce.Edit") ||
            context.User.IsInRole("Admin")));

    // Admin module
    options.AddPolicy("CanViewAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Admin.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanManageAdmin", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Admin.Manage") ||
            context.User.IsInRole("Admin")));

    // Reports module
    options.AddPolicy("CanViewReports", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Reports.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanExportReports", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Reports.Export") ||
            context.User.IsInRole("Admin")));

    // Settings module
    options.AddPolicy("CanViewSettings", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Settings.View") ||
            context.User.IsInRole("Admin")));
    options.AddPolicy("CanEditSettings", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim("permission", "Settings.Edit") ||
            context.User.IsInRole("Admin")));
});

builder.Services.AddControllers(); // For API endpoints

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();

    // Security headers for production
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Append("Permissions-Policy", "accelerometer=(), camera=(), geolocation=(), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()");
        await next();
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session must come before endpoints
app.UseSession();

// Tenant middleware must come after routing but before authentication
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers(); // For API endpoints

try
{
    Log.Information("Starting Algora ERP application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
