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
        public void Move(Vec2 origin, Vec2 offset) {
            Position? position = entity?.GetComponent<Position>();
            if (position == null) return;

            position.Origin = origin;
            position.Offset = offset;
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