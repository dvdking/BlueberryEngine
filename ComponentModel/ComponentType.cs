using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public struct ComponentType
    {
        public readonly int Value;

        internal ComponentType(int type)
        {
            this.Value = type;
        }
        public override int GetHashCode()
        {
            return Value;
        }
    }
}
