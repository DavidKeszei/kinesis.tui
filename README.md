# Kinesis

Declarative, enjoyable way to write TUI applications. A simple, Flutter-inspired library with love of the dotnet ecosystem in C#. You can build simple (or complex) apps, which works like their GUI counterparts with less nerve.
The goal to create an enjoyable library, which achives performance-boost and less memory pressure without giving up the modern, explicit and declarative UI building!

# For the start

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
See? It's that simple: you declare the UI elements you want to use in a tree structure. Everthing is explictly declarated with less magic, hidden controll-flow behind it!

# Disclamer
Developed as a __Diploma Thesis__ project.
