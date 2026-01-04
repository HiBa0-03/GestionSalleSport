using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class AbonnementsController : Controller
{
    private readonly AppDbContext _context;

    public AbonnementsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Abonnements.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Abonnements.FirstOrDefaultAsync(a => a.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    public IActionResult Create()
    {
        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Abonnement abonnement)
    {
        // Calculer automatiquement le prix si le type est valide
        if (AbonnementType.EstTypeValide(abonnement.Type))
        {
            abonnement.Prix = AbonnementType.CalculerPrix(abonnement.Type, abonnement.Duree);
        }

        if (ModelState.IsValid)
        {
            _context.Add(abonnement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        
        // Réinitialiser ViewData en cas d'erreur
        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(abonnement);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Abonnements.FindAsync(id);
        if (item == null) return NotFound();
        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Abonnement abonnement)
    {
        if (id != abonnement.Id) return NotFound();
        
        // Calculer automatiquement le prix si le type est valide
        if (AbonnementType.EstTypeValide(abonnement.Type))
        {
            abonnement.Prix = AbonnementType.CalculerPrix(abonnement.Type, abonnement.Duree);
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(abonnement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Abonnements.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        // Réinitialiser ViewData en cas d'erreur
        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(abonnement);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.Abonnements.FirstOrDefaultAsync(a => a.Id == id);
        if (item == null) return NotFound();
        return View(item);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var item = await _context.Abonnements.FindAsync(id);
        if (item != null)
        {
            _context.Abonnements.Remove(item);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
