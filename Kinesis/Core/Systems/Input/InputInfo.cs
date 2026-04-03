using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Core;

public record struct InputInfo(char Key, InputModifier Modifiers, bool IsPress);
