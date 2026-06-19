using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public record struct InputInfo(InputModifier Modifiers, char Key, bool IsPress);
