using System.Numerics;
using Blueprint.Elements;
using Blueprint.Geometry.Primitives;
using Blueprint.Layouts;

namespace Tests.Layouts;

public class RowTests
{
    [Fact]
    public void Measure_CombinesChildWidthsAndMaximumHeight()
    {
        Row row = new() { Spacing = Dimension.FromOffset(5) };
        LayoutContext context = CreateContext(CreateElement(20, 10), CreateElement(30, 15));

        Assert.Equal(new Vector2(55, 15), row.Measure(context, new Vector2(100, 80)));
    }

    [Fact]
    public void Measure_IncludesMarginsAndResolvesSpacingAgainstContentWidth()
    {
        Row row = new()
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

        Assert.Equal(new Vector2(80.8f, 25), row.Measure(context, new Vector2(200, 100)));
    }

    [Fact]
    public void Measure_EmptyContextReturnsMargins()
    {
        Row row = new() { Margin = new Thickness(Dimension.FromOffset(5)) };

        Assert.Equal(new Vector2(10, 10), row.Measure(CreateContext(), new Vector2(100, 80)));
    }

    [Fact]
    public void Arrange_EmptyContextReturnsNoArrangements()
    {
        Row row = new();

        Assert.Empty(row.Arrange(CreateContext(), new Vector2(100, 80)));
    }

    [Theory]
    [InlineData(Distribution.Start, 0, 25)]
    [InlineData(Distribution.Center, 27.5f, 52.5f)]
    [InlineData(Distribution.End, 55, 80)]
    [InlineData(Distribution.SpaceBetween, 0, 80)]
    [InlineData(Distribution.SpaceAround, 13.75f, 66.25f)]
    [InlineData(Distribution.SpaceEvenly, 55f / 3, 55f / 3 * 2 + 25)]
    public void Arrange_DistributionPlacesChildrenAlongMainAxis(
        Distribution distribution,
        float firstX,
        float secondX
    )
    {
        Row row = new() { Spacing = Dimension.FromOffset(5), Distribution = distribution };
        LayoutContext context = CreateContext(CreateElement(20, 10), CreateElement(20, 10));

        IReadOnlyList<LayoutArrangement> arrangements = row.Arrange(context, new Vector2(100, 80));

        Assert.Equal(firstX, arrangements[0].Result.Position.X, 3);
        Assert.Equal(secondX, arrangements[1].Result.Position.X, 3);
    }

    [Theory]
    [InlineData(Alignment.Start, 0, 10)]
    [InlineData(Alignment.Center, 35, 10)]
    [InlineData(Alignment.End, 70, 10)]
    [InlineData(Alignment.Stretch, 0, 80)]
    public void Arrange_AlignmentPlacesOrStretchesChildrenOnCrossAxis(
        Alignment alignment,
        float expectedY,
        float expectedHeight
    )
    {
        Row row = new() { Alignment = alignment };
        LayoutContext context = CreateContext(CreateElement(20, 10));

        LayoutArrangement arrangement = Assert.Single(row.Arrange(context, new Vector2(100, 80)));

        Assert.Equal(expectedY, arrangement.Result.Position.Y);
        Assert.Equal(expectedHeight, arrangement.Result.Size.Y);
    }

    [Fact]
    public void Arrange_MarginsAndOriginDetermineResolvedGeometry()
    {
        Row row = new()
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
            row.Arrange(CreateContext(child), new Vector2(100, 80))
        );

        Assert.Equal(new Vector2(35, 15), arrangement.Result.Position);
        Assert.Equal(new Vector2(30, 20), arrangement.Result.Size);
        Assert.Equal(new Vector2(-15, -10), arrangement.Result.OriginOffset);
    }

    [Fact]
    public void Arrange_StretchRespectsChildMaximumSize()
    {
        Row row = new() { Alignment = Alignment.Stretch };
        Element child = CreateElement(20, 10);
        child.MaximumSize.Value = Size.FromOffset(25, 30);

        LayoutArrangement arrangement = Assert.Single(
            row.Arrange(CreateContext(child), new Vector2(100, 80))
        );

        Assert.Equal(new Vector2(20, 30), arrangement.Result.Size);
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
