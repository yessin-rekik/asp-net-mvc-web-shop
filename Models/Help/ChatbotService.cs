using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;
using tp2.Models.Repositories;
using tp2.ViewModels;

namespace tp2.Models.Help
{
    public class ChatbotService
    {
        private static readonly string[] StopWords =
        {
            "bonjour", "salut", "hello", "bonsoir", "merci", "svp", "stp",
            "produit", "produits", "article", "articles", "cherche", "trouve",
            "acheter", "achete", "pour", "avec", "dans", "les", "des", "une",
            "mon", "ma", "mes", "est", "quel", "quelle", "quels", "quelles",
            "sont", "avez", "vous", "nous", "voir", "affiche", "montre", "moi",
            "sur", "les", "aux", "que", "qui", "comment", "combien", "puis"
        };

        private readonly IProductRepository productRepository;
        private readonly ICategorieRepository categoryRepository;
        private readonly IOrderRepository orderRepository;

        public ChatbotService(
            IProductRepository productRepository,
            ICategorieRepository categoryRepository,
            IOrderRepository orderRepository)
        {
            this.productRepository = productRepository;
            this.categoryRepository = categoryRepository;
            this.orderRepository = orderRepository;
        }

        public ChatResponse Respond(string message, IdentityUser? user)
        {
            var raw = (message ?? string.Empty).Trim();
            var text = Normalize(raw);
            var response = new ChatResponse();

            if (string.IsNullOrWhiteSpace(text))
            {
                response.Reply = "Écrivez-moi une question et je vous aiderai avec plaisir.";
                response.Suggestions.AddRange(DefaultSuggestions());
                return response;
            }

            if (ContainsAny(text, "bonjour", "salut", "hello", "bonsoir", "coucou"))
            {
                response.Reply = "Bonjour et bienvenue chez NovaShop. Je peux vous aider à trouver un produit, " +
                                 "suivre une commande ou répondre à vos questions sur la livraison et le paiement.";
                response.Suggestions.AddRange(DefaultSuggestions());
                return response;
            }

            if (ContainsAny(text, "merci", "parfait", "super"))
            {
                response.Reply = "Avec plaisir ! Je reste disponible si vous avez une autre question.";
                response.Suggestions.AddRange(DefaultSuggestions());
                return response;
            }

            if (ContainsAny(text, "livraison", "livrer", "delai", "frais de port", "expedition"))
            {
                response.Reply = "La livraison est offerte sur toutes les commandes de cette démo. " +
                                 "Après validation, vous recevez une confirmation et pouvez retrouver le détail dans « Mes commandes ».";
                response.Suggestions.AddRange(new[] { "Modes de paiement", "Suivre ma commande", "Voir les produits" });
                return response;
            }

            if (ContainsAny(text, "paiement", "payer", "paypal", "carte", "especes", "cash"))
            {
                response.Reply = "Deux méthodes sont proposées : carte de crédit et PayPal. " +
                                 "Le paiement est simulé dans cette version : aucune donnée bancaire réelle n'est demandée ni stockée.";
                response.Suggestions.AddRange(new[] { "Livraison", "Suivre ma commande", "Voir les produits" });
                return response;
            }

            if (ContainsAny(text, "retour", "remboursement", "echange", "garantie"))
            {
                response.Reply = "Les retours sont acceptés sous 7 jours après réception, à condition que le produit soit " +
                                 "dans son état d'origine. Rendez-vous dans « Mes commandes » pour identifier la commande concernée.";
                response.Suggestions.AddRange(new[] { "Suivre ma commande", "Modes de paiement" });
                return response;
            }

            if (ContainsAny(text, "commande", "suivi", "suivre", "historique"))
            {
                if (user == null)
                {
                    response.Reply = "Connectez-vous pour consulter vos commandes. Une fois connecté, ouvrez « Mes commandes » " +
                                     "depuis votre menu de compte.";
                    response.Links.Add(new ChatLink { Label = "Se connecter", Url = "/Account/Login" });
                }
                else
                {
                    var orders = orderRepository.GetByUserId(user.Id);
                    if (orders.Any())
                    {
                        var latest = orders.First();
                        response.Reply = $"Votre dernière commande est la #{latest.Id.ToString("D5")}, " +
                                         $"passée le {latest.OrderDate:dd/MM/yyyy à HH:mm} pour un total de {latest.TotalAmount:N2} DT. " +
                                         "Retrouvez son détail dans « Mes commandes ».";
                        response.Links.Add(new ChatLink { Label = "Mes commandes", Url = "/Panier/Orders" });
                    }
                    else
                    {
                        response.Reply = "Vous n'avez pas encore passé de commande. Votre panier et l'historique apparaîtront ici " +
                                         "dès votre première validation.";
                        response.Links.Add(new ChatLink { Label = "Voir les produits", Url = "/Product/Index" });
                    }
                }

                response.Suggestions.AddRange(new[] { "Livraison", "Modes de paiement" });
                return response;
            }

            if (ContainsAny(text, "voir les produits", "catalogue", "boutique", "tous les produits"))
            {
                response.Reply = "Tous les produits sont disponibles dans le catalogue, avec recherche et filtres par catégorie. " +
                                 "Cliquez ci-dessous pour commencer.";
                response.Links.Add(new ChatLink { Label = "Ouvrir le catalogue", Url = "/Product/Index" });
                response.Suggestions.AddRange(new[] { "Livraison", "Modes de paiement", "Suivre ma commande" });
                return response;
            }

            if (ContainsAny(text, "categorie", "categories", "rayon", "univers"))
            {
                var categories = categoryRepository.GetAll();
                response.Reply = categories.Any()
                    ? "Nous avons " + categories.Count + " univers : " +
                      string.Join(", ", categories.Take(8).Select(c => c.CategoryName)) +
                      ". Cliquez sur une catégorie dans le menu latéral pour filtrer les produits."
                    : "Aucune catégorie n'est encore disponible.";
                response.Links.Add(new ChatLink { Label = "Explorer les catégories", Url = "/Category/Index" });
                response.Suggestions.AddRange(new[] { "Voir les produits", "Livraison" });
                return response;
            }

            if (ContainsAny(text, "panier", "ajouter", "commander"))
            {
                response.Reply = "Pour ajouter un produit, ouvrez sa fiche ou cliquez sur « Ajouter » sur sa carte. " +
                                 "Vous pouvez ensuite ajuster les quantités depuis « Mon panier », puis lancer la commande.";
                response.Links.Add(new ChatLink { Label = "Mon panier", Url = "/Panier/Index" });
                response.Links.Add(new ChatLink { Label = "Voir les produits", Url = "/Product/Index" });
                return response;
            }

            if (ContainsAny(text, "promo", "promotion", "reduction", "solde", "remise"))
            {
                response.Reply = "Il n'y a pas de code promotionnel actif pour le moment, mais de nouveaux produits sont ajoutés " +
                                 "régulièrement. Jetez un œil à la page d'accueil et aux catégories.";
                response.Links.Add(new ChatLink { Label = "Voir les nouveautés", Url = "/Product/Index" });
                return response;
            }

            var products = FindProducts(text);
            if (products.Any())
            {
                response.Reply = products.Count == 1
                    ? "J'ai trouvé un produit qui correspond à votre recherche :"
                    : "Voici les produits qui pourraient vous intéresser :";
                response.Products = products.Select(p => new ChatProductLink
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Category = p.Category?.CategoryName ?? "Sans catégorie",
                    Price = p.Price,
                    Image = p.Image
                }).ToList();

                response.Suggestions.AddRange(new[] { "Voir les produits", "Livraison", "Modes de paiement" });
                return response;
            }

            response.Reply = "Je n'ai pas bien compris votre demande. Je peux vous aider à trouver un produit, " +
                             "consulter les catégories, suivre une commande ou répondre aux questions de livraison et de paiement.";
            response.Suggestions.AddRange(DefaultSuggestions());
            response.Links.Add(new ChatLink { Label = "Voir les produits", Url = "/Product/Index" });
            return response;
        }

        private List<Product> FindProducts(string normalizedText)
        {
            var terms = normalizedText
                .Split(new[] { ' ', ',', '.', '!', '?', ';', ':', '\'', '"' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 3 && !StopWords.Contains(t))
                .Distinct()
                .ToArray();

            if (terms.Length == 0)
            {
                return new List<Product>();
            }

            return productRepository.GetAllProducts()
                .ToList()
                .Where(p =>
                {
                    var name = Normalize(p.Name);
                    var category = Normalize(p.Category?.CategoryName ?? string.Empty);
                    return normalizedText.Contains(name) ||
                           terms.Any(t => name.Contains(t) || category.Contains(t));
                })
                .Take(3)
                .ToList();
        }

        private static IEnumerable<string> DefaultSuggestions()
        {
            return new[] { "Voir les produits", "Livraison", "Modes de paiement", "Suivre ma commande" };
        }

        private static bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(keyword => text.Contains(Normalize(keyword)));
        }

        private static string Normalize(string value)
        {
            var decomposed = value.Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);

            foreach (var character in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            return builder.ToString();
        }
    }
}
