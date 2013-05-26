using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Blueberry.ComponentModel
{
    public abstract class Component
    {
        private Entity _owner;
        public readonly ComponentType CType;

        public Entity Owner
        {
            get { return _owner; }
            internal set { _owner = value; }
        }

        internal SyncAction SyncAction { get; set; }

        protected Component()
        {
            CType = new ComponentType(GetType());
        }

        public virtual void ReceiveMessage(IMessage message)
        {
        }

        protected internal virtual void Initialize()
        {
            
        }

        protected internal virtual void Release()
        {
            ClearDependencies();
        }

        private static bool CanDependOn(Type type)
        {
            return typeof (Component).IsAssignableFrom(type);
        }

        public static Component Create(Type type)
        {
            Component result = null;
            try
            {
                result = Activator.CreateInstance(type) as Component;
            }
            catch (MissingMethodException)
            {
                throw new MissingMethodException(String.Format(
                    "The component type '{0}' does not provide a parameter-less constructor.", type.ToString()));
            }

            return result;
        }

        public static Component Create<T>() where T : Component
        {
            Component result = null;
            try
            {
                result = Activator.CreateInstance(typeof (T)) as Component;
            }
            catch (MissingMethodException)
            {
                throw new MissingMethodException(String.Format(
                    "The component type does not provide a parameter-less constructor."));
            }

            return result;
        }

        public static bool CanCreate(Type type)
        {
            return typeof (Component).IsAssignableFrom(type) && type.GetConstructor(Type.EmptyTypes) != null;
        }

        public static bool CanCreate<T>() where T : Component
        {
            return typeof (T).GetConstructor(Type.EmptyTypes) != null;
        }

        private Dictionary<FieldInfo, RequireComponentAttribute> _dependencies =
            new Dictionary<FieldInfo, RequireComponentAttribute>();

        internal void FindDependencies()
        {
            _dependencies.Clear();

            Type type = GetType();

            FindDependencies(type, _dependencies);

            while ((type = type.BaseType) != null)
            {
                if (!CanDependOn(type))
                    break;
                FindDependencies(type, _dependencies);
            }
        }

        private void FindDependencies(Type type, Dictionary<FieldInfo, RequireComponentAttribute> deps)
        {
            FieldInfo[] fields = type.GetFields(
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                foreach (RequireComponentAttribute dependency in
                    field.GetCustomAttributes(typeof (RequireComponentAttribute), false))
                {
                    Type componentType = field.FieldType;

                    if (CanDependOn(componentType))
                    {
                        if (dependency.Automatically)
                        {
                            if (!CanCreate(componentType))
                            {
                                throw new InvalidOperationException(
                                    String.Format(CultureInfo.InvariantCulture,
                                                  "This field can not be marked as a dependency because its type does not provide a parameter-less constructor. {0}",
                                                  field.DeclaringType.ToString() + "." + field.Name));
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            String.Format(CultureInfo.InvariantCulture,
                                          "This field can not be marked as a dependency because its type does not implement 'IComponent' or is a subclass of 'DependencyComponent'. {0}",
                                          field.DeclaringType.ToString() + "." + field.Name));
                    }

                    try
                    {
                        _dependencies.Add(field, dependency);
                    }
                    catch (ArgumentNullException)
                    {
                        continue;
                    }
                    catch (ArgumentException)
                    {
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Goes through all discovered dependencies and injects them one by one.
        /// </summary>
        internal void InjectDependencies()
        {
            if (Owner == null)
                return;

            foreach (var pair in _dependencies)
            {
                FieldInfo field = pair.Key;
                RequireComponentAttribute dependency = pair.Value;

                // Determines which entity the dependency should be grabbed from. 
                Entity entity = (!string.IsNullOrEmpty(dependency.FromEntityNamed))
                                    ? EntityWorld.Please.FindByTag(dependency.FromEntityNamed)
                                    : Owner;

                if (entity == null)
                    continue;

                /// Immediately attempt injecting the component.
                InjectDependency(field, entity, dependency);
            }
        }

        /// <summary>
        /// Attempts to inject a component into a field, and adds the component to the specified entity.
        /// </summary>
        /// <remarks>
        /// > Note that the dependency will remain, even if it becomes dettached from its entity.
        /// </remarks>
        private void InjectDependency(FieldInfo field, Entity entity, RequireComponentAttribute attribute)
        {
            if (field == null || entity == null)
                return;

            Type componentType = field.FieldType;
            Component dependency = entity.GetComponent(componentType);

            if (dependency == null && attribute.Automatically)
            {
                dependency = Create(componentType);

                if (dependency == null)
                    return;

                entity.AddComponent(dependency);
            }

            if (dependency != null)
                field.SetValue(this, dependency);
        }

        /// <summary>
        /// Clears out all managed dependencies.
        /// </summary>
        protected void ClearDependencies()
        {
            foreach (KeyValuePair<FieldInfo, RequireComponentAttribute> pair in _dependencies)
            {
                FieldInfo field = pair.Key;
                field.SetValue(this, null);
            }
        }
    }
}
