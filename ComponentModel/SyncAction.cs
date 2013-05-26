using System;

namespace Blueberry.ComponentModel
{
    [Flags]
    public enum SyncAction
    {
        Add = 1,
        Remove = 2,
        Resolve = 4,
        Regroup = 8
    }
}
