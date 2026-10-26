using System.ComponentModel.DataAnnotations;

namespace tp2.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 5)]
        [Display(Name = "Nom du produit")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 9999999, ErrorMessage = "Le prix doit être supérieur à 0.")]
        [Display(Name = "Prix (DT)")]
        public float Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "La quantité ne peut pas être négative.")]
        [Display(Name = "Quantité en stock")]
        public int QteStock { get; set; }

        [Display(Name = "Catégorie")]
        public int CategoryId { get; set; }

        public Category? Category { get; set; }

        [Required]
        [Display(Name = "Image")]
        public string Image { get; set; } = string.Empty;
    }
}
