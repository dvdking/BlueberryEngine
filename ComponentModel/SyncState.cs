using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    [Flags]
    public enum SyncState
    {
        Add = 1,
        Remove = 2,
        Refresh = 4
    }
}
