using BuildingBlocks.Common.Constants;
using BuildingBlocks.Common.ValueObjects;
using Inventory.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data.Seeders;

public static class InventoryDataSeeder
{
    public static async Task SeedInventoryAsync(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<InventoryDbContext>>();
        var dbContext = services.GetRequiredService<InventoryDbContext>();

        try
        {
            if (await dbContext.StockItems.AnyAsync())
            {
                logger.LogInformation("Inventory database already seeded. Skipping initial seeding.");
                return;
            }

            logger.LogInformation("Seeding initial stock items into inventory database...");

            var seedItems = new[]
            {
                (Sku: ProductSkus.LaptopPro15, Name: "Pro Laptop 15-inch", Qty: 100),
                (Sku: ProductSkus.Phone14Pro, Name: "Smartphone 14 Pro", Qty: 50),
                (Sku: ProductSkus.HeadphonesNc, Name: "Noise Cancelling Headphones", Qty: 20),
                (Sku: ProductSkus.Monitor4k27, Name: "27-inch 4K Monitor", Qty: 10),
                (Sku: ProductSkus.KeyboardMech, Name: "Mechanical Gaming Keyboard", Qty: 5),
                (Sku: ProductSkus.ShirtBlkM, Name: "Classic Black T-Shirt (M)", Qty: 50)
            };

            foreach (var (Sku, Name, Qty) in seedItems)
            {
                var stockItem = StockItem.Create(Sku, Name, Quantity.Create(Qty));
                dbContext.StockItems.Add(stockItem);

                var restockMovement = StockMovement.Create(
                    stockItem.Sku,
                    quantityDelta: Qty,
                    availableAfter: stockItem.QuantityAvailable,
                    reservedAfter: stockItem.QuantityReserved,
                    type: Domain.Enums.StockMovementType.Restock,
                    referenceId: "INITIAL_SEED"
                );
                dbContext.StockMovements.Add(restockMovement);
            }

            await dbContext.SaveChangesAsync();
            logger.LogInformation("Initial stock items seeded successfully.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding initial inventory data.");
            throw;
        }
    }
}

