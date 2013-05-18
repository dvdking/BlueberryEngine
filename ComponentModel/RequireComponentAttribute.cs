using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.ComponentModel
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class RequireComponentAttribute : Attribute 
    {
        /// <summary>
        /// Gets or sets the entity that the dependency should be retrieved from.
        /// </summary>
        /// <remarks>
        /// If this is not specified, it defaults to the entity that the component is currently attached to.
        /// </remarks>
        public string FromEntityNamed
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets whether the dependency should be automatically injected if possible.
        /// </summary>
        public bool Automatically
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets whether the dependency can be a subclass of the specified component type.
        /// </summary>
        public bool AllowDerivedTypes
        {
            get;
            set;
        }
    }
}
