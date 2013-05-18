using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class ComponentTypeManager
    {
        private static int typeValue = 0;
        private static ComponentTypeManager _instance;

        public static ComponentTypeManager Please
        {
            get
            {
                if (_instance == null)
                    _instance = new ComponentTypeManager();
                return _instance;
            }
        }


        private Dictionary<Type, int> _registeredTypes;

        public ComponentTypeManager()
        {
            _registeredTypes = new Dictionary<Type, int>();
        }
        public ComponentType GetComponentTypeOf(Type type)
        {
            int ctype;
            if (!_registeredTypes.TryGetValue(type, out ctype))
            {
                if (type.IsSubclassOf(typeof (Component)))
                {
                    ctype = typeValue;
                    _registeredTypes[type] = typeValue++;
                }
                else
                    throw new ArgumentException("Type must be subclass of a Component","type");
            }
            return new ComponentType(ctype);
        }
    }
}
