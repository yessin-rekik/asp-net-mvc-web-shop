using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class CreateViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Le nom du produit est obligatoire.")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Le nom doit contenir entre 5 et 50 caractères.")]
        [Display(Name = "Nom du produit")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le prix est obligatoire.")]
        [Range(0.01, 9999999, ErrorMessage = "Le prix doit être supérieur à 0.")]
        [Display(Name = "Prix (DT)")]
        public float Price { get; set; }

        [Required(ErrorMessage = "La quantité est obligatoire.")]
        [Range(0, int.MaxValue, ErrorMessage = "La quantité ne peut pas être négative.")]
        [Display(Name = "Quantité en stock")]
        public int QteStock { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Veuillez sélectionner une catégorie.")]
        [Display(Name = "Catégorie")]
        public int CategoryId { get; set; }

        [Display(Name = "Image du produit")]
        public IFormFile? ImagePath { get; set; }
    }
}
