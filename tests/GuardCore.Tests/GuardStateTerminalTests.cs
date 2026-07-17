namespace Takayama.GuardCore.Tests;

public class GuardStateTerminalTests
{
    [Fact]
    public void OnFailure_WhenStateHasFailed_ShouldExecuteActionNatively()
    {
        // Arrange
        var state = Guard.Expect(false, TestError.ValueTooLow);
        var actionExecuted = false;
        var capturedError = TestError.None;

        state.OnFailure(err =>
        {
            actionExecuted = true;
            capturedError = err;
        });

        actionExecuted.ShouldBeTrue();
        capturedError.ShouldBe(TestError.ValueTooLow);
    }

    [Fact]
    public void OnSuccess_WhenStateHasFailed_ShouldSkipActionExecution()
    {
        var state = Guard.Expect(false, TestError.ValueTooLow);
        var actionExecuted = false;

        state.OnSuccess(() => actionExecuted = true);
        actionExecuted.ShouldBeFalse();
    }

    [Fact]
    public void Then_WithValFactory_WhenSuccess_ShouldReturnSuccessResultWithValue()
    {
        var state = Guard.Expect(true, TestError.None);
        var result = state.Then(() => "HighPerformanceData Pipeline");

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("HighPerformanceData Pipeline");
    }

    [Fact]
    public void Then_WithValFactory_WhenFailed_ShouldReturnFailureResultWithError()
    {
        var state = Guard.Expect(false, TestError.ValueTooLow);
        var result = state.Then(() => "Should Not Be Evaluated");

        result.Failed.ShouldBeTrue();
        result.Error.ShouldBe(TestError.ValueTooLow);
    }

    [Fact]
    public void ToResult_WhenSuccess_ShouldReturnUnitSuccessResult()
    {
        var state = Guard.Expect(true, TestError.None);
        var result = state.ToResult();

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(Unit.Default);
    }
}
