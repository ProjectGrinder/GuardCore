namespace GuardCore;

public readonly struct Result<TValue, TError> where TError : Enum
{
    private readonly TValue _value;
    private readonly TError _error;
    private readonly bool _isInitialized;
    private readonly bool _isSuccess;

    public TValue Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!IsSuccess)
                throw new InvalidOperationException("Cannot access Value of a failed or uninitialized Result container.");
            return _value;
        }
    }

    public TError Error
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Cannot access Error of an uninitialized Result container.");
            if (_isSuccess)
                throw new InvalidOperationException("Cannot access Error of a successful Result container.");
            return _error;
        }
    }

    public bool IsSuccess => _isInitialized && _isSuccess;
    public bool Failed => !IsSuccess;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result(TValue value)
    {
        _value = value;
        _error = default!;
        _isSuccess = true;
        _isInitialized = true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result(TError error)
    {
        _value = default!;
        _error = error;
        _isSuccess = false;
        _isInitialized = true;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<TError, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value) : onFailure(Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Match(Action<TValue> onSuccess, Action<TError> onFailure)
    {
        if (IsSuccess)
            onSuccess(_value);
        else
            onFailure(Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNewValue, TError> Map<TNewValue>(Func<TValue, TNewValue> selector)
    {
        return IsSuccess
            ? new Result<TNewValue, TError>(selector(_value))
            : new Result<TNewValue, TError>(Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TNewValue, TError> Bind<TNewValue>(Func<TValue, Result<TNewValue, TError>> next)
    {
        return IsSuccess ? next(_value) : new Result<TNewValue, TError>(Error);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TValue, TError> OnFailure(Action<TError> failureAction)
    {
        if (Failed) failureAction.Invoke(Error);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result<TValue, TError> OnSuccess(Action<TValue> successAction)
    {
        if (IsSuccess) successAction.Invoke(_value);
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Deconstruct(out bool isSuccess, out TValue value, out TError error)
    {
        isSuccess = IsSuccess;
        value = _value;
        error = _error;
    }
}
