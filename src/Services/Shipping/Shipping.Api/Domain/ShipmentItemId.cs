using Thinktecture;

namespace Shipping.Api.Domain;

[ValueObject<Guid>]
public readonly partial struct ShipmentItemId
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value)
    {
        if (value == Guid.Empty)
        {
            validationError = new ValidationError("ShipmentItemId cannot be empty.");
        }
    }

    public static ShipmentItemId New() => Create(Guid.CreateVersion7());
}
