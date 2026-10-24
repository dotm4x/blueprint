using Blueprint.Shared.Primitives;

namespace Tests.Shared.Primitives;

public class UnitTests
{
    [Fact]
    public void Value_IsTheOnlyValue()
    {
        Assert.Equal(Unit.Value, new Unit());
        Assert.Equal(Unit.Value, Unit.Value);
    }
}
