# Kinesis

Declarative, enjoyable way to write TUI applications. A simple, Flutter-inspired library with love of the .NET ecosystem in C#. You can build simple (or complex) apps, which works like their GUI counterparts with less nerve.
The goal to create an enjoyable library, which archives performance-boost and less memory pressure without giving up the modern, explicit and declarative UI building!

# For the start
If you ever touched some UI library (Flutter, React) this will be familiar to you. If you haven't, don't worry I'll (try to) show you:

```csharp
KinesisEngine engine = new KinesisEngine(title: "Counter");
engine.RegisterIsland<App>(name: nameof(App), static (provider) => new App());

await engine.Start();

public sealed class App: Island {
    private readonly static string s_uiText = "__uiCounter__";
    private static uint m_count = 0;

    protected override Entity? Build(ref readonly BuildContext context) {
        return new Scaffold {
            Content = new OnUpdate<KeyboardMessage>(context) {
                On = static (message, ref readonly tree) => {

                    if (message.IsPressed && message.Key == 0x20)
                        tree.Visit<Text>(name: s_uiText)?.Content = $"Press count: {++m_count}";
                },
                Content = new Center {
                    Content = new Text {
                        Name = s_uiText,
                        Content = "Press the SPACE button to start..."
                    }
                }
            }
        };
    }
}
```
See? It's that simple: you declare the UI elements you want to use in a tree structure. Everything is explicitly declarated with less magic, hidden controll-flow behind it!
