using System.Numerics;
using Blueprint.Geometry.Primitives;

namespace Tests.Geometry.Primitives;

public class PositionTests
{
    [Fact]
    public void Zero_ReturnsOrigin() =>
        Assert.Equal(new Position(Dimension.Zero, Dimension.Zero), Position.Zero);

    [Fact]
    public void FromOffset_CreatesAbsoluteCoordinates()
    {
        Assert.Equal(
            new Position(Dimension.FromOffset(12), Dimension.FromOffset(24)),
            Position.FromOffset(12, 24)
        );
    }

    [Fact]
    public void FromScale_CreatesRelativeCoordinates()
    {
        Assert.Equal(
            new Position(Dimension.FromScale(0.5f), Dimension.FromScale(0.25f)),
            Position.FromScale(0.5f, 0.25f)
        );
    }

    [Fact]
    public void Resolve_ResolvesCoordinatesAgainstAvailableSize()
    {
        Position position = new(new Dimension(0.5f, 12), new Dimension(0.25f, 8));

        Assert.Equal(new Vector2(412, 158), position.Resolve(new Vector2(800, 600)));
    }

    [Fact]
    public void Addition_AddsCoordinatesElementWise()
    {
        Position left = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));
        Position right = new(new Dimension(0.25f, 4), new Dimension(0.5f, 12));

        Assert.Equal(
            new Position(new Dimension(0.75f, 12), new Dimension(0.75f, 16)),
            left + right
        );
    }

    [Fact]
    public void Subtraction_SubtractsCoordinatesElementWise()
    {
        Position left = new(new Dimension(0.5f, 8), new Dimension(0.75f, 12));
        Position right = new(new Dimension(0.25f, 4), new Dimension(0.5f, 8));

        Assert.Equal(new Position(new Dimension(0.25f, 4), new Dimension(0.25f, 4)), left - right);
    }

    [Fact]
    public void Multiplication_SupportsBothOperandOrders()
    {
        Position position = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(new Position(new Dimension(1.5f, 24), new Dimension(0.75f, 12)), position * 3);
        Assert.Equal(position * 3, 3 * position);
    }

    [Fact]
    public void Division_DividesCoordinates()
    {
        Position position = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(new Position(new Dimension(0.25f, 4), new Dimension(0.125f, 2)), position / 2);
    }

    [Fact]
    public void UnaryOperators_PreserveOrNegateCoordinates()
    {
        Position position = new(new Dimension(0.5f, 8), new Dimension(0.25f, 4));

        Assert.Equal(position, +position);
        Assert.Equal(new Position(new Dimension(-0.5f, -8), new Dimension(-0.25f, -4)), -position);
    }
}
