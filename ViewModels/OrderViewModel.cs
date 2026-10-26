using System.ComponentModel.DataAnnotations;

namespace tp2.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "L'adresse de livraison est obligatoire.")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "L'adresse doit contenir entre 10 et 500 caractères.")]
        [Display(Name = "Adresse de livraison")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Veuillez choisir une méthode de paiement.")]
        [Display(Name = "Méthode de paiement")]
        public string PaymentMethod { get; set; } = "CreditCard";

        public float TotalAmount { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new();
    }
}
