using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddOpenApi();

// Configuration de l'authentification
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Ensure database is created (for development). For migration history, run migrations with dotnet-ef.
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        
        // Créer un admin par défaut s'il n'existe pas
        if (!db.Admins.Any())
        {
            string HashPassword(string password)
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    return Convert.ToBase64String(hashedBytes);
                }
            }
            
            var admin = new Admin
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Email = "admin@perfectfitness.com",
                CreatedAt = DateTime.UtcNow
            };
            
            db.Admins.Add(admin);
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log l'erreur en développement
        var logger = scope.ServiceProvider.GetService<Microsoft.Extensions.Logging.ILogger<Program>>();
        logger?.LogError(ex, "Erreur lors de l'initialisation de la base de données: {Message}", ex.Message);
        // Ne pas faire échouer l'application si c'est juste un problème d'initialisation
        // L'application peut démarrer même si l'admin n'est pas créé
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
