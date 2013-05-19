using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class EntityDefinition
    {
        private Dictionary<Type, Dictionary<PropertyInfo, object>> _components; 

        public EntityDefinition()
        {
            _components = new Dictionary<Type, Dictionary<PropertyInfo, object>>();
        }

        public void DefineComponent<T>(params object[] initializationParameters) where T : Component
        {
            int numberPairs = initializationParameters.Length / 2;
            Type t = typeof (T);
            _components[t] = new Dictionary<PropertyInfo, object>();
            for (int i = 0; i < numberPairs*2; i+=2)
            {
                string name = initializationParameters[i] as string;
                object value = initializationParameters[i + 1];
                
                if (!string.IsNullOrEmpty(name))
                {
                    PropertyInfo info = t.GetProperty(name);
                    if(info != null)
                        _components[t].Add(info, value);
                }
            }
        }

        public Entity Create(string name = "")
        {
            var entity = EntityManager.Please.CreateEntity();


            foreach (var definition in _components)
            {
                var component = ComponentTypeManager.Please.Create(definition.Key);
                foreach (var property in definition.Value)
                {
                    property.Key.SetValue(component, property.Value, null);
                }
                entity.AddComponent(component);
            }
            return entity;
        }
    }
}
