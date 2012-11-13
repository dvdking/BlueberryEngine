using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.Diagnostics
{
    public interface IDiagnosable
    {
        string DebugInfo(int i);
        string DebugName { get; }
    }
}
