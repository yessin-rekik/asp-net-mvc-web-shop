using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using tp2.Models;
using tp2.Models.Repositories;
using tp2.ViewModels;

namespace tp2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ProductController : Controller
    {
        private const int PageSize = 6;
        private const long MaxImageSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        readonly IProductRepository ProductRepository;
        readonly ICategorieRepository CategRepository;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(IProductRepository ProdRepository, ICategorieRepository categRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            ProductRepository = ProdRepository;
            CategRepository = categRepository;
            this.hostingEnvironment = hostingEnvironment;
        }

        [AllowAnonymous]
        public IActionResult Index(int? categoryId, int page = 1, string? q = null)
        {
            LoadCategories();
            return View(BuildPagedModel(categoryId, page, q));
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            var product = ProductRepository.GetById(id);

            if (product == null)
                return NotFound();

            LoadCategories();
            ViewBag.RelatedProducts = ProductRepository.GetAllProducts()
                .Where(p => p.CategoryId == product.CategoryId && p.ProductId != product.ProductId)
                .Take(3)
                .ToList();

            return View(product);
        }

        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateViewModel model)
        {
            LoadCategories();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var uniqueFileName = SaveUploadedImage(model.ImagePath, out var imageError);
            if (imageError != null || uniqueFileName == null)
            {
                ModelState.AddModelError(nameof(model.ImagePath), imageError ?? "Veuillez sélectionner une image.");
                return View(model);
            }

            Product newProduct = new Product
            {
                Name = model.Name,
                Price = model.Price,
                QteStock = model.QteStock,
                CategoryId = model.CategoryId,
                Image = uniqueFileName
            };

            ProductRepository.Add(newProduct);
            TempData["SuccessMessage"] = "Le produit a été créé avec succès.";
            return RedirectToAction(nameof(Details), new { id = newProduct.ProductId });
        }

        public ActionResult Edit(int id)
        {
            LoadCategories();

            Product? product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            EditViewModel productEditViewModel = new EditViewModel
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Price = product.Price,
                QteStock = product.QteStock,
                CategoryId = product.CategoryId,
                ExistingImagePath = product.Image
            };
            return View(productEditViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditViewModel model)
        {
            LoadCategories();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Product? product = ProductRepository.GetById(model.ProductId);
            if (product == null)
                return NotFound();

            string? newImage = null;
            if (model.ImagePath != null && model.ImagePath.Length > 0)
            {
                newImage = SaveUploadedImage(model.ImagePath, out var imageError);
                if (imageError != null || newImage == null)
                {
                    ModelState.AddModelError(nameof(model.ImagePath), imageError ?? "Image invalide.");
                    return View(model);
                }
            }

            product.Name = model.Name;
            product.Price = model.Price;
            product.QteStock = model.QteStock;
            product.CategoryId = model.CategoryId;
            if (newImage != null)
            {
                product.Image = newImage;
            }

            Product? updatedProduct = ProductRepository.Update(product);
            if (updatedProduct == null)
                return NotFound();

            if (newImage != null)
            {
                DeleteImageFile(model.ExistingImagePath);
            }

            TempData["SuccessMessage"] = "Le produit a été mis à jour.";
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            LoadCategories();

            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = ProductRepository.GetById(id);
            if (product == null)
                return NotFound();

            ProductRepository.Delete(id);
            DeleteImageFile(product.Image);

            TempData["SuccessMessage"] = "Le produit a été supprimé.";
            return RedirectToAction(nameof(Index));
        }

        [AllowAnonymous]
        public ActionResult ByCategory(int? categId)
        {
            LoadCategories();
            return View("Index", BuildPagedModel(categId, 1, null));
        }

        [AllowAnonymous]
        public ActionResult Search(string name)
        {
            LoadCategories();
            return View("Index", BuildPagedModel(null, 1, name));
        }

        private ProductPaginationViewModel BuildPagedModel(int? categoryId, int page, string? search)
        {
            page = Math.Max(1, page);

            IQueryable<Product> productsQuery = ProductRepository.GetAllProducts();

            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                productsQuery = productsQuery.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Category != null && p.Category.CategoryName.Contains(term)));
            }

            var totalProducts = productsQuery.Count();
            var totalPages = Math.Max(1, (int)Math.Ceiling((double)totalProducts / PageSize));
            page = Math.Min(page, totalPages);

            var products = productsQuery
                .OrderBy(p => p.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return new ProductPaginationViewModel
            {
                Products = products,
                PageActuelle = page,
                TotalPages = totalPages,
                TotalProduits = totalProducts,
                PageSize = PageSize,
                CategoryId = categoryId,
                SearchTerm = search
            };
        }

        private void LoadCategories()
        {
            var categories = CategRepository.GetAll();
            ViewData["Categories"] = categories;
            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName");
        }

        private string? SaveUploadedImage(IFormFile? file, out string? error)
        {
            error = null;

            if (file == null || file.Length == 0)
            {
                return null;
            }

            if (file.Length > MaxImageSize)
            {
                error = "L'image ne doit pas dépasser 5 Mo.";
                return null;
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                error = "Format d'image non pris en charge. Utilisez JPG, PNG, GIF ou WEBP.";
                return null;
            }

            var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.CreateNew);
            file.CopyTo(fileStream);

            return uniqueFileName;
        }

        private void DeleteImageFile(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            var uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            var filePath = Path.Combine(uploadsFolder, Path.GetFileName(fileName));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
