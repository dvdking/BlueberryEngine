using System;
using System.Collections.Generic;
using System.Drawing;

namespace Blueberry.Tiled
{
    /// <summary>
    /// Represents an object in an ObjectLayer
    /// </summary>
    public class MapObject
    {
        /// <summary>
        /// Optional name of the object
        /// </summary>
        public string Name;

        /// <summary>
        /// Optional type of the object
        /// </summary>
        public string Type;

        /// <summary>
        /// Map location of the object in pixles
        /// </summary>
        public Rectangle Bounds;

        /// <summary>
        /// Optional image tile index in the Map.SourceTiles collection
        /// </summary>
        public int? TileID;

        /// <summary>
        /// True if the object is visible.  Possible in the TMX map format, but no UI exists for the user to change.
        /// </summary>
        public bool Visible;

        /// <summary>
        /// Custom properties for this object
        /// </summary>
        public Dictionary<String, Property> Properties;

        /// <summary>
        /// A sequence of lines that form a closed shape; can be null if there is no Polygon with this object
        /// </summary>
        public Polygon Polygon;

        /// <summary>
        /// A sequence of lines; can be null if there is no Polyline with this object
        /// </summary>
        public Polyline Polyline;
    }
}
