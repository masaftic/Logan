using BuildingBlocks.Common.ValueObjects;

namespace BuildingBlocks.Common.Constants;

public static class ProductSkus
{
    public static readonly Sku LaptopPro15 = Sku.Create("LAPTOP-PRO-15");
    public static readonly Sku Phone14Pro = Sku.Create("PHONE-14-PRO");
    public static readonly Sku Monitor4k27 = Sku.Create("MONITOR-4K-27");
    public static readonly Sku HeadphonesNc = Sku.Create("HEADPHONES-NC");
    public static readonly Sku KeyboardMech = Sku.Create("KEYBOARD-MECH");
    public static readonly Sku ShirtBlkM = Sku.Create("SHIRT-BLK-M");

    public static readonly IReadOnlyList<Sku> All =
    [
        LaptopPro15,
        Phone14Pro,
        Monitor4k27,
        HeadphonesNc,
        KeyboardMech,
        ShirtBlkM
    ];
}
