using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class EditAdminViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Le nom d'utilisateur est requis.")]
    [Display(Name = "Nom d'utilisateur")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Le nom d'utilisateur doit contenir entre 3 et 50 caractères.")]
    public string Username { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Nouveau mot de passe (laisser vide pour ne pas changer)")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmer le nouveau mot de passe")]
    public string? ConfirmPassword { get; set; }
}

