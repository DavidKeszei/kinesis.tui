using Kinesis;
using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI;

KinesisEngine engine = new KinesisEngine(title: "Playground");
engine.RegisterIsland<App>(name: nameof(App), onCreate: static(provider) => new App());

await engine.Start();

public sealed class App: Island {
    private int m_x = 0;

    protected override Entity? Build(BuildContext context) {
        return new OnUpdate<RenderMessage>(context.Root) {
            On = (message, ref visitor) => {
                visitor.Visit<UIBox>(name: "__hi")!.Move(x: (++m_x * message.DeltaTime * 300) % message.Scale.X, y: 0);
            },
            Child = new UIBox {
                Name = "__hi",

                Scale = new Vec2(x: 5, y: 2),
                Background = RGB.Red,

                Child = new OnUpdate<RenderMessage>(context.Root) {
                    On = static(message, ref visitor) => {
                        visitor.Visit<UIText>(name: "__fps")!.Text = $" FPS {message.FPS} ";
                    },
                    Child = new UIText {
                        Background = RGB.Yellow with { A = 128 }, 

                        Name = "__fps",
                        Text = " FPS 0 "
                    }
                }
            }
        };
    }
}