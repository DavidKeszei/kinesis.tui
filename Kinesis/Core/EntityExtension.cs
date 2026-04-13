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
            int count = 0;
            comparand ??= static(_) => true;

            foreach (Component comp in entity)
                if (comp.TypeOf(T.Name) && comparand((T)comp))
                    ++count;

            return count;
        }
    }
}