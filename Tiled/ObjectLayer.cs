using System;
using System.Collections.Generic;
using System.Drawing;

namespace Blueberry.Tiled
{
    /// <summary>
    /// Represents an object layer
    /// </summary>
    public class ObjectLayer
    {
        /// <summary>
        /// Optional name of the layer
        /// </summary>
        public string Name;

        /// <summary>
        /// Opacity of the layer, 1 is opaque and 0 is complete transparent
        /// </summary>
        public float Opacity;

        /// <summary>
        /// A color value of white, with the alpha component set to the layer Opacity
        /// </summary>
        public Color OpacityColor;

        /// <summary>
        /// True if the layer is visible
        /// </summary>
        public bool Visible;

        /// <summary>
        /// Custom properties
        /// </summary>
        public Dictionary<string, Property> Properties;

        /// <summary>
        /// Optional color for the layer, alpha is set to the value of Opacity
        /// </summary>
        public Color? Color;

        /// <summary>
        /// Collection of objects on this layer
        /// </summary>
        public MapObject[] MapObjects;
    }
}
