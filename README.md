<p align="center" style="margin-bottom: 0;">
  <img src="Assets/Logo.svg" width="120" alt="Combine">
</p>

<h1 align="center" style="margin-top: 0; margin-bottom: 8px;">
  Combine
</h1>

<p align="center" style="margin-top: 0;">
  <strong>The foundation for modern user interfaces.</strong>
</p>


Combine is an experimental, cross-platform technology for composing user interfaces through a shared, platform-independent model.

It aims to separate interface composition from rendering, allowing different platform implementations to interpret the same component structure.

## Vision

Combine aims to provide a consistent foundation for building interfaces across platforms, without tying the interface model to a specific rendering technology.

Its design is centered around:

- **Composition** — interfaces are described through hierarchical components.
- **Consistency** — structure, styling, and layout follow a shared model across platforms.
- **Separation** — interface descriptions remain independent of rendering implementations.
- **Extensibility** — platform-specific implementations can integrate without redefining the core model.

## Example

The following example illustrates the intended composition style of Combine. It is conceptual and intended to demonstrate the API's design direction, not a complete, production-ready implementation.

```csharp
using Combine.Core;

CounterComponent counter = new()
{
    Identifier = "Counter",
    Value = 5,
    Tags = { "Example", "Interactive" },
    Components =
    {
        new Component
        {
            Identifier = "Header",
            Tags = { "Content" },
        },
        new Component
        {
            Identifier = "Actions",
            Components =
            {
                new Component { Identifier = "Increment" },
                new Component { Identifier = "Decrement" },
            },
        },
    },
};

Console.WriteLine($"Component: {counter.Path}");
Console.WriteLine($"Value: {counter.Value}");
Console.WriteLine($"Increment: {counter.Components.GetByPath("Actions/Increment")!.Path}");

internal class Counter(ComponentOptions? options = null) : Component(options)
{
    public Store<int> Value { get; init; } = new(0);
}
```

## Project Status

> [!WARNING]
> Combine is in its early stages of development. Its architecture, APIs, and project structure are subject to change. It is not yet intended for production use.

## License

This project is licensed under the MIT License, see the [LICENSE](LICENSE) for more information.