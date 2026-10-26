using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tp2.Models;
using tp2.Models.Repositories;

namespace tp2.Controllers
{
    [Authorize(Roles = "Admin,Manager,OrderManager")]
    public class OrdersController : Controller
    {
        private readonly IOrderRepository orderRepository;

        public OrdersController(IOrderRepository orderRepository)
        {
            this.orderRepository = orderRepository;
        }

        public IActionResult Index(string? status, string? q)
        {
            IEnumerable<Order> orders = orderRepository.GetAll();

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                orders = orders.Where(o =>
                    o.Id.ToString().Contains(term) ||
                    o.CustomerName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    o.Email.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Statuses = OrderStatuses.All;
            ViewBag.SelectedStatus = status;
            ViewBag.SearchTerm = q;

            return View(orders.ToList());
        }

        public IActionResult Details(int id)
        {
            var order = orderRepository.GetById(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Statuses = OrderStatuses.All;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateStatus(int id, string status)
        {
            if (!OrderStatuses.All.Contains(status))
            {
                TempData["ErrorMessage"] = "Statut de commande invalide.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (!orderRepository.UpdateStatus(id, status))
            {
                return NotFound();
            }

            TempData["SuccessMessage"] = "Le statut de la commande a été mis à jour.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
