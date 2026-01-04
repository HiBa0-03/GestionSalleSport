using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class MembresAbonnementsController : Controller
{
    private readonly AppDbContext _context;

    public MembresAbonnementsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Récupérer tous les membres avec leurs paiements
        var membres = await _context.Membres
            .Include(m => m.Paiements)
            .ToListAsync();

        // Récupérer tous les abonnements
        var abonnements = await _context.Abonnements.ToListAsync();

        // Créer une vue combinée
        var viewModel = new MembresAbonnementsViewModel
        {
            Membres = membres,
            Abonnements = abonnements,
            MembreAbonnements = new List<MembreAbonnementViewModel>()
        };

        // Pour chaque membre, trouver son dernier paiement et l'abonnement associé
        foreach (var membre in membres)
        {
            var dernierPaiement = membre.Paiements?.OrderByDescending(p => p.Date).FirstOrDefault();
            var totalPaiements = membre.Paiements?.Sum(p => p.Montant) ?? 0;

            // Trouver l'abonnement le plus récent (basé sur le montant du dernier paiement)
            Abonnement? abonnementAssocie = null;
            if (dernierPaiement != null)
            {
                abonnementAssocie = abonnements
                    .Where(a => a.Prix == dernierPaiement.Montant)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault();
            }

            viewModel.MembreAbonnements.Add(new MembreAbonnementViewModel
            {
                MembreId = membre.Id,
                MembreNom = membre.Nom,
                MembreAge = membre.Age,
                MembreTelephone = membre.Telephone,
                AbonnementId = abonnementAssocie?.Id,
                AbonnementType = abonnementAssocie?.Type,
                AbonnementPrix = abonnementAssocie?.Prix,
                AbonnementDuree = abonnementAssocie?.Duree,
                DernierPaiement = dernierPaiement?.Date,
                TotalPaiements = totalPaiements
            });
        }

        return View(viewModel);
    }
}

