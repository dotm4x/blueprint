using Blueprint.Geometry.Primitives;

namespace Tests.Geometry.Primitives;

public class ThicknessTests
{
    [Fact]
    public void Zero_ReturnsZeroForEverySide()
    {
        Assert.Equal(
            new Thickness(Dimension.Zero, Dimension.Zero, Dimension.Zero, Dimension.Zero),
            Thickness.Zero
        );
    }

    [Fact]
    public void UniformConstructor_AppliesDimensionToEverySide()
    {
        Dimension dimension = new(0.5f, 8);

        Assert.Equal(
            new Thickness(dimension, dimension, dimension, dimension),
            new Thickness(dimension)
        );
    }

    [Fact]
    public void HorizontalAndVerticalConstructor_AppliesDimensionsToCorrectSides()
    {
        Dimension horizontal = new(0.5f, 8);
        Dimension vertical = new(0.25f, 4);

        Assert.Equal(
            new Thickness(vertical, horizontal, vertical, horizontal),
            new Thickness(horizontal, vertical)
        );
    }
}
