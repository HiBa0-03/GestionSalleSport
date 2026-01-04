using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Admin
    public async Task<IActionResult> Index()
    {
        return View(await _context.Admins.ToListAsync());
    }

    // GET: Admin/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(m => m.Id == id);
        if (admin == null)
        {
            return NotFound();
        }

        return View(admin);
    }

    // GET: Admin/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Admin/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAdminViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Vérifier si le nom d'utilisateur existe déjà
            if (await _context.Admins.AnyAsync(a => a.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Ce nom d'utilisateur existe déjà.");
                return View(model);
            }

            var admin = new Admin
            {
                Username = model.Username,
                PasswordHash = HashPassword(model.Password),
                Email = string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Add(admin);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: Admin/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var admin = await _context.Admins.FindAsync(id);
        if (admin == null)
        {
            return NotFound();
        }

        var model = new EditAdminViewModel
        {
            Id = admin.Id,
            Username = admin.Username
        };

        return View(model);
    }

    // POST: Admin/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditAdminViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        // Validation personnalisée pour le mot de passe
        if (!string.IsNullOrWhiteSpace(model.Password) && model.Password != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Les mots de passe ne correspondent pas.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var admin = await _context.Admins.FindAsync(id);
                if (admin == null)
                {
                    return NotFound();
                }

                // Vérifier si le nom d'utilisateur existe déjà (sauf pour l'admin actuel)
                if (await _context.Admins.AnyAsync(a => a.Username == model.Username && a.Id != id))
                {
                    ModelState.AddModelError("Username", "Ce nom d'utilisateur existe déjà.");
                    return View(model);
                }

                admin.Username = model.Username;

                // Mettre à jour le mot de passe seulement si un nouveau mot de passe est fourni
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    admin.PasswordHash = HashPassword(model.Password);
                }

                _context.Update(admin);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AdminExists(model.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: Admin/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var admin = await _context.Admins
            .FirstOrDefaultAsync(m => m.Id == id);
        if (admin == null)
        {
            return NotFound();
        }

        return View(admin);
    }

    // POST: Admin/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var admin = await _context.Admins.FindAsync(id);
        if (admin != null)
        {
            _context.Admins.Remove(admin);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AdminExists(int id)
    {
        return _context.Admins.Any(e => e.Id == id);
    }

    private string HashPassword(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}

