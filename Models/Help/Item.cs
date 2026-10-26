namespace tp2.Models.Help
{
    public class Item
    {
        public int quantite { get; set; }

        public Product Prod { get; set; } = null!;

        public string Description => Prod.Name;

        public float UnitPrice => Prod.Price;

        public int categoryId => Prod.CategoryId;

        public Category? category => Prod.Category;

        public float TotalPrice => Prod.Price * quantite;

        public Item()
        {
        }

        public Item(Product p)
        {
            Prod = p;
        }

        public bool Equals(Item item)
        {
            return item.Prod.ProductId == Prod.ProductId;
        }
    }
}
