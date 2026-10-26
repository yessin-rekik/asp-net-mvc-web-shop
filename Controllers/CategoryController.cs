using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tp2.Models;
using tp2.Models.Repositories;

namespace tp2.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class CategoryController : Controller
    {
        readonly ICategorieRepository CategoryRepository;

        public CategoryController(ICategorieRepository CatRepository)
        {
            CategoryRepository = CatRepository;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            LoadCategories();
            var Categories = CategoryRepository.GetAll();
            return View(Categories);
        }

        [AllowAnonymous]
        public ActionResult Details(int id)
        {
            LoadCategories();

            var c = CategoryRepository.GetById(id);
            if (c == null)
                return NotFound();

            return View(c);
        }

        public ActionResult Create()
        {
            LoadCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category c)
        {
            LoadCategories();

            if (!ModelState.IsValid)
                return View(c);

            CategoryRepository.Add(c);
            TempData["SuccessMessage"] = "La catégorie a été créée.";
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Edit(int id)
        {
            LoadCategories();

            var c = CategoryRepository.GetById(id);
            if (c == null)
                return NotFound();

            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category c)
        {
            LoadCategories();

            if (!ModelState.IsValid)
                return View(c);

            var updated = CategoryRepository.Update(c);
            if (updated == null)
                return NotFound();

            TempData["SuccessMessage"] = "La catégorie a été mise à jour.";
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int id)
        {
            LoadCategories();

            var c = CategoryRepository.GetById(id);
            if (c == null)
                return NotFound();

            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Category c)
        {
            var existing = CategoryRepository.GetById(c.CategoryId);
            if (existing == null)
                return NotFound();

            if (existing.Products != null && existing.Products.Any())
            {
                TempData["ErrorMessage"] = "Impossible de supprimer une catégorie qui contient des produits.";
                return RedirectToAction(nameof(Index));
            }

            CategoryRepository.Delete(c.CategoryId);
            TempData["SuccessMessage"] = "La catégorie a été supprimée.";
            return RedirectToAction(nameof(Index));
        }

        private void LoadCategories()
        {
            ViewData["Categories"] = CategoryRepository.GetAll();
        }
    }
}
