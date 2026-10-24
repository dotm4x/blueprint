using Blueprint.Geometry.Primitives;

namespace Tests.Geometry.Primitives;

public class DimensionTests
{
    [Fact]
    public void Zero_ReturnsZeroScaleAndOffset() =>
        Assert.Equal(new Dimension(0, 0), Dimension.Zero);

    [Theory]
    [InlineData(12, 0, 12)]
    [InlineData(-4, 0, -4)]
    public void FromOffset_UsesAbsoluteOffset(
        float offset,
        float expectedScale,
        float expectedOffset
    ) => Assert.Equal(new Dimension(expectedScale, expectedOffset), Dimension.FromOffset(offset));

    [Theory]
    [InlineData(0.5f, 0.5f, 0)]
    [InlineData(-2, -2, 0)]
    public void FromScale_UsesRelativeScale(
        float scale,
        float expectedScale,
        float expectedOffset
    ) => Assert.Equal(new Dimension(expectedScale, expectedOffset), Dimension.FromScale(scale));

    [Fact]
    public void Resolve_CombinesScaledAvailableSizeAndOffset()
    {
        Dimension dimension = new(0.5f, 24);

        Assert.Equal(424, dimension.Resolve(800));
    }

    [Fact]
    public void Addition_CombinesScaleAndOffset() =>
        Assert.Equal(new Dimension(0.75f, 12), new Dimension(0.5f, 8) + new Dimension(0.25f, 4));

    [Fact]
    public void Subtraction_SubtractsScaleAndOffset() =>
        Assert.Equal(new Dimension(0.25f, 4), new Dimension(0.5f, 8) - new Dimension(0.25f, 4));

    [Fact]
    public void Multiplication_SupportsBothOperandOrders()
    {
        Dimension dimension = new(0.5f, 8);

        Assert.Equal(new Dimension(1.5f, 24), dimension * 3);
        Assert.Equal(dimension * 3, 3 * dimension);
    }

    [Fact]
    public void Division_DividesScaleAndOffset() =>
        Assert.Equal(new Dimension(0.25f, 4), new Dimension(0.5f, 8) / 2);

    [Fact]
    public void Division_ByZero_Throws() =>
        Assert.Throws<DivideByZeroException>(() => new Dimension(1, 2) / 0);

    [Fact]
    public void UnaryOperators_PreserveOrNegateScaleAndOffset()
    {
        Dimension dimension = new(0.5f, 8);

        Assert.Equal(dimension, +dimension);
        Assert.Equal(new Dimension(-0.5f, -8), -dimension);
    }
}
