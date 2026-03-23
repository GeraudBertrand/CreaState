using CreaState.Components;
using CreaState.Data;
using CreaState.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

#region Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
#endregion

#region Authentication
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PrivateAccess", p => p.RequireClaim("Permission", "access_private"));
    options.AddPolicy("ManageRequests", p => p.RequireClaim("Permission", "manage_requests"));
    options.AddPolicy("ManageInventory", p => p.RequireClaim("Permission", "manage_inventory"));
    options.AddPolicy("ViewPrinters", p => p.RequireClaim("Permission", "view_printers"));
    options.AddPolicy("AdminOnly", p => p.RequireClaim("Permission", "admin_access"));
});
#endregion

#region Services
builder.Services.AddScoped<MemberService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<PageHeaderService>();
builder.Services.AddSingleton<PrinterService>();
builder.Services.AddHostedService<PrinterMqttWorker>();
builder.Services.AddScoped<RequestService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddScoped<PrintJobService>();
builder.Services.AddScoped<EmailService>();
#endregion


var app = builder.Build();

// Seed de la base de données
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
