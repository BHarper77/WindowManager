using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotKeyLibrary
{
    /// <summary>
    /// Enum representing modifier keys for a hotkey configuration. 
    /// Custom enum required as Windows VirtualKeys doesn't map directly to RegisterHotKey function paramater
    /// </summary>
    public enum ModKeys
    {
        Alt = 0x0001,
        Ctrl = 0x0002,
        Shift = 0x0004
    }
}
