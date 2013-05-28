using System;
using System.Drawing;

namespace Blueberry.Tiled 
{
    /// <summary>
    /// Represents a placed tiled on the map
    /// </summary>
    public class TileData 
    {
        /// <summary>
        /// Index of the source tile in the Map.SourceTiles collection
        /// </summary>
        public int SourceID;
        /// <summary>
        /// Horizontal flipping state of the placed tile
        /// </summary>
        public bool VerticalFlip;
        /// <summary>
        /// Vertical flipping state of the placed tile
        /// </summary>
        public bool HorisontalFlip;
        /// <summary>
        /// Location in pixels of the tile on the map
        /// </summary>
        public Rectangle Target;
        /// <summary>
        /// Rotation of the placed tile, in radians
        /// </summary>
        public float Rotation;
    }
}
