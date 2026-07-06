namespace GuardCore.Tests;

public class UnitTests
{
    [Fact]
    public void UnitDefault_ShouldAlwaysBeEqualAndReturnExpectedString()
    {
        var unit1 = Unit.Default;
        var unit2 = new Unit();

        (unit1 == unit2).ShouldBeTrue();
        (unit1 != unit2).ShouldBeFalse();
        unit1.Equals(unit2).ShouldBeTrue();
        unit1.GetHashCode().ShouldBe(0);
        unit1.ToString().ShouldBe("()");
        unit1.CompareTo(unit2).ShouldBe(0);
    }
}