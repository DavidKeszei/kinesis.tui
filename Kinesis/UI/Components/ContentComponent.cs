using Kinesis.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.UI.Components;

internal class ContentComponent: Component, IStaticType {
    private const string TYPE = nameof(ContentComponent);
    private Entity m_content = null!;

    public static string Name { get => TYPE; }

    public Entity Content { get => m_content; set => m_content = value; }

    public ContentComponent(Entity content) : base(id: ComponentRegistry.QueryComponent(name: TYPE))
        => m_content = content;
}
