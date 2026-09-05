using Thinktecture;

namespace Shipping.Api.Domain;

[ValueObject<Guid>]
public readonly partial struct ShipmentId
{
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value)
    {
        if (value == Guid.Empty)
        {
            validationError = new ValidationError("ShipmentId cannot be empty.");
        }
    }

    public static ShipmentId New() => Create(Guid.CreateVersion7());
}
