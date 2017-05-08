using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Utilities
{
    public delegate MessageBoxResult OnUserMessageDelegate(MessageBoxImage image, string message, MessageBoxButton buttons, MessageBoxResult defaultResult);
}
