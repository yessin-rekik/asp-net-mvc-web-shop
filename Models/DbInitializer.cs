using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace tp2.Models
{
    public static class DbInitializer
    {
        public static readonly string[] Roles = { "Admin", "Manager", "OrderManager", "Client" };

        public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                await context.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "La base de données n'a pas pu être mise à jour au démarrage.");
                return;
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            if (configuration.GetValue<bool>("SeedDemoData"))
            {
                await SeedCatalogAsync(context, logger);
            }

            var adminEmail = configuration["SeedAdmin:Email"];
            var adminPassword = configuration["SeedAdmin:Password"];
            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                {
                    logger.LogWarning("Impossible de créer le compte administrateur : {Errors}",
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                    return;
                }
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }

        private static async Task SeedCatalogAsync(AppDbContext context, ILogger logger)
        {
            if (await context.Products.AnyAsync())
            {
                return;
            }

            var categories = await context.Categories.ToListAsync();
            var createdCategories = 0;

            foreach (var name in new[] { "Électronique", "Maison", "Accessoires", "Sport" })
            {
                if (categories.All(c => c.CategoryName != name))
                {
                    categories.Add(new Category { CategoryName = name });
                    createdCategories++;
                }
            }

            if (createdCategories > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("{Count} catégorie(s) de démonstration créée(s).", createdCategories);
            }

            var categoriesByName = categories.ToDictionary(c => c.CategoryName);

            int CategoryId(string name)
            {
                return categoriesByName.TryGetValue(name, out var category)
                    ? category.CategoryId
                    : categories.First().CategoryId;
            }

            context.Products.AddRange(
                new Product
                {
                    Name = "Casque Bluetooth NovaSound",
                    Price = 129.90f,
                    QteStock = 24,
                    CategoryId = CategoryId("Électronique"),
                    Image = "seed-headphones.svg"
                },
                new Product
                {
                    Name = "Montre connectée Pulse",
                    Price = 249.00f,
                    QteStock = 12,
                    CategoryId = CategoryId("Électronique"),
                    Image = "seed-watch.svg"
                },
                new Product
                {
                    Name = "Enceinte portable Wave",
                    Price = 89.50f,
                    QteStock = 30,
                    CategoryId = CategoryId("Électronique"),
                    Image = "seed-speaker.svg"
                },
                new Product
                {
                    Name = "Lampe de bureau Lumen",
                    Price = 49.90f,
                    QteStock = 40,
                    CategoryId = CategoryId("Maison"),
                    Image = "seed-lamp.svg"
                },
                new Product
                {
                    Name = "Coussin décoratif Terra",
                    Price = 24.50f,
                    QteStock = 60,
                    CategoryId = CategoryId("Maison"),
                    Image = "seed-cushion.svg"
                },
                new Product
                {
                    Name = "Sac à dos urbain Trek",
                    Price = 79.00f,
                    QteStock = 18,
                    CategoryId = CategoryId("Accessoires"),
                    Image = "seed-backpack.svg"
                },
                new Product
                {
                    Name = "Gourde isotherme Hydro",
                    Price = 34.90f,
                    QteStock = 50,
                    CategoryId = CategoryId("Sport"),
                    Image = "seed-bottle.svg"
                },
                new Product
                {
                    Name = "Tapis de yoga Zen",
                    Price = 59.00f,
                    QteStock = 22,
                    CategoryId = CategoryId("Sport"),
                    Image = "seed-yoga.svg"
                });

            await context.SaveChangesAsync();
            logger.LogInformation("Produits de démonstration créés.");
        }
    }
}
