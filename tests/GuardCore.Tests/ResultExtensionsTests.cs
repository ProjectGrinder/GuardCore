namespace GuardCore.Tests;

public class ResultExtensionsTests
{
    [Fact]
    public void MapError_WhenResultIsFailure_ShouldTransformErrorTypeNatively()
    {
        Result<string, TestError> failureResult = TestError.ValueTooHigh;
        Result<string, AlternateError> transformed = failureResult.MapError(err =>
            err == TestError.ValueTooHigh 
                ? AlternateError.TranslatedFault 
                : AlternateError.None);

        transformed.Failed.ShouldBeTrue();
        transformed.Error.ShouldBe(AlternateError.TranslatedFault);
    }

    [Fact]
    public void MapError_WhenResultIsSuccess_ShouldForwardValueUnchanged()
    {
        Result<string, TestError> successResult = "PreserveMe";
        var transformed = successResult.MapError(_ => AlternateError.TranslatedFault);

        transformed.IsSuccess.ShouldBeTrue();
        transformed.Value.ShouldBe("PreserveMe");
    }
}