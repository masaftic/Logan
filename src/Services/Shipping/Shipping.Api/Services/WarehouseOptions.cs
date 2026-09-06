namespace Shipping.Api.Services;

public class WarehouseOptions
{
    public const string SectionName = "Warehouse";

    public string Name { get; set; } = "Logan Fulfillment Center";
    public string Street1 { get; set; } = "215 Clayton St.";
    public string City { get; set; } = "San Francisco";
    public string State { get; set; } = "CA";
    public string Zip { get; set; } = "94117";
    public string Country { get; set; } = "US";
    public string Phone { get; set; } = "+1 555 341 9393";
    public string Email { get; set; } = "warehouse@logan.com";
}
