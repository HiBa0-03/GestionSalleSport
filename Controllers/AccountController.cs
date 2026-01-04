using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == model.Username);

        if (admin == null || !VerifyPassword(model.Password, admin.PasswordHash))
        {
            ModelState.AddModelError("", "Nom d'utilisateur ou mot de passe incorrect.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }

    private bool VerifyPassword(string password, string hash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == hash;
    }

    // Méthode utilitaire pour créer un admin initial (à utiliser une seule fois)
    public async Task<IActionResult> CreateInitialAdmin()
    {
        if (await _context.Admins.AnyAsync())
        {
            return Content("Un admin existe déjà.");
        }

        var admin = new Admin
        {
            Username = "admin",
            PasswordHash = HashPassword("admin123"),
            Email = "admin@perfectfitness.com",
            CreatedAt = DateTime.UtcNow
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        return Content("Admin créé avec succès! Username: admin, Password: admin123");
    }

    // Action de diagnostic pour vérifier/réinitialiser l'admin
    [HttpGet]
    public async Task<IActionResult> ResetAdmin()
    {
        var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == "admin");
        
        if (existingAdmin != null)
        {
            // Réinitialiser le mot de passe de l'admin existant
            existingAdmin.PasswordHash = HashPassword("admin123");
            _context.Admins.Update(existingAdmin);
            await _context.SaveChangesAsync();
            return Content($"Admin réinitialisé avec succès!<br/>Nom d'utilisateur: <strong>admin</strong><br/>Mot de passe: <strong>admin123</strong>", "text/html");
        }
        else
        {
            // Créer un nouvel admin
            var admin = new Admin
            {
                Username = "admin",
                PasswordHash = HashPassword("admin123"),
                Email = "admin@perfectfitness.com",
                CreatedAt = DateTime.UtcNow
            };

            _context.Admins.Add(admin);
            await _context.SaveChangesAsync();
            return Content($"Admin créé avec succès!<br/>Nom d'utilisateur: <strong>admin</strong><br/>Mot de passe: <strong>admin123</strong>", "text/html");
        }
    }

    // Action pour vérifier l'état de l'admin
    [HttpGet]
    public async Task<IActionResult> CheckAdmin()
    {
        var admins = await _context.Admins.ToListAsync();
        var result = $"Nombre d'admins dans la base de données: {admins.Count}<br/><br/>";
        
        foreach (var admin in admins)
        {
            result += $"ID: {admin.Id}<br/>";
            result += $"Nom d'utilisateur: {admin.Username}<br/>";
            result += $"Email: {admin.Email}<br/>";
            result += $"Hash du mot de passe: {admin.PasswordHash.Substring(0, Math.Min(20, admin.PasswordHash.Length))}...<br/><br/>";
        }
        
        if (admins.Count == 0)
        {
            result += "<a href='/Account/ResetAdmin'>Cliquez ici pour créer/réinitialiser l'admin</a>";
        }
        else
        {
            result += "<a href='/Account/ResetAdmin'>Cliquez ici pour réinitialiser le mot de passe de l'admin</a>";
        }
        
        return Content(result, "text/html");
    }
}

