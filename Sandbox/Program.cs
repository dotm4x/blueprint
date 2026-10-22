using Combine.Core;
using Combine.Core.Events;

CounterComponent counter = new()
{
    Identifier = "Counter",
    Value = 5,
    Tags = { "Example", "Interactive" },
    Components =
    {
        new Component { Identifier = "Header", Tags = { "Content" } },
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

internal class CounterComponent(ComponentOptions? options = null) : Component(options)
{
    public Store<int> Value { get; init; } = new(0);
}
