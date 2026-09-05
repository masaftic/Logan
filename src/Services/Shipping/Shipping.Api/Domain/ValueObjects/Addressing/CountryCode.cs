using Thinktecture;

namespace Shipping.Api.Domain.ValueObjects;

[SmartEnum<string>]
public partial class CountryCode
{
    public static readonly CountryCode US = new("US", 5);
    public static readonly CountryCode DE = new("DE", 5);
    public static readonly CountryCode CH = new("CH", 4);
    public static readonly CountryCode CA = new("CA", 6);
    public static readonly CountryCode GB = new("GB", 0);

    public int PostalCodeLength { get; }
}
