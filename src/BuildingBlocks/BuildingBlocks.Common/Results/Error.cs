namespace BuildingBlocks.Common.Results;

public sealed record Error
{
    public ErrorType Type { get; init; }
    public string Code { get; init; }
    public string Description { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }

    public int? CustomStatusCode { get; init; }

    internal Error(
        ErrorType type,
        string code,
        string description,
        Dictionary<string, object>? metadata = null,
        int? customStatusCode = null)
    {
        Type = type;
        Code = code;
        Description = description;
        Metadata = metadata;
        CustomStatusCode = customStatusCode;
    }

    public Error WithMetadata(string key, object value)
    {
        var newMetadata = Metadata == null ? [] : new Dictionary<string, object>(Metadata);
        newMetadata[key] = value;
        return this with { Metadata = newMetadata };
    }
}

public enum ErrorType
{
    NotFound,
    Conflict,
    Validation,
    Unauthorized,
    Forbidden,
    ExternalService,
    Unexpected,
    Custom
}
