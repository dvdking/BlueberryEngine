using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.Diagnostics
{
    public interface IDiagnosable
    {
        void DebugAction();
        string DebugInfo();
        string DebugName { get; }
    }
}
