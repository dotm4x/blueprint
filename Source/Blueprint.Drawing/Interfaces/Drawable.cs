namespace Blueprint.Drawing.Interfaces;

/// <summary>
/// Defines an object that can draw its visual representation.
/// </summary>
public interface IDrawable
{
    /// <summary>
    /// Draws the object using the specified drawing context and canvas.
    /// </summary>
    /// <param name="context">
    /// The context containing the resolved geometry and rendering information.
    /// </param>
    /// <param name="canvas">
    /// The canvas on which the object is drawn.
    /// </param>
    void Draw(DrawContext context, DrawCanvas canvas);
}
