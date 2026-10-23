using Blueprint.Drawing.Brushes;

namespace Blueprint.Drawing.Commands;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Represents a command that outlines a rectangle.
/// </summary>
/// <param name="Position">The position of the rectangle.</param>
/// <param name="Size">The size of the rectangle.</param>
/// <param name="Pen">The pen used to draw the outline.</param>
public sealed record StrokeRectangleCommand(Position Position, Size Size, Brush Pen) : DrawCommand;
