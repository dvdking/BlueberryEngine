using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class Entity
    {
        private static int nextId = 0;
 
        private string _tag;
        private readonly int _id;

        internal BigInteger ComponentBits;
        internal BigInteger SystemBits;
        internal BigInteger GroupBits;

        public string Tag
        {
            get { return _tag; }
            set { _tag = value; }
        }

        public int Id  { get { return _id; } }

        internal SyncAction SyncAction { get; set; }

        internal Entity(string tag, int id)
        {
            _tag = tag;
            _id = id;
            Init();
        }
        internal Entity(string tag) : this(tag, nextId++) { }
        internal Entity() : this("", nextId++) { }


        /// <summary>
        /// resets entity to default state, used for poolling
        /// </summary>
        internal void Init()
        {
            ComponentBits = BigInteger.Zero;
            SystemBits = BigInteger.Zero;
            GroupBits = BigInteger.Zero;
            SyncAction = 0;
        }

        public void AddComponent(Component component)
        {
            if (component == null)
                throw new ArgumentNullException("component", "component can not be null");

            if ((ComponentBits & component.CType.TypeBit) != 0)
                throw new ArgumentException("Given component already contains in entity", "component");
            ComponentBits |= component.CType.TypeBit;
            component.Owner = this;
            EntityWorld.Please.AddToSync(component, SyncAction.Add);
        }
        public void AddComponent(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type", "type can not be null");
            if (Component.CanCreate(type))
            {
                if((ComponentBits & ComponentType.GetBit(type)) != 0)
                    throw new ArgumentException("Component of given type already contains in entity or in add queue", "type");
                Component component = Component.Create(type);
                ComponentBits |= component.CType.TypeBit;
                component.Owner = this;
                EntityWorld.Please.AddToSync(component, SyncAction.Add);
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
                    throw new ArgumentNullException("components", "components can not be null");
                ComponentType ctype = component.CType;
                if ((ComponentBits & ctype.TypeBit) != 0)
                    throw new ArgumentException("Given component already contains in entity or in add queue", "components[i]");
                ComponentBits |= ctype.TypeBit;
                component.Owner = this;
                EntityWorld.Please.AddToSync(component, SyncAction.Add);
            }
        }
        public void AddComponents(IEnumerable<Type> componentTypes)
        {
            foreach (var type in componentTypes)
            {
                if (type == null)
                    throw new ArgumentNullException("componentType", "Component type can't be null");
                if (Component.CanCreate(type))
                {
                    if ((ComponentBits & ComponentType.GetBit(type)) != 0)
                        throw new ArgumentException("Component of given type already contains in entity or in add queue","type");
                    Component component = Component.Create(type);
                    ComponentBits |= component.CType.TypeBit;
                    component.Owner = this;
                    EntityWorld.Please.AddToSync(component, SyncAction.Add);
                }
            }
        }
        public void AddComponents(params Component[] components)
        {
            AddComponents((IEnumerable<Component>)components);
        }
        public void AddComponents(params Type[] componentTypes)
        {
            AddComponents((IEnumerable<Type>)componentTypes);
        }

        public bool RemoveComponent(Component component)
        {
            if (component != null)
            {
                if ((ComponentBits & component.CType.TypeBit) != 0 && component.Owner == this)
                {
                    EntityWorld.Please.AddToSync(component, SyncAction.Remove);
                    ComponentBits ^= component.CType.TypeBit;
                    return true;
                }
            }
            return false;
        }

        public bool RemoveComponent(ComponentType ctype)
        {
            return RemoveComponent(EntityWorld.Please.GetComponent(this, ctype));
        }

        public void Broadcast(IMessage message)
        {
            var components = EntityWorld.Please.GetComponents(this);
            if(components != null)
                foreach (Component component in components)
                    component.ReceiveMessage(message);
        }

        public void Send<T>(IMessage message) where T : Component
        {
            Component component = GetComponent<T>();
            if(component != null)
                component.ReceiveMessage(message);
        }

        public Component GetComponent(ComponentType ctype)
        {
            if ((ComponentBits & ctype.TypeBit) != 0)
                return EntityWorld.Please.GetComponent(this, ctype);
            return null;
        }
        public Component GetComponent(Type type)
        {
            var ctype = new ComponentType(type);
            if ((ComponentBits & ctype.TypeBit) != 0)
                return EntityWorld.Please.GetComponent(this, ctype);
            return null;
        }
        public Component GetComponent<T>() where T: Component
        {
            return GetComponent(typeof (T));
        }

        public bool ContainsComponent(ComponentType type)
        {
            return (ComponentBits & type.TypeBit) != 0;
        }

        public bool ContainsComponent(Type type)
        {
            return (ComponentBits & ComponentType.GetBit(type)) != 0;
        }

        public IEnumerable<Component> GetComponents()
        {
            return EntityWorld.Please.GetComponents(this);
        }
        
        public void LeaveGroup(string groupName)
        {
            EntityGroup.GetGroup(groupName).Remove(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }
        public void LeaveGroup(EntityGroup group)
        {
            group.Remove(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }

        public void LeaveAllGroups()
        {
            EntityGroup.LeaveAll(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }

        public void JoinGroup(string groupName)
        {
            EntityGroup.GetGroup(groupName).Add(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }
        public void JoinGroup(EntityGroup group)
        {
            group.Add(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }

        public void ToggleGroup(string groupName)
        {
            ToggleGroup(EntityGroup.GetGroup(groupName));
        }
        public void ToggleGroup(EntityGroup group)
        {
            if ((GroupBits & group.GroupBit) != 0)
                group.Add(this);
            else
                group.Remove(this);
            EntityWorld.Please.AddToSync(this, SyncAction.Regroup);
        }

        public bool InGroup(string groupName)
        {
            return (GroupBits & EntityGroup.GetGroup(groupName).GroupBit) != 0;
        }
        public bool InGroup(EntityGroup group)
        {
            return (GroupBits & group.GroupBit) != 0;
        }

        public bool InEveryGroup(params string[] groups)
        {
            return InEveryGroup((IEnumerable<string>) groups);
        }
        public bool InAnyGroup(params string[] groups)
        {
            return InAnyGroup((IEnumerable<string>) groups);
        }
        public bool InEveryGroup(IEnumerable<string> groups)
        {
            foreach (var @group in groups)
            {
                EntityGroup g = EntityGroup.GetGroup(@group);
                if ((GroupBits & g.GroupBit) == 0) return false;
            }
            return true;
        }
        public bool InAnyGroup(IEnumerable<string> groups)
        {
            foreach (var @group in groups)
            {
                EntityGroup g = EntityGroup.GetGroup(@group);
                if ((GroupBits & g.GroupBit) != 0) return true;
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Id;
        }
        public void ResolveDependencies()
        {
            var components = EntityWorld.Please.GetComponents(this);
            foreach (var component in components)
            {
                component.FindDependencies();
                component.InjectDependencies();
            }
        }
    }
}
