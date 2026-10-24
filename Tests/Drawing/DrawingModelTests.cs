using System.Numerics;
using Blueprint.Drawing;
using Blueprint.Drawing.Brushes;
using Blueprint.Drawing.Commands;
using Blueprint.Drawing.Primitives;
using Blueprint.Geometry.Primitives;

namespace Tests.Drawing;

public class DrawingModelTests
{
    [Fact]
    public void DrawBounds_StoresResolvedGeometry()
    {
        DrawBounds bounds = new(new Vector2(10, 20), new Vector2(30, 40), Rotation.FromDegrees(45));

        Assert.Equal(new Vector2(10, 20), bounds.Position);
        Assert.Equal(new Vector2(30, 40), bounds.Size);
        Assert.Equal(Rotation.FromDegrees(45), bounds.Rotation);
    }

    [Fact]
    public void DrawContext_StoresBoundsAndSurfaceSize()
    {
        DrawBounds bounds = new(Vector2.One, new Vector2(20, 30), Rotation.Zero);
        Size surfaceSize = Size.FromOffset(800, 600);
        DrawContext context = new(bounds, surfaceSize);

        Assert.Same(bounds, context.Bounds);
        Assert.Equal(surfaceSize, context.SurfaceSize);
    }

    [Fact]
    public void Pen_StoresColorAndDefaultsThicknessToOne()
    {
        Pen pen = new(Color.Red);

        Assert.Equal(Color.Red, pen.Color);
        Assert.Equal(1, pen.Thickness);
    }

    [Fact]
    public void SolidBrush_StoresFillColor()
    {
        SolidBrush brush = new(Color.Blue);

        Assert.Equal(Color.Blue, brush.Color);
    }

    [Fact]
    public void DrawingCommands_StoreTheirArguments()
    {
        Position start = Position.FromOffset(1, 2);
        Position end = Position.FromOffset(3, 4);
        Position position = Position.FromOffset(5, 6);
        Size size = Size.FromOffset(7, 8);
        Pen pen = new(Color.Green, 2);
        SolidBrush brush = new(Color.Yellow);

        Assert.Equal(new DrawLineCommand(start, end, pen), new DrawLineCommand(start, end, pen));
        Assert.Equal(
            new FillRectangleCommand(position, size, brush),
            new FillRectangleCommand(position, size, brush)
        );
        Assert.Equal(
            new FillEllipseCommand(position, size, brush),
            new FillEllipseCommand(position, size, brush)
        );
        Assert.Equal(
            new StrokeRectangleCommand(position, size, pen),
            new StrokeRectangleCommand(position, size, pen)
        );
        Assert.Equal(
            new StrokeEllipseCommand(position, size, pen),
            new StrokeEllipseCommand(position, size, pen)
        );
    }
}
