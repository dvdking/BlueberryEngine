using System;
using System.Collections.Generic;
using System.Drawing;
using Blueberry.Graphics;

namespace Blueberry.Tiled
{
    /// <summary>
    /// Represents an image in an ImageLayer
    /// </summary>
    public class MapImage
    {
        /// <summary>
        /// Optional name of the tileset
        /// </summary>
        public string Name;
        /// <summary>
        /// Custom properties
        /// </summary>
        public Dictionary<String, Property> Properties;
        /// <summary>
        /// Full path of the image referenced in the TMX file
        /// </summary>
        public string ImageFileName;
        /// <summary>
        /// Transparent color as set in the Tiled editor; null if not set
        /// </summary>
        public Color? ImageTransparentColor;
        /// <summary>
        /// Width of image in pixels
        /// </summary>
        public int ImageWidth;
        /// <summary>
        /// Height of image in pixels
        /// </summary>
        public int ImageHeight;
        /// <summary>
        /// Image as an Texture instance; null if Map.LoadTextures is false
        /// </summary>
        public Texture Texture;
    }
}
