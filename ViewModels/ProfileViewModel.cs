using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "L'adresse e-mail est obligatoire.")]
        [EmailAddress(ErrorMessage = "Adresse e-mail invalide.")]
        [Display(Name = "Adresse e-mail")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string? CurrentPassword { get; set; }
    }
}
