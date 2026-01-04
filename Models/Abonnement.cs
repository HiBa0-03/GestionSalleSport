using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Abonnement
{
    public int Id { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal Prix { get; set; }

    // Durée en mois
    public int Duree { get; set; }
}
