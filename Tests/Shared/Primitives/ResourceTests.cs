using Blueprint.Shared.Primitives;

namespace Tests.Shared.Primitives;

public class ResourceTests
{
    [Fact]
    public void Destroy_UsesDestroyableLifecycle()
    {
        TestResource resource = new();

        resource.Destroy();

        Assert.True(resource.Destroyed);
        Assert.Equal(1, resource.CleanupCount);
    }

    private sealed class TestResource : Resource
    {
        public int CleanupCount { get; private set; }

        protected override void OnDestroy() => CleanupCount++;
    }
}
