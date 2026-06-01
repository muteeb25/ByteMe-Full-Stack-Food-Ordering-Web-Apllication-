using ByteMe.Data;
using ByteMe.Services;
using ByteMe.Services.Observers;
using ByteMe.Services.Patterns;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for Netlify frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlify", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "SqlServer";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (dbProvider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        options.UseSqlite(connectionString);
    else
        options.UseSqlServer(connectionString);
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddScoped<PaymentContext>();
builder.Services.AddSingleton<OrderNotificationService>(sp =>
{
    var service = new OrderNotificationService();
    service.Subscribe(new CustomerNotificationObserver());
    service.Subscribe(new AdminLogObserver());
    return service;
});
builder.Services.AddSingleton<CustomerNotificationObserver>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowNetlify");
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

SeedData.Initialize(app);

app.Run();
