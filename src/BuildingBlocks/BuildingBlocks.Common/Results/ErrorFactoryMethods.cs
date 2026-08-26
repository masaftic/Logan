namespace BuildingBlocks.Common.Results;

public static class ErrorFactoryMethods
{
    extension(Error)
    {
        public static Error NotFound(string code = "General.NotFound", string description = "Resource not found", Dictionary<string, object>? metadata = null)
            => new(ErrorType.NotFound, code, description, metadata);

        public static Error Conflict(string code = "General.Conflict", string description = "Resource conflict", Dictionary<string, object>? metadata = null)
            => new(ErrorType.Conflict, code, description, metadata);

        public static Error Validation(string field = "General.Validation", string description = "Validation error", Dictionary<string, object>? metadata = null)
            => new(ErrorType.Validation, field, description, metadata);

        public static Error Unauthorized(string code = "General.Unauthorized", string description = "Unauthorized access", Dictionary<string, object>? metadata = null)
            => new(ErrorType.Unauthorized, code, description, metadata);

        public static Error Forbidden(string code = "General.Forbidden", string description = "Forbidden access", Dictionary<string, object>? metadata = null)
            => new(ErrorType.Forbidden, code, description, metadata);

        public static Error ExternalService(string code = "External.Error", string description = "External service error", Dictionary<string, object>? metadata = null)
            => new(ErrorType.ExternalService, code, description, metadata);

        public static Error Unexpected(string code = "General.Unexpected", string description = "Unexpected error", Dictionary<string, object>? metadata = null)
            => new(ErrorType.Unexpected, code, description, metadata);

        public static Error Custom(string code, string description, Dictionary<string, object>? metadata = null, int? customStatusCode = null)
            => new(ErrorType.Custom, code, description, metadata, customStatusCode);
    }
}
