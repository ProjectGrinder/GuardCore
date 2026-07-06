namespace GuardCore;

/// <summary>
/// Transforms the inner error type if the result is a failure.
/// Syntactic sugar compiling down to a zero-cost static call.
/// </summary>
public static class ResultExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Result<TValue, TNewError> MapError<TValue, TError, TNewError>(
        this Result<TValue, TError> result,
        Func<TError, TNewError> errorSelector)
        where TError : Enum
        where TNewError : Enum
    {
        return result.IsSuccess
            ? new Result<TValue, TNewError>(result.Value)
            : new Result<TValue, TNewError>(errorSelector(result.Error));
    }
}

