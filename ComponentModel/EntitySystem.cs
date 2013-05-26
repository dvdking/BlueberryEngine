using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace Blueberry.ComponentModel
{
    public class EntitySystem
    {
        private static BigInteger _nextBit = 1;

        public readonly BigInteger SystemBit;


        private readonly HashSet<Entity> _appropriateEntities;
        private readonly Aspect _filter;
        private BigInteger _groupBits;

        public Aspect Filter { get { return _filter; } }

        public EntitySystem(Aspect filter)
        {
            _filter = filter;
            _appropriateEntities = new HashSet<Entity>();
            
            SystemBit = _nextBit;
            _nextBit <<= 1;
        }

        public EntitySystem() : this(Aspect.Empty())
        {}

        public void IncludeGroup(string groupName)
        {
            _groupBits |= EntityGroup.GetGroup(groupName).GroupBit;
        }
        public void IncludeGroup(EntityGroup group)
        {
            _groupBits |= group.GroupBit;
        }

        public void ExcludeGroup(string groupName)
        {
            _groupBits ^= EntityGroup.GetGroup(groupName).GroupBit;
        }
        public void ExcludeGroup(EntityGroup group)
        {
            _groupBits ^= group.GroupBit;
        }

        public bool GroupIncluded(string groupName)
        {
            return (EntityGroup.GetGroup(groupName).GroupBit & _groupBits) != 0;
        }
        public bool GroupIncluded(EntityGroup group)
        {
            return (group.GroupBit & _groupBits) != 0;
        }

        public void FilterEntities(IEnumerable<Entity> entities)
        {
            _appropriateEntities.Clear();
            foreach (var entity in entities)
            {
                if (!_filter.Interests(entity)) continue;
                if (_groupBits != 0)
                {
                    if((entity.GroupBits & _groupBits) != 0)
                        AddEntity(entity);
                }
                else
                    AddEntity(entity);
            }
        }

        public void SyncEntity(Entity entity)
        {
            bool contain = (entity.SystemBits & SystemBit) != 0;
            if ((entity.SyncAction & (SyncAction.Add | SyncAction.Resolve | SyncAction.Regroup)) != 0)
            {
                if (_filter.Interests(entity))
                {
                    if (_groupBits != 0)
                    {
                        if ((_groupBits & entity.GroupBits) != 0)
                        {
                            if (!contain) AddEntity(entity);
                        }
                        else
                        {
                            if (contain) RemoveEntity(entity);
                        }

                    }
                    else
                    {
                        if (!contain) AddEntity(entity);
                    }
                }
                else
                {
                    if (contain) RemoveEntity(entity);
                }
            }
            if (entity.SyncAction.HasFlag(SyncAction.Remove) && contain) RemoveEntity(entity);
        }

        public void SyncEntities(IEnumerable<Entity> entities)
        {
            foreach (var entity in entities)
            {
                SyncEntity(entity);
            }
        }

        private void RemoveEntity(Entity entity)
        {
            entity.SystemBits ^= SystemBit;
            _appropriateEntities.Remove(entity);
        }
        private void AddEntity(Entity entity)
        {
            entity.SystemBits |= SystemBit;
            _appropriateEntities.Add(entity);
        }
        public void Clear()
        {
            _appropriateEntities.Clear();
        }
        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }
        public override bool Equals(object obj)
        {
            return GetType() == obj.GetType();
        }

        public void Process()
        {
            ProcessEntities(_appropriateEntities);
        }
        protected virtual void ProcessEntities(IEnumerable<Entity> entities)
        {
        }
    }
}
