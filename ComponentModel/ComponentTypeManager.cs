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

        private List<ComponentsPool> _componentsPools;

        private Dictionary<Type, int> _registeredTypes;

        public ComponentTypeManager()
        {
            _registeredTypes = new Dictionary<Type, int>();
            _componentsPools = new List<ComponentsPool>();
        }

        /// <summary>
        /// Helper method to create an instance of a specified type, and casting it to `Component`
        /// </summary>
        public Component Create(Type type)
        {
            var pool = BinarySearchOfPool(GetComponentTypeOf(type));
            return pool.Create();
        }

        public Component Create<T>() where T : Component
        {
            var pool = BinarySearchOfPool(GetComponentTypeOf(typeof (T)));
            return pool.Create();
        }

        public ComponentType GetComponentTypeOf(Type type)
        {
            int ctype;
            ComponentType componentType;
            if (!_registeredTypes.TryGetValue(type, out ctype))
            {
                if (!type.IsSubclassOf(typeof (Component)))
                    throw new ArgumentException("Type must be subclass of a Component", "type");

                ctype = typeValue;
                _registeredTypes[type] = typeValue++;
                componentType = new ComponentType(ctype);
                CreatePool(componentType, type);
            }
            else
                componentType = new ComponentType(ctype);

            return componentType;
        }

        private void CreatePool(ComponentType componentType, Type type)
        {
            var pool = new ComponentsPool(componentType, type.GetConstructor(null));

            _componentsPools.Add(pool);

            _componentsPools.Sort((p1, p2) => p1.ComponentType.Value - p2.ComponentType.Value);
        }
        private ComponentsPool BinarySearchOfPool(ComponentType componentType)
        {
            int imin = 0;
            int imax = _componentsPools.Count - 1;

            while(imax >= imin)
            {
                int imid = (imin + imax)/2;
                if (_componentsPools[imid].ComponentType.Value < componentType.Value)
                    imin = imid + 1;
                else
                    imax = imid;
            }

            if((imax == imin) &&(_componentsPools[imin].ComponentType.Value == componentType.Value))
                return _componentsPools[imin];
            return null;
        }

    }
}
