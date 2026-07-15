# Kinesis

Declarative, enjoyable way to write TUI applications. A simple, Flutter-inspired library with love of the dotnet ecosystem in C#. You can build simple (or complex) apps, which works like their GUI counterparts with less nerve.
The goal to create an enjoyable library, which achives performance-boost and less memory pressure without giving up the modern, explicit and declarative UI building!

# For the start

If you ever touched some UI library (Flutter, React) this will be familiar to you. If you haven't, don't worry I'll (try to) show you! So let's see "the beast"!

```csharp
public static class Program {
    public static async Task Main(string[] args) {
        KinesisEngine engine = new KinesisEngine(title: "Playground");
        engine.RegisterIsland<App>(name: nameof(App), onCreate: static provider => new App());

        await engine.Start();
    }
}

public class App: Island {
    protected override Entity? Build(ref readonly BuildContext context) {
        return new Scaffold {
            Content = new Center {
                Content = new AnimatedArea<RGB, UIBox> {
                    Selector   = static(box)        => box.Background,
                    Applier    = static(box, color) => box.Background = color,

                    Duration   = TimeSpan.FromSeconds(.5f),
                    To         = RGB.Blue,

                    IsPeriodic = true,
                    Content = new UIBox {
                        Scale      = Vec2.AsSquare(scale: Vec2.One * 5.5f),
                        Background = RGB.White
                    }
                }
            }
        };
    }
}
```
See? It's that simple: you declare the UI elements you want to use in a tree structure. Everthing is explictly declarated with less magic, hidden controll-flow behind it!

# Disclamer
Developed as a __Diploma Thesis__ project.
