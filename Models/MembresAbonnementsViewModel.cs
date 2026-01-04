namespace WebApplication1.Models;

public class MembresAbonnementsViewModel
{
    public List<Membre> Membres { get; set; } = new();
    public List<Abonnement> Abonnements { get; set; } = new();
    public List<MembreAbonnementViewModel> MembreAbonnements { get; set; } = new();
}



