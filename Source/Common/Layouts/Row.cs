using System.Numerics;

namespace Combine.Common.Layouts;

/// <summary>
/// Arranges children horizontally from left to right.
/// </summary>
/// <remarks>
/// Child dimensions are resolved against the available content size.
/// Minimum size, maximum size, and aspect ratio constraints are applied
/// consistently when measuring and arranging children.
/// Alignment controls the vertical placement of children, while
/// distribution controls their horizontal placement.
/// </remarks>
public class Row : Layout
{
    /// <inheritdoc />
    public override Vector2 Measure(LayoutContext context, Vector2 availableSize)
    {
        Vector2 margin = ResolveMargin(availableSize);
        var contentSize = Vector2.Max(availableSize - margin, Vector2.Zero);

        float spacing = Spacing.Resolve(contentSize.X);
        float width = 0;
        float height = 0;

        foreach (Component child in context.Children)
        {
            Vector2 resolvedSize = ResolveChildSize(child, contentSize);

            width += resolvedSize.X;
            height = MathF.Max(height, resolvedSize.Y);
        }

        if (context.Children.Count > 1)
        {
            width += spacing * (context.Children.Count - 1);
        }

        return new Vector2(width + margin.X, height + margin.Y);
    }

    /// <inheritdoc />
    public override IReadOnlyList<LayoutArrangement> Arrange(
        LayoutContext context,
        Vector2 finalSize
    )
    {
        List<LayoutArrangement> arrangements = new(context.Children.Count);

        if (context.Children.Count == 0)
        {
            return arrangements;
        }

        Vector2 margin = ResolveMargin(finalSize);
        var contentSize = Vector2.Max(finalSize - margin, Vector2.Zero);

        float spacing = Spacing.Resolve(contentSize.X);
        float positionX = Margin.Left.Resolve(finalSize.X);

        List<Vector2> sizes = new(context.Children.Count);
        float totalWidth = 0;

        foreach (Component child in context.Children)
        {
            Vector2 size = ResolveChildSize(child, contentSize);

            if (Alignment == Alignment.Stretch)
            {
                size = ResolveChildSize(child, contentSize, new Vector2(size.X, contentSize.Y));
            }

            sizes.Add(size);
            totalWidth += size.X;
        }

        float availableSpacing = MathF.Max(
            0,
            contentSize.X - totalWidth - spacing * (context.Children.Count - 1)
        );

        float leadingSpace = 0;

        if (Distribution == Distribution.Center)
        {
            leadingSpace = availableSpacing / 2;
        }
        else if (Distribution == Distribution.End)
        {
            leadingSpace = availableSpacing;
        }
        else if (Distribution == Distribution.SpaceBetween && context.Children.Count > 1)
        {
            spacing += availableSpacing / (context.Children.Count - 1);
        }
        else if (Distribution == Distribution.SpaceAround)
        {
            spacing += availableSpacing / context.Children.Count;
            leadingSpace = availableSpacing / context.Children.Count / 2;
        }
        else if (Distribution == Distribution.SpaceEvenly)
        {
            spacing += availableSpacing / (context.Children.Count + 1);
            leadingSpace = availableSpacing / (context.Children.Count + 1);
        }

        positionX += leadingSpace;

        for (int index = 0; index < context.Children.Count; index++)
        {
            Component child = context.Children[index];
            Vector2 size = sizes[index];

            float positionY = Margin.Top.Resolve(finalSize.Y);

            if (Alignment == Alignment.Center)
            {
                positionY += (contentSize.Y - size.Y) / 2;
            }
            else if (Alignment == Alignment.End)
            {
                positionY += contentSize.Y - size.Y;
            }

            LayoutResult result = new(new Vector2(positionX, positionY), size);

            arrangements.Add(new LayoutArrangement(child, result));
            positionX += size.X + spacing;
        }

        return arrangements;
    }
}
