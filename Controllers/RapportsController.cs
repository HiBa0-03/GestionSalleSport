using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Authorize]
public class RapportsController : Controller
{
    private readonly AppDbContext _context;

    public RapportsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var now = DateTime.UtcNow;
        var date30Jours = now.AddDays(-30);

        var membresCount = await _context.Membres.CountAsync();
        var abonnementsCount = await _context.Abonnements.CountAsync();
        var paiementsQuery = _context.Paiements.Include(p => p.Membre);
        var paiementsList = await paiementsQuery.ToListAsync(); // passage côté client pour éviter les limites SQLite sur decimal + SUM

        var paiementsCount = paiementsList.Count;
        var totalRevenus = paiementsList.Sum(p => p.Montant);
        var revenus30Jours = paiementsList
            .Where(p => p.Date >= date30Jours)
            .Sum(p => p.Montant);

        var derniersPaiements = paiementsList
            .OrderByDescending(p => p.Date)
            .Take(10)
            .ToList();

        var vm = new RapportViewModel
        {
            MembresCount = membresCount,
            AbonnementsCount = abonnementsCount,
            PaiementsCount = paiementsCount,
            TotalRevenus = totalRevenus,
            Revenus30Jours = revenus30Jours,
            DerniersPaiements = derniersPaiements
        };

        return View(vm);
    }
}

