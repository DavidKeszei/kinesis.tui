using Kinesis.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core
;

/// <summary>
/// Represent a target for a navigation.
/// </summary>
/// <param name="Creation">Creation method for a <see cref="UI.Island"/>.</param>
/// <param name="Page">The page itself.</param>
internal record NavigationTarget(Func<ISystemProvider, Island> Creation, Island? Page);
