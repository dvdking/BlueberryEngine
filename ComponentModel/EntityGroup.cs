using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class EntityGroup
    {
        private static Dictionary<string, EntityGroup> groups;
        private static BigInteger nextBit;

        static EntityGroup()
        {
            groups = new Dictionary<string, EntityGroup>();
            nextBit = 1;
        }
        public static EntityGroup GetGroup(string name)
        {
            EntityGroup group;
            if (!groups.TryGetValue(name, out group))
                group = groups[name] = new EntityGroup(name);
            return group;
        }
        public static void LeaveAll(Entity entity)
        {
            foreach (var group in groups.Values)
                if ((entity.GroupBits & group.GroupBit) != 0)
                    group._entities.Remove(entity);
            entity.GroupBits = 0;
        }


        private string _name;
        private HashSet<Entity> _entities;
        public readonly BigInteger GroupBit;

        public IEnumerable<Entity> Entities { get { return _entities; } }

        private EntityGroup(string name)
        {
            _entities = new HashSet<Entity>();
            _name = name;
            GroupBit = nextBit;
            nextBit <<= 1;
        }

        public void Add(Entity entity)
        {
            if ((entity.GroupBits & GroupBit) == 0)
            {
                entity.GroupBits |= GroupBit;
                _entities.Add(entity);
            }
        }
        public void Remove(Entity entity)
        {
            if ((entity.GroupBits & GroupBit) != 0)
            {
                entity.GroupBits ^= GroupBit;
                _entities.Remove(entity);
            }
        }
        public bool Contains(Entity entity)
        {
            return (entity.GroupBits & GroupBit) != 0;
        }
    }
}
