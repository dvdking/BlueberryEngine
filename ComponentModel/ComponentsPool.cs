using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class ComponentsPool
    {
        public ComponentType ComponentType
        {
            get { return _componentType; }
        }

        private readonly ComponentType _componentType;
        private readonly ConstructorInfo _constructor;
        private readonly Stack<Component> _components;



        public ComponentsPool(ComponentType componentType, ConstructorInfo constructor, int capacity = 32)
        {
            _componentType = componentType;
            _constructor = constructor;
            _components = new Stack<Component>(capacity);
        }

        public void Push(Component component)
        {
            _components.Push(component);
        }

        public Component Create()
        {
            if (_components.Count > 0)
                return _components.Pop();

            return (Component)_constructor.Invoke(null);
        }

        public void Clear()
        {
            _components.Clear();
        }
    }
}
