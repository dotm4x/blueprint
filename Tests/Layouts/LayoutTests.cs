using System.Numerics;
using Blueprint.Elements;
using Blueprint.Geometry.Primitives;
using Blueprint.Layouts;

namespace Tests.Layouts;

public class LayoutTests
{
    [Fact]
    public void Defaults_UseZeroSpacingMarginAndStartPlacement()
    {
        TestLayout layout = new();

        Assert.Equal(Thickness.Zero, layout.Margin);
        Assert.Equal(Dimension.Zero, layout.Spacing);
        Assert.Equal(Alignment.Start, layout.Alignment);
        Assert.Equal(Distribution.Start, layout.Distribution);
    }

    [Fact]
    public void ResolveChildSize_ClampsNegativeDimensionsToZero()
    {
        TestLayout layout = new();
        Element child = CreateElement(Size.FromOffset(-10, 20));

        Assert.Equal(Vector2.UnitY * 20, layout.ResolveSize(child, new Vector2(100, 100)));
    }

    [Fact]
    public void ResolveChildSize_AppliesMinimumAndMaximumConstraints()
    {
        TestLayout layout = new();
        Element child = CreateElement(Size.FromOffset(100, 10));
        child.MinimumSize.Value = Size.FromOffset(40, 30);
        child.MaximumSize.Value = Size.FromOffset(60, 25);

        Assert.Equal(new Vector2(60, 30), layout.ResolveSize(child, new Vector2(100, 100)));
    }

    [Fact]
    public void ResolveChildSize_UsesPreferredSizeBeforeApplyingConstraints()
    {
        TestLayout layout = new();
        Element child = CreateElement(Size.FromOffset(10, 10));
        child.MinimumSize.Value = Size.FromOffset(20, 15);

        Assert.Equal(
            new Vector2(20, 25),
            layout.ResolveSize(child, new Vector2(100, 100), new Vector2(5, 25))
        );
    }

    [Fact]
    public void ResolveChildSize_PreservesAspectRatioWithinConstraints()
    {
        TestLayout layout = new();
        Element child = CreateElement(Size.FromOffset(100, 20));
        child.MinimumSize.Value = Size.FromOffset(30, 20);
        child.MaximumSize.Value = Size.FromOffset(80, 50);
        child.AspectRatio.Value = 2;

        Assert.Equal(new Vector2(72, 36), layout.ResolveSize(child, new Vector2(100, 100)));
    }

    [Fact]
    public void ResolveMargin_ResolvesEachSideAgainstItsAxis()
    {
        TestLayout layout = new()
        {
            Margin = new Thickness(
                Dimension.FromScale(0.1f),
                Dimension.FromOffset(4),
                Dimension.FromScale(0.2f),
                Dimension.FromOffset(6)
            ),
        };

        Assert.Equal(new Vector2(10, 30), layout.ResolveMargins(new Vector2(100, 100)));
    }

    [Fact]
    public void ResolveGeometry_OffsetsPositionByElementOrigin()
    {
        TestLayout layout = new();
        Element child = CreateElement(Size.FromOffset(40, 20));
        child.Origin.Value = new Vector2(0.5f, 1);

        Assert.Equal(
            new LayoutResult(new Vector2(70, 80), new Vector2(40, 20), new Vector2(-20, -20)),
            layout.ResolveElementGeometry(child, new Vector2(50, 60), new Vector2(40, 20))
        );
    }

    private static Element CreateElement(Size size)
    {
        Element element = new();
        element.Size.Value = size;
        return element;
    }

    private sealed class TestLayout : Layout
    {
        public Vector2 ResolveSize(Element element, Vector2 availableSize, Vector2? preferredSize = null) =>
            ResolveChildSize(element, availableSize, preferredSize);

        public Vector2 ResolveMargins(Vector2 availableSize) => ResolveMargin(availableSize);

        public LayoutResult ResolveElementGeometry(Element element, Vector2 position, Vector2 size) =>
            ResolveGeometry(element, position, size);

        public override Vector2 Measure(LayoutContext context, Vector2 availableSize) => availableSize;

        public override IReadOnlyList<LayoutArrangement> Arrange(LayoutContext context, Vector2 finalSize) =>
            [];
    }
}
