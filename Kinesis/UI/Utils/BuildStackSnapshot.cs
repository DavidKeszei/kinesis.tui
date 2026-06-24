using System;
using System.Collections.Generic;
using System.Text;

using Kinesis.UI.Components;

namespace Kinesis.UI;

/// <summary>
/// Represents a split state from the <see cref="BuildContext"/> at some <see cref="Island"/>.
/// </summary>
/// <param name="Scale">Closest <see cref="Components.Scale"/> instance.</param>
/// <param name="Position">Closest <see cref="Components.Position"/> instance.</param>
/// <param name="Styles">Styles of the of the <see cref="BuildContext"/>.</param>
internal record class BuildStackSnapshot(Scale Scale, Position Position, Style[] Styles);
