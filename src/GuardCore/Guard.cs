namespace GuardCore;

public static class Guard
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GuardState<TError> Expect<TError>(bool condition, TError errorIfFalse) where TError : Enum
    {
        return condition
            ? new GuardState<TError>(default!, true)
            : new GuardState<TError>(errorIfFalse, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(bool condition, string fatalMessage)
    {
        if (!condition) throw new InvalidOperationException($"[CRITICAL] {fatalMessage}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Ensure(bool condition, Func<string> lazyMessageFactory)
    {
        if (!condition)
            throw new InvalidOperationException($"[CRITICAL] {lazyMessageFactory()}");
    }
}

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct GuardState<TError>(TError error, bool isSuccess)
    where TError : Enum
{
    public TError Error { get; } = error;
    public bool IsSuccess { get; } = isSuccess;
    public bool Failed => !IsSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> And(bool condition, TError errorIfFalse)
    {
        if (Failed) return this;
        return condition ? this : new GuardState<TError>(errorIfFalse, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> And(Func<bool> conditionFactory, TError errorIfFalse)
    {
        if (Failed) return this;
        return conditionFactory() ? this : new GuardState<TError>(errorIfFalse, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> Or(bool condition, TError errorIfFalse)
    {
        if (IsSuccess) return this;
        return condition ? new GuardState<TError>(default!, true) : new GuardState<TError>(errorIfFalse, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> Or(Func<bool> conditionFactory, TError errorIfFalse)
    {
        if (IsSuccess) return this;
        return conditionFactory() ? new GuardState<TError>(default!, true) : new GuardState<TError>(errorIfFalse, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> Not(TError errorIfTrue)
    {
        return IsSuccess
            ? new GuardState<TError>(errorIfTrue, false)
            : new GuardState<TError>(default!, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator bool(GuardState<TError> state) => state.IsSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> OnFailure(Action<TError> failureAction)
    {
        if (Failed) failureAction.Invoke(Error);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GuardState<TError> OnSuccess(Action action)
    {
        if (IsSuccess) action.Invoke();
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Then<T>(T value)
    {
        return IsSuccess ? value : Error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<T, TError> Then<T>(Func<T> valueFactory)
    {
        return IsSuccess ? valueFactory() : Error;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out bool isSuccess, out TError error)
    {
        isSuccess = IsSuccess;
        error = Error;
    }
}

