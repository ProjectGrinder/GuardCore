namespace Takayama.GuardCore.Tests;

public class EnsureTests
{
    [Fact]
    public void Ensure_WithStringMessage_WhenConditionFails_ShouldThrowInvalidOperationException()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            Guard.Ensure(false, "Database connection lost.");
        });

        exception.Message.ShouldBe("[CRITICAL] Database connection lost.");
    }

    [Fact]
    public void Ensure_WithLazyMessageFactory_WhenConditionFails_ShouldEvaluateFactoryAndThrow()
    {
        const uint errorCode = 503;

        var exception = Should.Throw<InvalidOperationException>(() =>
        {
            Guard.Ensure(false, () => $"Server returned fatal code: {errorCode}");
        });

        exception.Message.ShouldBe("[CRITICAL] Server returned fatal code: 503");
    }

    [Fact]
    public void Ensure_WhenConditionPasses_ShouldNotThrow()
    {
        Should.NotThrow(() => Guard.Ensure(true, "This should never throw"));
    }
}
