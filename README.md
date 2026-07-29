# Kinesis

Kinesis is small & simple UI library for building terminal apps! It lets you build complex terminal interfaces using a cozy OOP shell that gets flattened into a data-oriented beast under the hood. All this, while (try) keeping a stable footprint—because your RAM has better things to do.

# Example: The "Not-so-scary" Counter

If you ever touched some UI library (Flutter, React) this will be familiar to you. If you haven't, don't worry I'll (try to) show you! So let's see "the beast"!

```csharp
KinesisEngine engine = new KinesisEngine(title: "Counter");
engine.RegisterIsland<App>(name: nameof(App), static (provider) => new App());

await engine.Start();

public sealed class App: Island {
    private readonly static string s_uiText = "__uiCounter__";
    private static uint m_count = 0;

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<InputMessage>(context) {
            On = static(message, ref tree) => {

                if (message.IsPressed && message.Key == 0x20)
                    tree.Visit<UIText>(name: s_uiText)?.Text = $"Press count: {m_count++}";
            },
            Child = new Center {
                Child = new UIText {
                    Name = s_uiText,
                    Text = "Press any button to start..."
                }
            }
        };
    }
}
```
See? It's that simple: you declare the UI elements you want to use in a tree structure. If you want some interactivity, just use OnUpdate<T> with an On callback! Easy-peasy!

# The Secret Sauce (How it works?)

"Okay, it's simple and cozy, but where is the 'beast' part?" — you might ask. The magic happens behind the scenes:
- __Island Flattening__: When you call Build(), Kinesis doesn't just keep a heavy object tree. It flattens everything into a lean, read-only list for the ECS core.
- __Priority Lane__: We don't like lag. Input information is strictly processed before rendering metadata. Always.
- __Zero-Allocation Goals__: The engine is a _gentleman_: not spamming the RAM with unpleasant bytes.

# Disclamer
Developed as a __Diploma Thesis__ project.
