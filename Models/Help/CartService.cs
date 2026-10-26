using System.Text.Json;
using tp2.Models.Repositories;

namespace tp2.Models.Help
{
    public class CartService
    {
        private const string SessionKey = "tp2.ShoppingCart";
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IProductRepository productRepository;

        public CartService(IHttpContextAccessor httpContextAccessor, IProductRepository productRepository)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.productRepository = productRepository;
        }

        public ListeCart GetCart()
        {
            var cart = new ListeCart();

            foreach (var entry in ReadEntries())
            {
                var product = productRepository.GetById(entry.Key);
                if (product == null || product.QteStock <= 0)
                {
                    continue;
                }

                var quantity = Math.Clamp(entry.Value, 1, product.QteStock);
                cart.AddItem(product, quantity);
            }

            return cart;
        }

        public void Save(ListeCart cart)
        {
            var entries = cart.Items
                .Where(i => i.quantite > 0)
                .GroupBy(i => i.Prod.ProductId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.quantite));

            var session = httpContextAccessor.HttpContext?.Session;
            if (session == null)
            {
                return;
            }

            session.SetString(SessionKey, JsonSerializer.Serialize(entries));
        }

        public void Clear()
        {
            httpContextAccessor.HttpContext?.Session.Remove(SessionKey);
        }

        public int GetItemCount()
        {
            return ReadEntries().Values.Sum();
        }

        private Dictionary<int, int> ReadEntries()
        {
            var session = httpContextAccessor.HttpContext?.Session;
            var raw = session?.GetString(SessionKey);

            if (string.IsNullOrWhiteSpace(raw))
            {
                return new Dictionary<int, int>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<int, int>>(raw)
                       ?? new Dictionary<int, int>();
            }
            catch (JsonException)
            {
                session?.Remove(SessionKey);
                return new Dictionary<int, int>();
            }
        }
    }
}
