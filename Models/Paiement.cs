using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Paiement
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Montant { get; set; }

    // Foreign key to Membre
    public int MembreId { get; set; }
    public Membre? Membre { get; set; }
}
