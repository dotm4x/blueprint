namespace Blueprint.Drawing;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Provides the contextual information required to draw an element.
/// </summary>
/// <param name="bounds">The resolved geometry of the element.</param>
/// <param name="surfaceSize">The size of the rendering surface.</param>
public class DrawContext(DrawBounds bounds, Size surfaceSize)
{
    /// <summary>
    /// Gets the resolved geometry of the element being drawn.
    /// </summary>
    public DrawBounds Bounds { get; } = bounds;

    /// <summary>
    /// Gets the size of the rendering surface.
    /// </summary>
    public Size SurfaceSize { get; } = surfaceSize;
}
