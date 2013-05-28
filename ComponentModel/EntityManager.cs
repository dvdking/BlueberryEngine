using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class EntityManager
    {
        private static EntityManager _instance;
        public static EntityManager Please
        {
            get 
            { 
                if (_instance == null) 
                    _instance = new EntityManager();
                return _instance;
            }
        }

        public bool NeedSync
        {
            get { return _syncList.Count > 0; }
        }

        private HashSet<Entity> _globalStorage;
 
        private Dictionary<string, HashSet<Entity>> _entityGroups;
        private HashSet<Entity> _syncList;

        private Dictionary<string, EntityDefinition> _definitions;

        private List<EntityFilter> _filters;

        public Entity CreateEntity(string name = "", params Component[] components)
        {
            Entity entity = new Entity(name);
            entity.AddComponents(components);
            AddToSync(entity, SyncState.Add | SyncState.Refresh);
            return entity;
        }

        public Entity CreateEntity(string name = "", params Type[] componentTypes)
        {
            Entity entity = new Entity(name);//todo: entities pool
            entity.AddComponents(componentTypes);
            AddToSync(entity, SyncState.Add | SyncState.Refresh);
            
            return entity;
        }

        public Entity CreateEntity(string name = "")
        {
            Entity entity = new Entity(name); //todo: entities pool
            AddToSync(entity, SyncState.Add);
            //todo: entities pool
            return entity;
        }

        public void UtilizeEntity(Entity entity)
        {
            var components = entity.GetComponents();
            entity.ClearComponents();
            AddToSync(entity, SyncState.Remove);
           
        }

        public void DefineEntity(string definitionName, EntityDefinition definition)
        {
            _definitions[definitionName] = definition;
        }
        public Entity CreateFromDefinition(string definitionName, string entityName)
        {
            return _definitions[definitionName].Create(entityName);
        }

        public EntityManager()
        {
            _globalStorage = new HashSet<Entity>();
            _entityGroups = new Dictionary<string, HashSet<Entity>>();
            _syncList = new HashSet<Entity>();
            _definitions = new Dictionary<string, EntityDefinition>();
            _filters = new List<EntityFilter>();
        }

        public void CreateGroup(string name)
        {
            if(!_entityGroups.ContainsKey(name))
                _entityGroups[name] = new HashSet<Entity>();
        }

        public void Broadcast(IMessage message)
        {
            foreach (var entity in _globalStorage)
            {
                entity.Broadcast(message);
            }
        }

        public void Broadcast(string groupName, IMessage message)
        {
            HashSet<Entity> group;
            if (_entityGroups.TryGetValue(groupName, out group))
            {
                foreach (var entity in group)
                {
                    entity.Broadcast(message);
                }
            }
        }

        public void Send(Entity reciever, IMessage message)
        {
            reciever.Broadcast(message);
        }

        public Entity Find(string name)
        {
            foreach (var entity in _globalStorage)
            {
                if (entity.Name == name)
                    return entity;
            }
            return null;
        }

        internal void AddToSync(Entity entity, SyncState syncState)
        {
            entity.SyncState |= syncState;
            _syncList.Add(entity);
        }
        
        public void Sync()
        {
            foreach (var entity in _syncList)
            {
                if (entity.SyncState.HasFlag(SyncState.Add))
                {
                    _globalStorage.Add(entity);
                }
                else if (entity.SyncState.HasFlag(SyncState.Remove))
                {
                    _globalStorage.Remove(entity);
                }
                if (entity.SyncState.HasFlag(SyncState.Refresh))
                    entity.ResolveComponents();
            }
            UpdateFilters(_syncList);
            _syncList.Clear();
        }

        public void ApplyFilter(EntityFilter filter)
        {
            _filters.Add(filter);
            filter.FilterEntities(_globalStorage);
        }

        public void DisableFilter(EntityFilter filter)
        {
            _filters.Remove(filter);
            filter.Clear();
        }

        private void UpdateFilters(Entity entity)
        {
            if (_filters.Count > 0)
                foreach (var entityFilter in _filters)
                    entityFilter.CheckEntity(entity);
        }

        private void UpdateFilters(IEnumerable<Entity> entities)
        {
            if (_filters.Count > 0)
                foreach (var entityFilter in _filters)
                    entityFilter.CheckEntities(entities);
        }

        public void AddToGroup(Entity entity, string groupName)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            HashSet<Entity> group;
            if (!_entityGroups.ContainsKey(groupName))
            {
                group = new HashSet<Entity>();
                _entityGroups[groupName] = group;
            }
            else
                group = _entityGroups[groupName];
            group.Add(entity);
            UpdateFilters(entity);
        }
        
        public void RemoveFromGroup(Entity entity, string groupName)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (!_entityGroups.ContainsKey(groupName))
                return;
            _entityGroups[groupName].Remove(entity);
            UpdateFilters(entity);
        }
        
        public void RemoveFromAllGroups(Entity entity)
        {
            foreach (var entityGroup in _entityGroups.Values)
                entityGroup.Remove(entity);
            UpdateFilters(entity);
        }
        public void ToggleGroup(Entity entity, string groupName)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            HashSet<Entity> group;
            if (!_entityGroups.ContainsKey(groupName))
            {
                group = new HashSet<Entity>();
                _entityGroups[groupName] = group;
                group.Add(entity);
            }
            else
            {
                group = _entityGroups[groupName];
                if (group.Contains(entity))
                    group.Remove(entity);
                else
                    group.Add(entity);
            }
            UpdateFilters(entity);
        }

        public bool InGroup(Entity entity, string groupName)
        {
            HashSet<Entity> group;
            if (_entityGroups.TryGetValue(groupName, out group))
                return group.Contains(entity);
            return false;
        }
        public bool InEveryGroup(Entity entity, params string[] groups)
        {
            return InEveryGroup(entity, groups);
        }
        public bool InAnyGroup(Entity entity, params string[] groups)
        {
            return InAnyGroup(entity, groups);
        }
        public bool InEveryGroup(Entity entity, IEnumerable<string> groups)
        {
            foreach (var groupName in groups)
            {
                HashSet<Entity> group;
                if (_entityGroups.TryGetValue(groupName, out group))
                    if (!group.Contains(entity)) return false;
            }
            return true;
        }
        public bool InAnyGroup(Entity entity, IEnumerable<string> groups)
        {
            foreach (var groupName in groups)
            {
                HashSet<Entity> group;
                if (_entityGroups.TryGetValue(groupName, out group))
                    if (group.Contains(entity)) return true;
            }
            return false;
        }
    }
}
