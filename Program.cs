using Microsoft.EntityFrameworkCore;
using Multi.DataAccess.Data;
using Multi.DataAccess.Repository;
using Multi.DataAccess.Repository.IUnitOfWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Multi.Utility;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;
using Multi.DataAccess.DBInitializer;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Adding DB Context to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<IdentityUser,IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

// Configure facebook login
builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "1964625640645653";
    options.AppSecret = "f093c1f922581305c9584d59551af759";
});

// Configure Application Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});

// Adding session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});




// Configure the StripeSettings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

// Add email sender service
builder.Services.AddScoped<IEmailSender,EmailSender>();

builder.Services.AddRazorPages();

// Adding Unit of Work to the container
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Add db initializer
builder.Services.AddScoped<IDBInitializer, DBInitializer>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Configure Stripe API
StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:SecretKey").Get<string>();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
SeedDatabase();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();


void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDBInitializer>();
        dbInitializer.Initialize();
    }
}