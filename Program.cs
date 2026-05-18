using DoctorPortfolioSite.Application.Courses;
using DoctorPortfolioSite.Application.Tokens;
using DoctorPortfolioSite.Data;
using DoctorPortfolioSite.Domain.Models;
using DoctorPortfolioSite.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Specification 8.1: password strength check.
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection(AdminSeedOptions.SectionName));
builder.Services.Configure<SeedingOptions>(builder.Configuration.GetSection(SeedingOptions.SectionName));
builder.Services.AddScoped<RoleSeeder>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<CreditSeeder>();
builder.Services.AddScoped<SampleUserSeeder>();
builder.Services.AddScoped<SampleCourseSeeder>();

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<DoctorPortfolioSite.Application.Credits.ICreditService, DoctorPortfolioSite.Application.Credits.CreditService>();

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddScoped<ICompletionTokenService, ClassCompletionTokenService>();
builder.Services.AddSingleton<DoctorPortfolioSite.Infrastructure.Qr.IQrCodeService, DoctorPortfolioSite.Infrastructure.Qr.QrCodeService>();
builder.Services.AddScoped<DoctorPortfolioSite.Application.CourseCompletions.ICourseCompletionService, DoctorPortfolioSite.Application.CourseCompletions.CourseCompletionService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        db.Database.Migrate();
    }
    await scope.ServiceProvider.GetRequiredService<RoleSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<AdminSeeder>().SeedAsync();
    await scope.ServiceProvider.GetRequiredService<CreditSeeder>().SeedAsync();

    var seedingOptions = scope.ServiceProvider.GetRequiredService<IOptions<SeedingOptions>>().Value;
    if (seedingOptions.LoadSampleData)
    {
        await scope.ServiceProvider.GetRequiredService<SampleUserSeeder>().SeedAsync();
        await scope.ServiceProvider.GetRequiredService<SampleCourseSeeder>().SeedAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();

public partial class Program { }
