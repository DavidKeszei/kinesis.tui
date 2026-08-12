using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Kinesis.UI;

public sealed class Toggle<T>: Island, ICopyable<BuildContext> {
    private readonly string m_toogleTextName = null!;
    private readonly string m_toogleTitleName = null!;

    private readonly string m_offState = null!;
    private readonly string m_onState  = null!;

    private readonly string m_title = null!;
    private readonly T m_offValue = default!;

    private readonly T m_onValue = default!;
    private bool m_isToggled = false;

    public string On { init => m_onState = value; }

    public string Off { init => m_offState = value; }

    public string Title { init => m_title = value; }

    public bool IsToggled { get => m_isToggled; set => m_isToggled = value; }

    public T Value { get => m_isToggled ? m_onValue : m_offValue; }

    public Toggle(T off, T on) {
        m_offValue = off;
        m_onValue  = on;

        m_toogleTextName  = $"__toggle_{Guid.CreateVersion7()}__";
        m_toogleTitleName = $"__toggle_title_{Guid.CreateVersion7()}__";

        _ = Attach<Position>(ComponentPool<Position>.Instance.Rent<Position>());
        _ = Attach<Scale>(ComponentPool<Scale>.Instance.Rent<Scale>(static(scale) => scale.Value = Vec2.Auto with { Y = 1 }));
    }

    public void Copy(ref BuildContext context) {
        context.SetPivot<Scale>(this);
        context.SetPivot<Position>(this);
    }

    protected override Entity? Build(ref readonly BuildContext context) {
        Get<Scale>()!.Value = new Vec2(x: m_offState.Length + m_title.Length + 1, y: 1);

        return new Viewport {
            Content = new OnUpdate<RenderMessage>(context) {
                On = (message, ref readonly tree) => {

                    Text text = tree.Visit<Text>(name: m_toogleTextName)!;
                    text.Content = m_isToggled ? m_onState : m_offState;

                    Text title = tree.Visit<Text>(name: m_toogleTitleName)!;
                    title.Move(x: text.Content.Length + 1, y: 0);

                    Get<Scale>()!.Value = new Vec2(x: text.Content.Length + title.Content.Length + 1, y: 1);
                    text.ClipRenderScale();
                },
                Content = new Stack(capacity: 2) {
                    Content = [
                        new Text {
                            Name = m_toogleTitleName,
                            Content = m_title ?? string.Empty
                        },
                        new Text {
                            Name = m_toogleTextName,
                            Content = m_offState
                        }
                    ]
                }
            }
        };
    }
}
