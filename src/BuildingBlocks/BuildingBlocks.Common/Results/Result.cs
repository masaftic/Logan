using System;

namespace BuildingBlocks.Common.Results;


public interface IAppResult
{
    bool IsSuccess { get; }
    bool IsError { get; }
    List<Error> Errors { get; }
    Error FirstError { get; }
}

public interface IAppResult<out T> : IAppResult
{
    T Value { get; }
}

public sealed class Result<T> : IAppResult<T>
{
    private readonly T? _value;
    private readonly List<Error>? _errors;

    private Result(T value) => _value = value;
    private Result(List<Error> errors) => _errors = errors;

    public bool IsSuccess => !IsError;
    public bool IsError => _errors is { Count: > 0 };
    public T Value => IsError ? throw new InvalidOperationException("Result is an error.") : _value!;
    public List<Error> Errors => _errors ?? [];
    public Error FirstError => _errors?[0] ?? throw new InvalidOperationException("No errors.");

    public static Result<T> Ok(T value) => new(value);
    public static Result<T> Fail(Error error) => new([error]);
    public static Result<T> Fail(List<Error> errors) => new(errors);

    public static implicit operator Result<T>(T value) => Ok(value);
    public static implicit operator Result<T>(Error error) => Fail(error);
    public static implicit operator Result<T>(List<Error> errors) => Fail(errors);

    // Monadic Match
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<List<Error>, TResult> onFailure)
        => IsError ? onFailure(Errors) : onSuccess(Value);

    // Monadic Map
    public Result<TNew> Map<TNew>(Func<T, TNew> mapper)
        => IsError ? Result<TNew>.Fail(Errors) : Result<TNew>.Ok(mapper(Value));

    // Monadic Bind
    public Result<TNew> Bind<TNew>(Func<T, Result<TNew>> binder)
        => IsError ? Result<TNew>.Fail(Errors) : binder(Value);
}

public sealed class Result : IAppResult
{
    private readonly List<Error>? _errors;

    private Result() { }
    private Result(List<Error> errors) => _errors = errors;

    public bool IsSuccess => !IsError;
    public bool IsError => _errors is { Count: > 0 };
    public List<Error> Errors => _errors ?? [];
    public Error FirstError => _errors?[0] ?? throw new InvalidOperationException("No errors.");

    public static Result Ok() => new();
    public static Result Fail(Error error) => new([error]);
    public static Result Fail(List<Error> errors) => new(errors);

    public static implicit operator Result(Error error) => Fail(error);
    public static implicit operator Result(List<Error> errors) => Fail(errors);

    // Monadic Match
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<List<Error>, TResult> onFailure)
        => IsError ? onFailure(Errors) : onSuccess();
}