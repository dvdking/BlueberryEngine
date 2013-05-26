using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blueberry.ComponentModel
{
    public struct ComponentType
    {
        private static int _nextId = 0;
        private static Dictionary<Type, int> _typeRegister;
 
        public readonly int Id;
        public readonly BigInteger TypeBit;

        static ComponentType()
        {
            _typeRegister = new Dictionary<Type, int>();
        }

        internal ComponentType(Type type)
        {
            int id;
            if (!_typeRegister.TryGetValue(type, out id))
            {
                id = _nextId++;
                _typeRegister[type] = id;
            }
            Id = id;
            TypeBit = 1;
            TypeBit <<= id;
        }
        
        public override int GetHashCode()
        {
            return Id;
        }
        public static BigInteger GetBit(Type type)
        {
            int id;
            if (_typeRegister.TryGetValue(type, out id))
            {
                BigInteger bit = 1;
                bit <<= id;
                return bit;
            }
            return 0;
        }
    }
}
