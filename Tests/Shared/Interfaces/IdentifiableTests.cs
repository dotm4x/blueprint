using Blueprint.Shared.Interfaces;

namespace Tests.Shared.Interfaces;

public class IdentifiableTests
{
    [Fact]
    public void Identifier_ReturnsImplementedIdentifier()
    {
        IIdentifiable<Guid> identifiable = new TestIdentifiable(Guid.NewGuid());

        Assert.Equal(((TestIdentifiable)identifiable).ExpectedIdentifier, identifiable.Identifier);
    }

    private sealed class TestIdentifiable(Guid identifier) : IIdentifiable<Guid>
    {
        public Guid ExpectedIdentifier { get; } = identifier;

        public Guid Identifier => ExpectedIdentifier;
    }
}
