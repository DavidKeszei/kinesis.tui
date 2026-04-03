using Kinesis.Core;
using Kinesis.Core.Rendering;
using Kinesis.UI;
using Kinesis.UI.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis;

public static class EntityExtension {
    extension(Entity entity) {

        /// <summary>
        /// Move the current <see cref="Entity"/> with <paramref name="x"/> and <paramref name="y"/> by the <paramref name="anchor"/>.
        /// </summary>
        /// <param name="x">Move adjustment on the X axis.</param>
        /// <param name="y">Move adjustment on the Y axis.</param>
        /// <param name="anchor">Pivot/Anchor of the movement.</param>
        public void Move(float x, float y, MoveAnchor anchor = MoveAnchor.ABSOLUTE) {
            Transform? transform = entity?.GetComponent<Transform>();
            if (transform == null) return;

            transform.Position = anchor switch {
                MoveAnchor.RELATIVE => new Vec2(x: transform.Position.X + x, y: transform.Position.Y + y),
                MoveAnchor.ABSOLUTE => new Vec2(x, y),
                _ => transform.Position
            };
        }

        public Style? QueryStyle(StyleTag tag) {
            using StyleEnumerator styles = new StyleEnumerator(entity);
            foreach (Style component in styles) {
                if (component.Tag == tag) return component;
            }

            return null!;
        }

        public int CountComponent<T>(Func<T, bool>? comparand = null) where T: Component, IStaticType {
            int count = 0;
            comparand ??= static(_) => true;

            foreach (Component comp in entity)
                if (comp.TypeOf(T.Name) && comparand((T)comp))
                    ++count;

            return count;
        }
    }
}

public enum MoveAnchor: byte {
    ABSOLUTE,
    RELATIVE
}
