using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    internal class EntityPool
    {
        private Stack<Entity> _entities; 

        internal EntityPool()
        {
            _entities = new Stack<Entity>();
        }

        internal void Push(Entity entity)
        {
            _entities.Push(entity);
        }

        internal Entity CreateEntity(string tag)
        {
           if(_entities.Count == 0)
                return new Entity(tag);

            var entity = _entities.Pop();
            entity.Init();
            entity.Tag = tag;
            return entity;
        }

        public Entity CreateEntity()
        {
            if (_entities.Count == 0)
                return new Entity();
            var entity = _entities.Pop();
            entity.Init();
            return entity;
        }
    }
}
