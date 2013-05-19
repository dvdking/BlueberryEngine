using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class Aspect
    {
        protected Aspect()
        {
            this.AnyTypesMap = new HashSet<ComponentType>();
            this.ExcludeTypesMap = new HashSet<ComponentType>();
            this.AllTypesMap = new HashSet<ComponentType>();
        }

        protected HashSet<ComponentType> AllTypesMap { get; set; }

        protected HashSet<ComponentType> ExcludeTypesMap { get; set; }

        protected HashSet<ComponentType> AnyTypesMap { get; set; }

        public static Aspect All(params Type[] types)
        {
            return new Aspect().GetAll(types);
        }

        public static Aspect Empty()
        {
            return new Aspect();
        }

        public static Aspect Exclude(params Type[] types)
        {
            return new Aspect().GetExclude(types);
        }

        public static Aspect Any(params Type[] types)
        {
            return new Aspect().GetOne(types);
        }

        public virtual bool Interests(Entity entity)
        {
            if (AllTypesMap.Count == 0 && AnyTypesMap.Count == 0 && ExcludeTypesMap.Count == 0) return false;
            
            var entityTypes = entity.GetComponentTypes();
            foreach (var ctype in AllTypesMap)
            {
                if (!entity.ContainsComponent(ctype)) return false;
            }
            bool any = false;
            foreach (var ctype in entity.GetComponentTypes())
            {
                if (ExcludeTypesMap.Contains(ctype)) return false;
                if (AnyTypesMap.Contains(ctype)) any = true;
            }
            if(AnyTypesMap.Count > 0)
                return any;
            return true;
        }

        public Aspect GetAll(params Type[] types)
        {
            foreach (var type in types)
                AllTypesMap.Add(ComponentTypeManager.Please.GetComponentTypeOf(type));
            return this;
        }

        public Aspect GetExclude(params Type[] types)
        {
            foreach (var type in types)
                this.ExcludeTypesMap.Add(ComponentTypeManager.Please.GetComponentTypeOf(type));
            return this;
        }

        public Aspect GetOne(params Type[] types)
        {
            foreach (var type in types)
                this.AnyTypesMap.Add(ComponentTypeManager.Please.GetComponentTypeOf(type));
            return this;
        }
    }
}
