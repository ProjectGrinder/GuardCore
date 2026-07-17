namespace Takayama.GuardCore.Tests;

public enum AlternateError
{
    None = 0,
    TranslatedFault,
    UpstreamTimeout
}

public class ResultTests
{
    [Fact]
    public void UninitializedResult_ShouldBeMarkedAsFailedAndThrowOnAccess()
    {
        Result<int, TestError> uninitialized = default;

        uninitialized.IsSuccess.ShouldBeFalse();
        uninitialized.Failed.ShouldBeTrue();

        Should.Throw<InvalidOperationException>(() => { var _ = uninitialized.Value; })
            .Message.ShouldBe("Cannot access Value of a failed or uninitialized Result container.");

        Should.Throw<InvalidOperationException>(() => { var _ = uninitialized.Error; })
            .Message.ShouldBe("Cannot access Error of an uninitialized Result container.");
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateValidSuccessResult()
    {
        Result<string, TestError> result = "SystemNormalized";
        result.IsSuccess.ShouldBeTrue();
        result.Failed.ShouldBeFalse();
        result.Value.ShouldBe("SystemNormalized");

        Should.Throw<InvalidOperationException>(() => { var _ = result.Error; })
            .Message.ShouldBe("Cannot access Error of a successful Result container.");
    }

    [Fact]
    public void ImplicitConversion_FromEnum_ShouldCreateValidFailureResult()
    {
        Result<int, TestError> result = TestError.InvalidId;
        result.IsSuccess.ShouldBeFalse();
        result.Failed.ShouldBeTrue();
        result.Error.ShouldBe(TestError.InvalidId);
    }

    [Fact]
    public void Match_WithFunc_ShouldExecuteCorrectLogicalBranch()
    {
        Result<int, TestError> successTarget = 200;
        Result<int, TestError> failureTarget = TestError.ValueTooLow;

        string successPath = successTarget.Match(val => $"Ok:{val}", _ => "Err");
        string failurePath = failureTarget.Match(_ => "Ok", err => $"Fail:{err}");

        successPath.ShouldBe("Ok:200");
        failurePath.ShouldBe("Fail:ValueTooLow");
    }

    [Fact]
    public void Map_WhenSuccess_ShouldTransformInnerValueTypeNatively()
    {
        Result<int, TestError> initial = 50;
        var mapped = initial.Map(val => val * 2);
        mapped.IsSuccess.ShouldBeTrue();
        mapped.Value.ShouldBe(100);
    }

    [Fact]
    public void Bind_WhenSuccess_ShouldChainMonadicOperationsWithoutAllocating()
    {
        Result<string, TestError> initial = "100";

        var bound = initial.Bind(val =>
            int.TryParse(val, out int cleanInt)
                ? new Result<int, TestError>(cleanInt)
                : new Result<int, TestError>(TestError.FallbackError));

        bound.IsSuccess.ShouldBeTrue();
        bound.Value.ShouldBe(100);
    }

    [Fact]
    public void OnSuccessAndOnFailure_ShouldExecuteConditionalSideEffects()
    {
        Result<int, TestError> successState = 42;
        bool successExecuted = false;
        bool failureExecuted = false;

        successState
            .OnSuccess(_ => successExecuted = true)
            .OnFailure(_ => failureExecuted = true);

        successExecuted.ShouldBeTrue();
        failureExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Deconstruct_ShouldUnpackInternalStateNatively()
    {
        Result<string, TestError> result = "DataStream";
        var (isSuccess, value, _) = result;
        isSuccess.ShouldBeTrue();
        value.ShouldBe("DataStream");
    }
}
