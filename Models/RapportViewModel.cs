using WebApplication1.Models;

namespace WebApplication1.Models;

public class RapportViewModel
{
    public int MembresCount { get; set; }
    public int AbonnementsCount { get; set; }
    public int PaiementsCount { get; set; }
    public decimal TotalRevenus { get; set; }
    public decimal Revenus30Jours { get; set; }
    public List<Paiement> DerniersPaiements { get; set; } = new();
}






