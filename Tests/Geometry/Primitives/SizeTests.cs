using System.Numerics;
using Blueprint.Geometry.Primitives;

namespace Tests.Geometry.Primitives;

public class SizeTests
{
    [Fact]
    public void Zero_ReturnsZeroDimensions() =>
        Assert.Equal(new Size(Dimension.Zero, Dimension.Zero), Size.Zero);

    [Fact]
    public void FromOffset_CreatesAbsoluteDimensions()
    {
        Assert.Equal(
            new Size(Dimension.FromOffset(320), Dimension.FromOffset(240)),
            Size.FromOffset(320, 240)
        );
    }

    [Fact]
    public void FromScale_CreatesRelativeDimensions()
    {
        Assert.Equal(
            new Size(Dimension.FromScale(0.5f), Dimension.FromScale(0.25f)),
            Size.FromScale(0.5f, 0.25f)
        );
    }

    [Fact]
    public void Resolve_ResolvesDimensionsAgainstAvailableSize()
    {
        Size size = new(new Dimension(0.5f, 12), new Dimension(0.25f, 8));

        Assert.Equal(new Vector2(412, 158), size.Resolve(new Vector2(800, 600)));
    }

    [Fact]
    public void Addition_AddsDimensionsElementWise()
    {
        Size left = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));
        Size right = new(new Dimension(0.25f, 4), new Dimension(0.5f, 12));

        Assert.Equal(new Size(new Dimension(0.75f, 12), new Dimension(0.75f, 16)), left + right);
    }

    [Fact]
    public void Subtraction_SubtractsDimensionsElementWise()
    {
        Size left = new(new Dimension(0.5f, 8), new Dimension(0.75f, 12));
        Size right = new(new Dimension(0.25f, 4), new Dimension(0.5f, 8));

        Assert.Equal(new Size(new Dimension(0.25f, 4), new Dimension(0.25f, 4)), left - right);
    }

    [Fact]
    public void Multiplication_SupportsBothOperandOrders()
    {
        Size size = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(new Size(new Dimension(1.5f, 24), new Dimension(0.75f, 12)), size * 3);
        Assert.Equal(size * 3, 3 * size);
    }

    [Fact]
    public void Division_DividesDimensions()
    {
        Size size = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(new Size(new Dimension(0.25f, 4), new Dimension(0.125f, 2)), size / 2);
    }

    [Fact]
    public void UnaryOperators_PreserveOrNegateDimensions()
    {
        Size size = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(size, +size);
        Assert.Equal(new Size(new Dimension(-0.5f, -8), new Dimension(-0.25f, -4)), -size);
    }
}
