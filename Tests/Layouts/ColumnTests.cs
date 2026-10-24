using System.Numerics;
using Blueprint.Elements;
using Blueprint.Geometry.Primitives;
using Blueprint.Layouts;

namespace Tests.Layouts;

public class ColumnTests
{
    [Fact]
    public void Measure_CombinesChildHeightsAndMaximumWidth()
    {
        Column column = new() { Spacing = Dimension.FromOffset(5) };
        LayoutContext context = CreateContext(CreateElement(20, 10), CreateElement(30, 15));

        Assert.Equal(new Vector2(30, 30), column.Measure(context, new Vector2(100, 80)));
    }

    [Fact]
    public void Measure_IncludesMarginsAndResolvesSpacingAgainstContentHeight()
    {
        Column column = new()
        {
            Margin = new Thickness(
                Dimension.FromOffset(4),
                Dimension.FromOffset(10),
                Dimension.FromOffset(6),
                Dimension.FromOffset(2)
            ),
            Spacing = Dimension.FromScale(0.1f),
        };
        LayoutContext context = CreateContext(CreateElement(20, 10), CreateElement(30, 15));

        Assert.Equal(new Vector2(42, 44), column.Measure(context, new Vector2(200, 100)));
    }

    [Fact]
    public void Measure_EmptyContextReturnsMargins()
    {
        Column column = new() { Margin = new Thickness(Dimension.FromOffset(5)) };

        Assert.Equal(new Vector2(10, 10), column.Measure(CreateContext(), new Vector2(100, 80)));
    }

    [Fact]
    public void Arrange_EmptyContextReturnsNoArrangements()
    {
        Column column = new();

        Assert.Empty(column.Arrange(CreateContext(), new Vector2(100, 80)));
    }

    [Theory]
    [InlineData(Distribution.Start, 0, 15)]
    [InlineData(Distribution.Center, 37.5f, 52.5f)]
    [InlineData(Distribution.End, 75, 90)]
    [InlineData(Distribution.SpaceBetween, 0, 90)]
    [InlineData(Distribution.SpaceAround, 18.75f, 71.25f)]
    [InlineData(Distribution.SpaceEvenly, 25, 65)]
    public void Arrange_DistributionPlacesChildrenAlongMainAxis(
        Distribution distribution,
        float firstY,
        float secondY
    )
    {
        Column column = new() { Spacing = Dimension.FromOffset(5), Distribution = distribution };
        LayoutContext context = CreateContext(CreateElement(10, 10), CreateElement(10, 10));

        IReadOnlyList<LayoutArrangement> arrangements = column.Arrange(
            context,
            new Vector2(80, 100)
        );

        Assert.Equal(firstY, arrangements[0].Result.Position.Y, 3);
        Assert.Equal(secondY, arrangements[1].Result.Position.Y, 3);
    }

    [Theory]
    [InlineData(Alignment.Start, 0, 20)]
    [InlineData(Alignment.Center, 40, 20)]
    [InlineData(Alignment.End, 80, 20)]
    [InlineData(Alignment.Stretch, 0, 100)]
    public void Arrange_AlignmentPlacesOrStretchesChildrenOnCrossAxis(
        Alignment alignment,
        float expectedX,
        float expectedWidth
    )
    {
        Column column = new() { Alignment = alignment };
        LayoutContext context = CreateContext(CreateElement(20, 10));

        LayoutArrangement arrangement = Assert.Single(
            column.Arrange(context, new Vector2(100, 80))
        );

        Assert.Equal(expectedX, arrangement.Result.Position.X);
        Assert.Equal(expectedWidth, arrangement.Result.Size.X);
    }

    [Fact]
    public void Arrange_MarginsAndOriginDetermineResolvedGeometry()
    {
        Column column = new()
        {
            Margin = new Thickness(
                Dimension.FromOffset(5),
                Dimension.FromOffset(10),
                Dimension.FromOffset(15),
                Dimension.FromOffset(20)
            ),
        };
        Element child = CreateElement(30, 20);
        child.Origin.Value = new Vector2(0.5f, 0.5f);

        LayoutArrangement arrangement = Assert.Single(
            column.Arrange(CreateContext(child), new Vector2(100, 80))
        );

        Assert.Equal(new Vector2(35, 15), arrangement.Result.Position);
        Assert.Equal(new Vector2(30, 20), arrangement.Result.Size);
        Assert.Equal(new Vector2(-15, -10), arrangement.Result.OriginOffset);
    }

    [Fact]
    public void Arrange_StretchRespectsChildMaximumSize()
    {
        Column column = new() { Alignment = Alignment.Stretch };
        Element child = CreateElement(10, 20);
        child.MaximumSize.Value = Size.FromOffset(30, 25);

        LayoutArrangement arrangement = Assert.Single(
            column.Arrange(CreateContext(child), new Vector2(100, 80))
        );

        Assert.Equal(new Vector2(30, 20), arrangement.Result.Size);
    }

    private static Element CreateElement(float width, float height)
    {
        Element element = new();
        element.Size.Value = Size.FromOffset(width, height);
        return element;
    }

    private static LayoutContext CreateContext(params Element[] elements) =>
        new(new Element(), elements);
}
