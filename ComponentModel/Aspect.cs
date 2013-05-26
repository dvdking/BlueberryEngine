using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Blueberry.ComponentModel
{
    public class Aspect
    {
        private Aspect()
        {
            this.AnyTypesMap = 0;
            this.ExcludeTypesMap = 0;
            this.AllTypesMap = 0;
        }

        protected BigInteger AllTypesMap { get; set; }

        protected BigInteger ExcludeTypesMap { get; set; }

        protected BigInteger AnyTypesMap { get; set; }

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
            if (!(this.AllTypesMap > 0 || this.ExcludeTypesMap > 0 || this.AnyTypesMap > 0))
                return false;

            return ((this.AnyTypesMap & entity.ComponentBits) != 0 || this.AnyTypesMap == 0) &&
                   ((this.AllTypesMap & entity.ComponentBits) == this.AllTypesMap || this.AllTypesMap == 0) &&
                   ((this.ExcludeTypesMap & entity.ComponentBits) == 0);
        }

        public Aspect GetAll(params Type[] types)
        {
            foreach (var type in types)
                AllTypesMap |= ComponentType.GetBit(type);
            return this;
        }

        public Aspect GetExclude(params Type[] types)
        {
            foreach (var type in types)
                ExcludeTypesMap |= ComponentType.GetBit(type);
            return this;
        }

        public Aspect GetOne(params Type[] types)
        {
            foreach (var type in types)
                AnyTypesMap |= ComponentType.GetBit(type);
            return this;
        }
    }
}
