using System.ComponentModel.DataAnnotations;

namespace tp2.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public int ProductId { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public float Price { get; set; }

        public float TotalPrice => Price * Quantity;
    }
}
