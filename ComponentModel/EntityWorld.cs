using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class EntityWorld
    {
        private static EntityWorld _instance;
        public static EntityWorld Please
        {
            get 
            { 
                if (_instance == null) 
                    _instance = new EntityWorld();
                return _instance;
            }
        }

        public bool NeedSync
        {
            get { return _componentSyncList.Count > 0 || _entitySyncList.Count > 0; }
        }

        private Bag<Bag<Component>> _globalStorage;// Dictionary<Entity, Dictionary<ComponentType, Component>> _globalStorage;
        private Dictionary<int, Entity> _entities; 

        private HashSet<Component> _componentSyncList;
        private HashSet<Entity> _entitySyncList;
        private HashSet<EntitySystem> _systems;

        private Dictionary<string, EntityDefinition> _definitions;

        public Entity CreateEntity(params Component[] components)
        {
            Entity entity = new Entity();
            entity.AddComponents(components);
            entity.SyncAction |= SyncAction.Add;
            _entitySyncList.Add(entity);
            return entity;
        }

        public Entity CreateEntity(params Type[] componentTypes)
        {
            Entity entity = new Entity();
            entity.AddComponents(componentTypes);
            entity.SyncAction |= SyncAction.Add;
            _entitySyncList.Add(entity);
            return entity;
        }

        public Entity CreateEntity(string tag = "")
        {
            Entity entity = new Entity(tag);
            entity.SyncAction |= SyncAction.Add;
            _entitySyncList.Add(entity);
            return entity;
        }
        
        public void Remove(Entity entity)
        {
            entity.SyncAction |= SyncAction.Remove;
            _entitySyncList.Add(entity);
        }

        public void DefineEntity(string definitionName, EntityDefinition definition)
        {
            _definitions[definitionName] = definition;
        }
        public Entity CreateFromDefinition(string definitionName, string entityName)
        {
            return _definitions[definitionName].Create(entityName);
        }

        public EntityWorld()
        {
            _globalStorage = new Bag<Bag<Component>>(); //_globalStorage = new Dictionary<Entity, Dictionary<ComponentType, Component>>();
            _componentSyncList = new HashSet<Component>();
            _definitions = new Dictionary<string, EntityDefinition>();
            _systems = new HashSet<EntitySystem>();
            _entitySyncList = new HashSet<Entity>();
            _entities = new Dictionary<int, Entity>();
        }

        public void Broadcast(IMessage message)
        {
            foreach (var entity in _entities.Values)
            {
                entity.Broadcast(message);
            }
        }

        public void Broadcast(string groupName, IMessage message)
        {
            foreach (var entity in EntityGroup.GetGroup(groupName).Entities)
            {
                entity.Broadcast(message);
            }
        }

        public void Send(Entity reciever, IMessage message)
        {
            reciever.Broadcast(message);
        }

        public Entity FindByTag(string tag)
        {
            foreach (var entity in _entities.Values)
            {
                if (entity.Tag == tag)
                    return entity;
            }
            return null;
        }

        internal void AddToSync(Component component, SyncAction syncAction)
        {
            component.SyncAction |= syncAction;
            _componentSyncList.Add(component);
            AddToSync(component.Owner, SyncAction.Resolve);
            
        }
        internal void AddToSync(Entity entity, SyncAction syncAction)
        {
            entity.SyncAction |= syncAction;
            _entitySyncList.Add(entity);
        }
        
        public void Sync()
        {
            if (_componentSyncList.Count > 0)
            {
                Bag<Component> components;
                foreach (var component in _componentSyncList)
                {
                    if (component.SyncAction.HasFlag(SyncAction.Add))
                    {
                        if (!_globalStorage.TryGetValue(component.Owner.Id, out components))
                            components = _globalStorage[component.Owner.Id] = new Bag<Component>();
                        components.Set(component.CType.Id, component);
                        component.Initialize();
                    }
                    else if (component.SyncAction.HasFlag(SyncAction.Remove))
                    {
                        if (!_globalStorage.TryGetValue(component.Owner.Id, out components))
                            components = _globalStorage[component.Owner.Id] = new Bag<Component>();
                        components.Set(component.CType.Id, null);
                        if (component.Owner.ComponentBits == 0)
                        {
                            // TODO: Remove or pool entity
                        }
                        component.Release();
                    }
                }
            }

            foreach (var entity in _entitySyncList)
            {
                if (entity.SyncAction.HasFlag(SyncAction.Add))
                {
                    _entities[entity.Id] = entity;
                }
                if (entity.SyncAction.HasFlag(SyncAction.Remove))
                {
                    // TODO: Remove or pool entity
                    _entities.Remove(entity.Id);
                }
                if (entity.SyncAction.HasFlag(SyncAction.Resolve))
                    entity.ResolveDependencies();
            }
            foreach (var entitySystem in _systems)
            {
                entitySystem.SyncEntities(_entitySyncList);
            }
            _componentSyncList.Clear();
            _entitySyncList.Clear();
        }

        public void EnableSystem(EntitySystem system)
        {
            if (_systems.Add(system))
            {
                system.FilterEntities(_entities.Values);
            }
        }
        public void DisableSystem(EntitySystem system)
        {
            _systems.Remove(system);
        }

        internal IEnumerable<Component> GetComponents(Entity entity)
        {
            Bag<Component> components;
            if (_globalStorage.TryGetValue(entity.Id, out components))
                return components;
            return null;
        }
        
        internal Component GetComponent(Entity entity, ComponentType type)
        {
            Bag<Component> components;
            if (_globalStorage.TryGetValue(entity.Id, out components))
            {
                Component component;
                if (components.TryGetValue(type.Id, out component))
                    return component;
            }
            return null;
        }
    }
}
