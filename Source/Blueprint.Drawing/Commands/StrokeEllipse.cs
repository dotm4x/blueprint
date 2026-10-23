using Blueprint.Drawing.Primitives;

namespace Blueprint.Drawing.Commands;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Represents a command that outlines an ellipse.
/// </summary>
/// <param name="Position">The position of the ellipse.</param>
/// <param name="Size">The size of the ellipse.</param>
/// <param name="Pen">The pen used to draw the outline.</param>
public sealed record StrokeEllipseCommand(Position Position, Size Size, Pen Pen) : DrawCommand;
