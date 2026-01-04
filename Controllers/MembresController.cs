using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class MembresController : Controller
{
    private readonly AppDbContext _context;

    public MembresController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Membres.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var membre = await _context.Membres.FirstOrDefaultAsync(m => m.Id == id);
        if (membre == null) return NotFound();
        return View(membre);
    }

    public IActionResult Create()
    {
        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMembreViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Vérifier que le type d'abonnement est valide
            if (!AbonnementType.EstTypeValide(model.TypeAbonnement))
            {
                ModelState.AddModelError("TypeAbonnement", "Le type d'abonnement sélectionné n'est pas valide.");
                ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
                return View(model);
            }

            // Créer le membre
            var membre = new Membre
            {
                Nom = model.Nom,
                Telephone = model.Telephone,
                Age = model.Age
            };
            _context.Add(membre);
            await _context.SaveChangesAsync();

            // Créer l'abonnement
            var prixTotal = AbonnementType.CalculerPrix(model.TypeAbonnement, model.Duree);
            var abonnement = new Abonnement
            {
                Type = model.TypeAbonnement,
                Prix = prixTotal,
                Duree = model.Duree
            };
            _context.Add(abonnement);
            await _context.SaveChangesAsync();

            // Créer le paiement initial
            var paiement = new Paiement
            {
                MembreId = membre.Id,
                Date = DateTime.Now,
                Montant = prixTotal
            };
            _context.Add(paiement);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(model);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var membre = await _context.Membres
            .Include(m => m.Paiements)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (membre == null) return NotFound();

        // Récupérer le dernier paiement pour trouver l'abonnement associé
        var dernierPaiement = membre.Paiements?.OrderByDescending(p => p.Date).FirstOrDefault();
        Abonnement? abonnement = null;
        
        if (dernierPaiement != null)
        {
            // Trouver l'abonnement correspondant au montant du dernier paiement
            abonnement = await _context.Abonnements
                .Where(a => a.Prix == dernierPaiement.Montant)
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
        }

        var model = new EditMembreViewModel
        {
            Id = membre.Id,
            Nom = membre.Nom,
            Telephone = membre.Telephone ?? string.Empty,
            Age = membre.Age,
            TypeAbonnement = abonnement?.Type ?? string.Empty,
            Duree = abonnement?.Duree ?? 1
        };

        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditMembreViewModel model)
    {
        if (id != model.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Vérifier que le type d'abonnement est valide
            if (!AbonnementType.EstTypeValide(model.TypeAbonnement))
            {
                ModelState.AddModelError("TypeAbonnement", "Le type d'abonnement sélectionné n'est pas valide.");
                ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
                ViewData["PrixMensuels"] = AbonnementType.GetTypes();
                return View(model);
            }

            try
            {
                // Mettre à jour le membre
                var membre = await _context.Membres.FindAsync(id);
                if (membre == null) return NotFound();

                membre.Nom = model.Nom;
                membre.Telephone = model.Telephone;
                membre.Age = model.Age;
                _context.Update(membre);
                await _context.SaveChangesAsync();

                // Calculer le nouveau prix
                var nouveauPrix = AbonnementType.CalculerPrix(model.TypeAbonnement, model.Duree);

                // Récupérer le dernier paiement pour trouver l'abonnement actuel
                var dernierPaiement = await _context.Paiements
                    .Where(p => p.MembreId == membre.Id)
                    .OrderByDescending(p => p.Date)
                    .FirstOrDefaultAsync();

                Abonnement? abonnementExistant = null;
                if (dernierPaiement != null)
                {
                    abonnementExistant = await _context.Abonnements
                        .Where(a => a.Prix == dernierPaiement.Montant)
                        .OrderByDescending(a => a.Id)
                        .FirstOrDefaultAsync();
                }

                // Si l'abonnement existe et que les valeurs sont différentes, mettre à jour
                if (abonnementExistant != null)
                {
                    if (abonnementExistant.Type != model.TypeAbonnement || 
                        abonnementExistant.Duree != model.Duree || 
                        abonnementExistant.Prix != nouveauPrix)
                    {
                        abonnementExistant.Type = model.TypeAbonnement;
                        abonnementExistant.Duree = model.Duree;
                        abonnementExistant.Prix = nouveauPrix;
                        _context.Update(abonnementExistant);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Créer un nouvel abonnement
                    var nouvelAbonnement = new Abonnement
                    {
                        Type = model.TypeAbonnement,
                        Prix = nouveauPrix,
                        Duree = model.Duree
                    };
                    _context.Add(nouvelAbonnement);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MembreExists(model.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["TypesAbonnement"] = AbonnementType.GetTypeNames();
        ViewData["PrixMensuels"] = AbonnementType.GetTypes();
        return View(model);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var membre = await _context.Membres.FirstOrDefaultAsync(m => m.Id == id);
        if (membre == null) return NotFound();
        return View(membre);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var membre = await _context.Membres.FindAsync(id);
        if (membre != null)
        {
            _context.Membres.Remove(membre);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool MembreExists(int id) => _context.Membres.Any(e => e.Id == id);
}
