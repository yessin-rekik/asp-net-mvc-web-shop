namespace tp2.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public float Price { get; set; }
        public string? Image { get; set; }
        public float TotalPrice => Price * Quantity;
    }
}
