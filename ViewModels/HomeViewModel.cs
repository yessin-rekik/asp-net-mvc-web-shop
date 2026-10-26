using tp2.Models;

namespace tp2.ViewModels
{
    public class HomeViewModel
    {
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
    }
}
