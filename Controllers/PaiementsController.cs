using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class PaiementsController : Controller
{
    private readonly AppDbContext _context;

    public PaiementsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var paiements = _context.Paiements.Include(p => p.Membre);
        return View(await paiements.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var paiement = await _context.Paiements.Include(p => p.Membre).FirstOrDefaultAsync(p => p.Id == id);
        if (paiement == null) return NotFound();
        return View(paiement);
    }

    public IActionResult Create()
    {
        ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "Nom");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Paiement paiement)
    {
        if (ModelState.IsValid)
        {
            _context.Add(paiement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "Nom", paiement.MembreId);
        return View(paiement);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var paiement = await _context.Paiements.FindAsync(id);
        if (paiement == null) return NotFound();
        ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "Nom", paiement.MembreId);
        return View(paiement);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Paiement paiement)
    {
        if (id != paiement.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(paiement);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Paiements.Any(e => e.Id == id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        ViewData["MembreId"] = new SelectList(_context.Membres, "Id", "Nom", paiement.MembreId);
        return View(paiement);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var paiement = await _context.Paiements.Include(p => p.Membre).FirstOrDefaultAsync(p => p.Id == id);
        if (paiement == null) return NotFound();
        return View(paiement);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var paiement = await _context.Paiements.FindAsync(id);
        if (paiement != null)
        {
            _context.Paiements.Remove(paiement);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
