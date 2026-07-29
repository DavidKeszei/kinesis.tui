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

        public int CountComponent<T>(Func<T, bool>? comparand = null) where T: Component, IStaticType {
            if (entity == null) return 0;

            int count = 0;
            comparand ??= static(_) => true;

            foreach (Component comp in entity) {
                if (comp == null) continue;

                if (comp.TypeOf(T.Name) && comparand((T)comp))
                    ++count;
            }

            return count;
        }

        /// <summary>
        /// Move an <see cref="Entity"/> based on the <paramref name="x"/> and <paramref name="y"/> values.
        /// </summary>
        /// <param name="x">Move amount on the X axis.</param>
        /// <param name="y">Move amount on the Y axis.</param>
        /// <remarks>
        /// <b>Remarks:</b> If the entity not has <see cref="Position"/> component, then the function does nothing.
        /// </remarks>
        public void Move(float x, float y) {
            if (entity.Get<Position>() == null) return;
            entity.Get<Position>()!.Relative = new Vec2(x, y);
        }
    }
}