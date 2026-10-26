using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace tp2.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "CreditCard";

        public float TotalAmount { get; set; }

        [Required]
        [StringLength(30)]
        public string Status { get; set; } = OrderStatuses.Pending;

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;

        public IdentityUser? User { get; set; }

        public List<OrderItem> Items { get; set; } = new();
    }
}
