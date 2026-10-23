using Blueprint.Drawing.Brushes;

namespace Blueprint.Drawing.Commands;

using Blueprint.Geometry.Primitives;

/// <summary>
/// Represents a command that fills a rectangle.
/// </summary>
/// <param name="Position">The position of the rectangle.</param>
/// <param name="Size">The size of the rectangle.</param>
/// <param name="Brush">The brush used to fill the rectangle.</param>
public sealed record FillRectangleCommand(Position Position, Size Size, Brush Brush) : DrawCommand;
