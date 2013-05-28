using System;

namespace Blueberry.Tiled 
{
    /// <summary>
    /// Represents a custom property value
    /// </summary>
    public struct Property 
    {
        /// <summary>
        /// Raw String value of the propery
        /// </summary>
        public string Value;
        /// <summary>
        /// Value converted to a float, null if conversion failed
        /// </summary>
        public float? AsSingle;
        /// <summary>
        /// Value converted to an int, null if conversion failed
        /// </summary>
        public int? AsInt32;
        /// <summary>
        /// Value converted to a boolean, null if conversion failed
        /// </summary>
        public bool? AsBoolean;

        /// <summary>
        /// Creates a property from a raw string value
        /// </summary>
        /// <param name="value">Value of the property</param>
        public static Property Create(string value)
        {
            var p = new Property {Value = value};

            bool testBool;
            if (bool.TryParse(value, out testBool))
                p.AsBoolean = testBool;
            else
                p.AsBoolean = null;

            float testSingle;
            if (float.TryParse(value, out testSingle))
                p.AsSingle = testSingle;
            else
                p.AsSingle = null;

            int testInt;
            if (int.TryParse(value, out testInt))
                p.AsInt32 = testInt;
            else
                p.AsInt32 = null;
            
            return p;
        }
    }
}
