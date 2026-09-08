using BuildingBlocks.Common.Constants;
using BuildingBlocks.Common.ValueObjects;
using Catalog.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Api.Data.Seeders;

public static class CatalogDataSeeder
{
    public static async Task SeedCatalogAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<CatalogDbContext>>();
        var dbContext = services.GetRequiredService<CatalogDbContext>();

        try
        {
            if (await dbContext.Categories.AnyAsync())
            {
                logger.LogInformation("Catalog database already seeded. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding initial catalog categories and products...");

            var electronicsCat = Category.Create(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "Electronics & Computers",
                "electronics-computers",
                "Laptops, phones, monitors, and computer hardware.");

            var audioCat = Category.Create(
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "Audio & Peripherals",
                "audio-peripherals",
                "Headphones, mechanical keyboards, and gaming gear.");

            var apparelCat = Category.Create(
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Apparel & Merch",
                "apparel-merch",
                "Branded apparel and everyday wearables.");

            dbContext.Categories.AddRange(electronicsCat, audioCat, apparelCat);

            var products = new[]
            {
                Product.Create(
                    Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    ProductSkus.LaptopPro15,
                    "Pro Laptop 15-inch",
                    "High-performance laptop with 15-inch Retina display and 32GB RAM",
                    Price.Create(1499.99m),
                    CurrencyCode.USD,
                    Weight.Create(2000m, WeightUnit.Gram),
                    PackageDimensions.Create(35m, 24m, 2m, LengthUnit.Centimeter),
                    electronicsCat.Id),

                Product.Create(
                    Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                    ProductSkus.Phone14Pro,
                    "Smartphone 14 Pro",
                    "Flagship smartphone with 120Hz OLED display and triple camera system",
                    Price.Create(999.00m),
                    CurrencyCode.USD,
                    Weight.Create(206m, WeightUnit.Gram),
                    PackageDimensions.Create(15m, 7.5m, 1m, LengthUnit.Centimeter),
                    electronicsCat.Id),

                Product.Create(
                    Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    ProductSkus.Monitor4k27,
                    "27-inch 4K Monitor",
                    "Ultra-sharp 4K UHD IPS display with USB-C connectivity",
                    Price.Create(449.50m),
                    CurrencyCode.USD,
                    Weight.Create(5500m, WeightUnit.Gram),
                    PackageDimensions.Create(61m, 36m, 5m, LengthUnit.Centimeter),
                    electronicsCat.Id),

                Product.Create(
                    Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                    ProductSkus.HeadphonesNc,
                    "Noise Cancelling Headphones",
                    "Over-ear wireless headphones with active noise cancellation",
                    Price.Create(249.99m),
                    CurrencyCode.USD,
                    Weight.Create(250m, WeightUnit.Gram),
                    PackageDimensions.Create(18m, 16m, 8m, LengthUnit.Centimeter),
                    audioCat.Id),

                Product.Create(
                    Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                    ProductSkus.KeyboardMech,
                    "Mechanical Gaming Keyboard",
                    "RGB mechanical keyboard with tactile switches and aluminum frame",
                    Price.Create(129.00m),
                    CurrencyCode.USD,
                    Weight.Create(950m, WeightUnit.Gram),
                    PackageDimensions.Create(44m, 13m, 4m, LengthUnit.Centimeter),
                    audioCat.Id),

                Product.Create(
                    Guid.Parse("a6666666-6666-6666-6666-666666666666"),
                    ProductSkus.ShirtBlkM,
                    "Classic Black T-Shirt (M)",
                    "100% organic cotton crewneck t-shirt in black",
                    Price.Create(29.99m),
                    CurrencyCode.USD,
                    Weight.Create(180m, WeightUnit.Gram),
                    PackageDimensions.Create(25m, 20m, 2m, LengthUnit.Centimeter),
                    apparelCat.Id)
            };

            dbContext.Products.AddRange(products);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Catalog categories and products seeded successfully ({Count} products).", products.Length);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding catalog data.");
            throw;
        }
    }
}
