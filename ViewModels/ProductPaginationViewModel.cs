using tp2.Models;

namespace tp2.ViewModels
{
    public class ProductPaginationViewModel
    {
        public List<Product> Products { get; set; } = new();
        public int PageActuelle { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public int TotalProduits { get; set; }
        public int PageSize { get; set; }
        public int? CategoryId { get; set; }
        public string? SearchTerm { get; set; }

        public bool HasPreviousPage => PageActuelle > 1;
        public bool HasNextPage => PageActuelle < TotalPages;
    }
}
