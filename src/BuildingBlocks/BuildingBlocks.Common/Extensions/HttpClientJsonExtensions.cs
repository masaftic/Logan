using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Common.Results;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Common.Extensions;

public static class HttpClientJsonExtensions
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<Result<TResponse>> PostAsJsonResultAsync<TRequest, TResponse>(
        this HttpClient httpClient,
        string requestUri,
        TRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, request, DefaultJsonOptions, ct);
            return await ReadResultAsync<TResponse>(response, ct);
        }
        catch (Exception ex)
        {
            return Error.ExternalService("Http.Unavailable", $"Request to '{requestUri}' failed: {ex.Message}");
        }
    }

    public static async Task<Result> PostAsJsonResultAsync<TRequest>(
        this HttpClient httpClient,
        string requestUri,
        TRequest request,
        CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.PostAsJsonAsync(requestUri, request, DefaultJsonOptions, ct);
            return await ReadResultAsync(response, ct);
        }
        catch (Exception ex)
        {
            return Error.ExternalService("Http.Unavailable", $"Request to '{requestUri}' failed: {ex.Message}");
        }
    }

    public static async Task<Result<TResponse>> GetFromJsonResultAsync<TResponse>(
        this HttpClient httpClient,
        string requestUri,
        CancellationToken ct = default)
    {
        try
        {
            var response = await httpClient.GetAsync(requestUri, ct);
            return await ReadResultAsync<TResponse>(response, ct);
        }
        catch (Exception ex)
        {
            return Error.ExternalService("Http.Unavailable", $"Request to '{requestUri}' failed: {ex.Message}");
        }
    }

    private static async Task<Result<TResponse>> ReadResultAsync<TResponse>(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            var data = await response.Content.ReadFromJsonAsync<TResponse>(DefaultJsonOptions, ct);
            return data is not null
                ? Result<TResponse>.Ok(data)
                : Error.Unexpected("Http.NullPayload", "Received null response payload.");
        }

        var error = await ParseProblemDetailsAsync(response, ct);
        return error;
    }

    private static async Task<Result> ReadResultAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return Result.Ok();
        }

        var error = await ParseProblemDetailsAsync(response, ct);
        return error;
    }

    private static async Task<Error> ParseProblemDetailsAsync(
        HttpResponseMessage response,
        CancellationToken ct)
    {
        try
        {
            var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(DefaultJsonOptions, ct);
            if (problem is not null)
            {
                var detail = problem.Detail ?? problem.Title ?? $"Request failed with status {(int)response.StatusCode}.";
                var code = problem.Extensions.TryGetValue("code", out var codeObj) && codeObj is not null
                    ? codeObj.ToString()!
                    : $"Http.{(int)response.StatusCode}";

                return response.StatusCode switch
                {
                    HttpStatusCode.NotFound => Error.NotFound(code, detail),
                    HttpStatusCode.Conflict => Error.Conflict(code, detail),
                    HttpStatusCode.BadRequest => Error.Validation(code, detail),
                    HttpStatusCode.Unauthorized => Error.Unauthorized(code, detail),
                    HttpStatusCode.Forbidden => Error.Forbidden(code, detail),
                    _ => Error.ExternalService(code, detail)
                };
            }
        }
        catch
        {
            // Fallback if response is not valid JSON or ProblemDetails
        }

        var rawContent = await response.Content.ReadAsStringAsync(ct);
        var fallbackDetail = !string.IsNullOrWhiteSpace(rawContent)
            ? rawContent
            : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";

        return Error.ExternalService($"Http.{(int)response.StatusCode}", fallbackDetail);
    }
}
