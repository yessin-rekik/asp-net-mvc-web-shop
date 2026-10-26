using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Le mot de passe actuel est obligatoire.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mot de passe actuel")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nouveau mot de passe est obligatoire.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Le mot de passe doit contenir au moins 6 caractères.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nouveau mot de passe")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmer le nouveau mot de passe")]
        [Compare(nameof(NewPassword), ErrorMessage = "Les mots de passe ne correspondent pas.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
