using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using tp2.Models;
using tp2.Models.Repositories;
using tp2.ViewModels;

namespace tp2.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository productRepository;
        private readonly ICategorieRepository categoryRepository;

        public HomeController(IProductRepository productRepository, ICategorieRepository categoryRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
        }

        public IActionResult Index()
        {
            var productsQuery = productRepository.GetAllProducts();
            var categories = categoryRepository.GetAll();

            var model = new HomeViewModel
            {
                FeaturedProducts = productsQuery
                    .Where(p => p.QteStock > 0)
                    .OrderByDescending(p => p.ProductId)
                    .Take(6)
                    .ToList(),
                Categories = categories.Take(8).ToList(),
                TotalProducts = productsQuery.Count(),
                TotalCategories = categories.Count
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
