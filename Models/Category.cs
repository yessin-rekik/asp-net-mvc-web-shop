using System.ComponentModel.DataAnnotations;

namespace tp2.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Le nom de la catégorie est obligatoire.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Le nom doit contenir entre 2 et 80 caractères.")]
        [Display(Name = "Nom de la catégorie")]
        public string CategoryName { get; set; } = string.Empty;

        public ICollection<Product>? Products { get; set; }
    }
}
