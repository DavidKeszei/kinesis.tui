using System;
using System.Collections.Generic;
using System.Text;

namespace Kinesis.Input.Windows;

public record struct InputInfo(char Key, InputModifier Modifiers, bool IsPress);
