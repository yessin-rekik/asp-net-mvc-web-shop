using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using tp2.Models;
using tp2.Models.Help;
using tp2.Models.Repositories;
using tp2.ViewModels;

namespace tp2.Controllers
{
    public class PanierController : Controller
    {
        readonly IProductRepository productRepository;
        readonly IOrderRepository orderRepository;
        private readonly UserManager<IdentityUser> userManager;
        private readonly CartService cartService;

        public PanierController(
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            UserManager<IdentityUser> userManager,
            CartService cartService)
        {
            this.productRepository = productRepository;
            this.orderRepository = orderRepository;
            this.userManager = userManager;
            this.cartService = cartService;
        }

        public IActionResult Index()
        {
            return View(cartService.GetCart());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddProduct(int id, string? returnUrl = null)
        {
            var product = productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            if (product.QteStock <= 0)
            {
                TempData["ErrorMessage"] = "Ce produit est actuellement en rupture de stock.";
                return RedirectBack(returnUrl);
            }

            var cart = cartService.GetCart();
            var existing = cart.Items.FirstOrDefault(i => i.Prod.ProductId == id);
            if (existing != null && existing.quantite >= product.QteStock)
            {
                TempData["ErrorMessage"] = $"Stock maximum atteint pour « {product.Name} ».";
                return RedirectBack(returnUrl);
            }

            cart.AddItem(product);
            cartService.Save(cart);

            TempData["SuccessMessage"] = $"« {product.Name} » a été ajouté au panier.";
            return RedirectBack(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlusProduct(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Produit introuvable." });
            }

            var cart = cartService.GetCart();
            var item = cart.Items.FirstOrDefault(i => i.Prod.ProductId == id);

            if (item != null && item.quantite >= product.QteStock)
            {
                return Json(new
                {
                    success = false,
                    message = "Stock maximum atteint pour ce produit.",
                    quantity = item.quantite,
                    total = cart.GetSubTotal(),
                    totalRow = item.TotalPrice,
                    cartCount = cart.GetTotalQuantity()
                });
            }

            cart.AddItem(product);
            cartService.Save(cart);

            item = cart.Items.First(i => i.Prod.ProductId == id);
            return Json(new
            {
                success = true,
                ct = 1,
                quantity = item.quantite,
                total = cart.GetSubTotal(),
                totalRow = item.TotalPrice,
                cartCount = cart.GetTotalQuantity()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MinusProduct(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Produit introuvable." });
            }

            var cart = cartService.GetCart();
            cart.SetLessOneItem(product);
            cartService.Save(cart);

            var item = cart.Items.FirstOrDefault(i => i.Prod.ProductId == id);
            if (item == null)
            {
                return Json(new
                {
                    success = true,
                    ct = 0,
                    removed = true,
                    total = cart.GetSubTotal(),
                    cartCount = cart.GetTotalQuantity()
                });
            }

            return Json(new
            {
                success = true,
                ct = 1,
                removed = false,
                quantity = item.quantite,
                total = cart.GetSubTotal(),
                totalRow = item.TotalPrice,
                cartCount = cart.GetTotalQuantity()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveProduct(int id)
        {
            var product = productRepository.GetById(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Produit introuvable." });
            }

            var cart = cartService.GetCart();
            cart.RemoveItem(product);
            cartService.Save(cart);

            return Json(new
            {
                success = true,
                total = cart.GetSubTotal(),
                cartCount = cart.GetTotalQuantity()
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Checkout)) });
            }

            var cart = cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new OrderViewModel
            {
                Address = string.Empty,
                PaymentMethod = "CreditCard",
                CartItems = MapCartItems(cart),
                TotalAmount = cart.GetSubTotal()
            };

            return View(viewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderViewModel model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Checkout)) });
            }

            var cart = cartService.GetCart();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Votre panier est vide.";
                return RedirectToAction(nameof(Index));
            }

            // Le panier est la source de vérité : on recharge toujours les lignes et le total.
            model.CartItems = MapCartItems(cart);
            model.TotalAmount = cart.GetSubTotal();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            foreach (var item in cart.Items)
            {
                if (item.quantite > item.Prod.QteStock)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Stock insuffisant pour « {item.Prod.Name} ». Disponible : {item.Prod.QteStock}.");
                    return View(model);
                }
            }

            var order = new Order
            {
                CustomerName = user.UserName ?? user.Email ?? "Client",
                Email = user.Email ?? string.Empty,
                Address = model.Address,
                PaymentMethod = model.PaymentMethod,
                TotalAmount = model.TotalAmount,
                Status = OrderStatuses.Pending,
                OrderDate = DateTime.Now,
                UserId = user.Id,
                Items = cart.Items.Select(item => new OrderItem
                {
                    ProductId = item.Prod.ProductId,
                    ProductName = item.Prod.Name,
                    Quantity = item.quantite,
                    Price = item.Prod.Price
                }).ToList()
            };

            orderRepository.Add(order);

            foreach (var item in cart.Items)
            {
                var product = productRepository.GetById(item.Prod.ProductId);
                if (product != null)
                {
                    product.QteStock = Math.Max(0, product.QteStock - item.quantite);
                    productRepository.Update(product);
                }
            }

            cartService.Clear();
            TempData["SuccessMessage"] = "Votre commande a été passée avec succès.";
            return RedirectToAction(nameof(Confirmation), new { orderId = order.Id });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Confirmation(int orderId)
        {
            var order = orderRepository.GetById(orderId);
            if (order == null)
            {
                return NotFound();
            }

            var user = await userManager.GetUserAsync(User);
            if (!User.IsInRole("Admin") && (user == null || order.UserId != user.Id))
            {
                return Forbid();
            }

            return View(order);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action(nameof(Orders)) });
            }

            var orders = orderRepository.GetByUserId(user.Id);
            return View(orders);
        }

        private static List<CartItemViewModel> MapCartItems(ListeCart cart)
        {
            return cart.Items.Select(item => new CartItemViewModel
            {
                ProductId = item.Prod.ProductId,
                ProductName = item.Prod.Name,
                Quantity = item.quantite,
                Price = item.Prod.Price,
                Image = item.Prod.Image
            }).ToList();
        }

        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
