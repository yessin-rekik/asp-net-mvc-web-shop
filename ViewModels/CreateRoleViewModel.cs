using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [Display(Name = "Nom du rôle")]
        public string RoleName { get; set; } = string.Empty;
    }
}
