using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class Entity
    {
        private static int nextId = 0;
 
        private string _name;
        private int _id;

        private Dictionary<ComponentType, Component> _components;
        private Dictionary<ComponentType, Component> _syncList; 

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Id  { get { return _id; } }

        public int Count { get { return _components.Count; } }

        public SyncState SyncState { get; internal set; }

        internal Entity(string name, int id)
        {
            _name = name;
            _id = id;
            _components = new Dictionary<ComponentType, Component>();
            _syncList = new Dictionary<ComponentType, Component>();
        }
        internal Entity(string name) : this(name, nextId++) { }
        internal Entity() : this("", nextId++) { }

        public void AddComponent(Component component)
        {
            if (component == null)
                throw new ArgumentNullException("component", "component can not be null");

            if (_components.ContainsKey(component.CType))
                throw new ArgumentException("Given component already contains in entity or in add queue", "component");
            _syncList[component.CType] = component;
            component.SyncState = SyncState.Add;
            EntityManager.Please.AddToSync(this, SyncState.Refresh);
        }
        public void AddComponent(Type componentType)
        {
            if (componentType == null)
                throw new ArgumentNullException("type", "type can not be null");
            if (Component.CanCreate(componentType))
            {
                ComponentType ctype = ComponentTypeManager.Please.GetComponentTypeOf(componentType);

                if (_components.ContainsKey(ctype))
                    throw new ArgumentException("Component of given type already contains in entity or in add queue", "componentType");
                Component component = ComponentTypeManager.Please.Create(componentType);
                _syncList[ctype] = component;
                component.SyncState = SyncState.Add;
                EntityManager.Please.AddToSync(this, SyncState.Refresh);
            }
        }

        public void AddComponent<T>() where T : Component
        {
            AddComponent(typeof (T));
        }

        public void AddComponents(IEnumerable<Component> components)
        {
            foreach (var component in components)
            {
                if (component == null)
                    throw new ArgumentNullException("component", "component can not be null");
                ComponentType ctype = component.CType;
                if (_components.ContainsKey(ctype))
                    throw new ArgumentException("Given component already contains in entity or in add queue", "component");
                _syncList[ctype] = component;  
                component.SyncState = SyncState.Add;
            }

            EntityManager.Please.AddToSync(this, SyncState.Refresh);
        }
        public void AddComponents(IEnumerable<Type> componentTypes)
        {
            foreach (var type in componentTypes)
            {
                if (type == null)
                    throw new ArgumentNullException("componentType", "Component type can't be null");
                if (Component.CanCreate(type))
                {
                    ComponentType ctype = ComponentTypeManager.Please.GetComponentTypeOf(type);
                    if (_components.ContainsKey(ctype))
                        throw new ArgumentException("Component of given type already contains in entity or in add queue",
                                                    "type");
                    Component component = ComponentTypeManager.Please.Create(type);
                    _syncList[ctype] = component;
                    component.SyncState = SyncState.Add;
                }
            }
            EntityManager.Please.AddToSync(this, SyncState.Refresh);
        }
        public void AddComponents(params Component[] components)
        {
            AddComponents((IEnumerable<Component>)components);
        }
        public void AddComponents(params Type[] componentTypes)
        {
            AddComponents((IEnumerable<Type>)componentTypes);
        }

        public void RemoveComponent(Component component)
        {
            if (component != null)
            {
                if (_components.ContainsKey(component.CType))
                {
                    _syncList.Add(component.CType, component);
                    component.SyncState = SyncState.Remove;
                }
            }
        }
        public void RemoveComponent(ComponentType type)
        {
            Component component;
            if (_components.TryGetValue(type, out component))
            {
                _syncList.Add(type, component);
                component.SyncState = SyncState.Remove;
            }
        }

        public void ResolveComponents()
        {
            foreach (var component in _syncList.Values)
            {
                if(component.SyncState == SyncState.Add)
                {
                    _components.Add(component.CType, component);
                    component.Owner = this;
                    component.OnAdded();
                }
                else if (component.SyncState == SyncState.Remove)
                {
                    _components.Remove(component.CType);
                    component.Owner = null;
                    component.OnRemoved();
                }
            }
            _syncList.Clear();

            foreach (var component in _components.Values)
            {
                component.FindDependencies();
                component.InjectDependencies();
            }
        }

        public void Broadcast(IMessage message)
        {
            foreach (Component component in _components.Values)
            {
                component.ReceiveMessage(message);
            }
        }

        public Component GetComponent(ComponentType type)
        {
            Component component;
            if (_components.TryGetValue(type, out component))
                return component;
            return null;
        }

        public Component GetComponent<T>() where T: Component
        {
            Component component;
            if (_components.TryGetValue(ComponentTypeManager.Please.GetComponentTypeOf(typeof(T)), out component))
                return component;
            return null;
        }

        public Component GetComponent(Type type, bool allowingDerivedTypes)
        {
            Component component = null;

            foreach (var c in _components.Values)
            {
                if ((allowingDerivedTypes && c.GetType().IsSubclassOf(type)) || c.GetType() == type)
                {
                    component = c;
                    break;
                }
            }
            return component;
        }

        public Component GetComponent<T>(bool allowingDerivedTypes) where T : Component
        {
            Component component = null;
            Type t = typeof (T);
            foreach (var c in _components.Values)
            {
                if ((allowingDerivedTypes && c is T) || c.GetType() == t)
                {
                    component = c;
                    break;
                }
            }
            return component;
        }

        public bool ContainsComponent(ComponentType type)
        {
            return _components.ContainsKey(type);
        }

        public bool ContainsComponent(Type type)
        {
            return _components.ContainsKey(ComponentTypeManager.Please.GetComponentTypeOf(type));
        }

        public IEnumerable<Component> GetComponents()
        {
            return _components.Values;
        }
        public IEnumerable<ComponentType> GetComponentTypes()
        {
            return _components.Keys;
        }

        public void LeaveGroup(string groupName)
        {
            EntityManager.Please.RemoveFromGroup(this, groupName);
        }

        public void LeaveAllGroups()
        {
            EntityManager.Please.RemoveFromAllGroups(this);
        }

        public void JoinGroup(string groupName)
        {
            EntityManager.Please.AddToGroup(this, groupName);
        }

        public void ToggleGroup(string groupName)
        {
            EntityManager.Please.ToggleGroup(this, groupName);
        }

        public bool InGroup(string groupName)
        {
            return EntityManager.Please.InGroup(this, groupName);
        }

        public bool InEveryGroup(params string[] groups)
        {
            return EntityManager.Please.InEveryGroup(this, groups);
        }
        public bool InAnyGroup(params string[] groups)
        {
            return EntityManager.Please.InAnyGroup(this, groups);
        }
        public bool InEveryGroup(IEnumerable<string> groups)
        {
            return EntityManager.Please.InEveryGroup(this, groups);
        }
        public bool InAnyGroup(IEnumerable<string> groups)
        {
            return EntityManager.Please.InAnyGroup(this, groups);
        }
        public override int GetHashCode()
        {
            return Id;
        }
    }
}
