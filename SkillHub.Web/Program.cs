using Microsoft.AspNetCore.Identity;
using SkillHub.Application;
using SkillHub.Infrastructure;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Capas de Clean Architecture
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuración de Cookies de autenticación para la Web
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.Cookie.Name = "SkillHub.Auth.Cookie";
    options.ExpireTimeSpan = TimeSpan.FromHours(24);
    options.SlidingExpiration = true;
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Workshops}/{action=Index}/{id?}");

app.Run();
