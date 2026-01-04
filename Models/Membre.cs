using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models;

public class Membre
{
    public int Id { get; set; }

    [Required]
    public string Nom { get; set; } = string.Empty;

    public int Age { get; set; }

    public string? Telephone { get; set; }

    public List<Paiement>? Paiements { get; set; }
}
