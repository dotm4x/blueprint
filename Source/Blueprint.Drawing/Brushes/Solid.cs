using Blueprint.Drawing.Primitives;

namespace Blueprint.Drawing.Brushes;

/// <summary>
/// Represents a brush that fills an area with a solid color.
/// </summary>
/// <param name="Color">The color used to fill the area.</param>
public sealed record SolidBrush(Color Color) : Brush;
