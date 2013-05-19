using System;
using System.Collections.Generic;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class EntityFilter
    {
        private HashSet<Entity> appropriateEntities;
        private Aspect _filter;
        private HashSet<string> groups; 

        public Aspect Filter { get { return _filter; } }

        public EntityFilter(Aspect filter)
        {
            _filter = filter;
            appropriateEntities = new HashSet<Entity>();
            groups = new HashSet<string>();
        }

        public void IncludeGroup(string group)
        {
            groups.Add(group);
        }

        public void ExcludeGroup(string group)
        {
            groups.Remove(group);
        }

        public bool GroupIncluded(string group)
        {
            if (groups.Count == 0) return true;
            return groups.Contains(group);
        }

        public void FilterEntities(IEnumerable<Entity> entities)
        {
            appropriateEntities.Clear();
            foreach (var entity in entities)
            {
                if (_filter.Interests(entity))
                {
                    if (groups.Count > 0)
                    {
                        if (entity.InAnyGroup(groups))
                            appropriateEntities.Add(entity);
                    }
                    else
                        appropriateEntities.Add(entity);
                }
            }
        }

        public void CheckEntity(Entity entity)
        {
            bool contain = appropriateEntities.Contains(entity);
            if (_filter.Interests(entity))
            {
                if (groups.Count > 0)
                {
                    if (entity.InAnyGroup(groups))
                    {
                        if (!contain)
                            appropriateEntities.Add(entity);
                    }
                    else
                    {
                        if (contain)
                            appropriateEntities.Remove(entity);
                    }
                }
                else
                {
                    if (!contain)
                        appropriateEntities.Add(entity);
                }
            }
            else
            {
                if(contain)
                    appropriateEntities.Remove(entity);
            }
        }
        public void CheckEntities(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
                CheckEntity(entity);
        }
        public void RemoveEntity(Entity entity)
        {
            appropriateEntities.Remove(entity);
        }

        public void Clear()
        {
            appropriateEntities.Clear();
        }
    }
}
