using System;

namespace Blueberry.ComponentModel
{
    [Flags]
    public enum SyncAction
    {
        NoAction = 0,
        Add = 1,
        Remove = 2,
        Resolve = 4,
        Regroup = 8
    }
}
