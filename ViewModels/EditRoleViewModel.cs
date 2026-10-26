using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
        }

        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le nom du rôle est obligatoire.")]
        [Display(Name = "Nom du rôle")]
        public string RoleName { get; set; } = string.Empty;

        public List<string> Users { get; set; }
    }
}
