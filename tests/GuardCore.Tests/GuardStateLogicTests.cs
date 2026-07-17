namespace Takayama.GuardCore.Tests;

public class GuardStateLogicTests
{
    [Fact]
    public void Expect_WhenConditionIsTrue_ShouldImplicitlyEvaluateAsTrue()
    {
        var state = Guard.Expect(true, TestError.InvalidId);
        bool success = state;
        success.ShouldBeTrue();
        state.Failed.ShouldBeFalse();
    }

    [Fact]
    public void And_WithFailedPriorState_ShouldShortCircuitAndPreserveOriginalError()
    {
        var state = Guard.Expect(false, TestError.ValueTooLow);
        var finalState = state.And(() => throw new Exception("Should short circuit!"), TestError.ValueTooHigh);

        finalState.IsSuccess.ShouldBeFalse();
        finalState.Error.ShouldBe(TestError.ValueTooLow);
    }

    [Fact]
    public void Or_WithFailedPriorState_WhenSecondaryConditionPasses_ShouldRecoverToSuccess()
    {
        var state = Guard.Expect(false, TestError.ValueTooLow);
        var recoveredState = state.Or(true, TestError.ValueTooHigh);
        recoveredState.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public void Not_ShouldInvertValidationStateCleanly()
    {
        var successState = Guard.Expect(true, TestError.InvalidId);
        var invertedState = successState.Not(TestError.ValueTooHigh);

        invertedState.IsSuccess.ShouldBeFalse();
        invertedState.Error.ShouldBe(TestError.ValueTooHigh);
    }

    [Fact]
    public void Deconstruct_ShouldUnpackInternalStateFieldsCorrectly()
    {
        var state = Guard.Expect(false, TestError.InvalidId);
        var (isSuccess, error) = state;

        isSuccess.ShouldBeFalse();
        error.ShouldBe(TestError.InvalidId);
    }
}
