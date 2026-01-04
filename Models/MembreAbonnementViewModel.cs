namespace WebApplication1.Models;

public class MembreAbonnementViewModel
{
    public int MembreId { get; set; }
    public string MembreNom { get; set; } = string.Empty;
    public int MembreAge { get; set; }
    public string? MembreTelephone { get; set; }
    public int? AbonnementId { get; set; }
    public string? AbonnementType { get; set; }
    public decimal? AbonnementPrix { get; set; }
    public int? AbonnementDuree { get; set; }
    public DateTime? DernierPaiement { get; set; }
    public decimal? TotalPaiements { get; set; }
}



