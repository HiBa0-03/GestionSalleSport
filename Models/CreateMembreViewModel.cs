using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class CreateMembreViewModel
{
    [Required(ErrorMessage = "Le nom complet est requis.")]
    [Display(Name = "Nom complet")]
    public string Nom { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le téléphone est requis.")]
    [Display(Name = "Téléphone")]
    public string Telephone { get; set; } = string.Empty;

    [Display(Name = "Âge")]
    [Range(1, 120, ErrorMessage = "L'âge doit être entre 1 et 120 ans.")]
    public int Age { get; set; }

    [Required(ErrorMessage = "Le type d'abonnement est requis.")]
    [Display(Name = "Type d'abonnement")]
    public string TypeAbonnement { get; set; } = string.Empty;

    [Display(Name = "Durée (mois)")]
    [Range(1, int.MaxValue, ErrorMessage = "La durée doit être d'au moins 1 mois.")]
    public int Duree { get; set; } = 1;
}



