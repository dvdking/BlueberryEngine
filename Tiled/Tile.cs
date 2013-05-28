using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace Blueberry.Tiled 
{
    /// <summary>
    /// Represents a source tile image's metadata
    /// </summary>
    public class Tile 
    {
        /// <summary>
        /// Index of the Tileset in the Map.Tilesets collection this tile belongs to
        /// </summary>
        public int TilesetID;
        /// <summary>
        /// Custom properties
        /// </summary>
        public Dictionary<string, Property> Properties;
        /// <summary>
        /// Area in the texture of the tileset that represents this tile
        /// </summary>
        public Rectangle Source;
        /// <summary>
        /// Origin of this tile for rendering; set to the center of the tile
        /// </summary>
        public Vector2 Origin;
    }
}
