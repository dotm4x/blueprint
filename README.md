<p align="center" style="margin-bottom: 0;">
  <img src="Assets/Logo.svg" width="120" alt="Blueprint">
</p>

<h1 align="center" style="margin-top: 0; margin-bottom: 8px;">
  Blueprint
</h1>

<p align="center" style="margin-top: 0;">
  <strong>The foundation for modern user interfaces.</strong>
</p>


Blueprint is an experimental, cross-platform technology for composing user interfaces through a shared, platform-independent model.

It aims to separate interface building from rendering, allowing different platform implementations to interpret the same element structure.

## Vision

Blueprint aims to provide a consistent foundation for building interfaces across platforms, without tying the interface model to a specific rendering technology.

Its design is centered around:

- **Build** — interfaces are described through hierarchical elements.
- **Consistency** — structure, styling, and layout follow a shared model across platforms.
- **Separation** — interface descriptions remain independent of rendering implementations.
- **Extensibility** — platform-specific implementations can integrate without redefining the core model.

## Example

The following example illustrates the intended building style of Blueprint. It is conceptual and intended to demonstrate the API's design direction, not a complete, production-ready implementation.

```csharp
using Blueprint.Common;
using Blueprint.Common.Events;

CounterElement counter = new()
{
    Identifier = "Counter",
    Value = 5,
    Tags = { "Example", "Interactive" },
    Elements =
    {
        new Element
        {
            Identifier = "Header",
            Tags = { "Content" },
        },
        new Element
        {
            Identifier = "Actions",
            Elements =
            {
                new Element { Identifier = "Increment" },
                new Element { Identifier = "Decrement" },
            },
        },
    },
};

Console.WriteLine($"Element: {counter.Path}");
Console.WriteLine($"Value: {counter.Value}");
Console.WriteLine($"Increment: {counter.Elements.GetByPath("Actions/Increment")!.Path}");

internal class CounterElement(ElementOptions? options = null) : Element(options)
{
    public Store<int> Value { get; init; } = new(0);
}
```

## Project Status

> [!WARNING]
> Blueprint is in its early stages of development. Its architecture, APIs, and project structure are subject to change. It is not yet intended for production use.

## License

This project is licensed under the MIT License, see the [LICENSE](LICENSE) for more information.