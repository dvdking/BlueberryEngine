using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Blueberry.Graphics;
using Blueberry.SceneManagement;
using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.SceneManagement
{
    public class Sprite : Actor, IBoundedActor
    {
        protected Tile tile;
        protected Vector2 position;
        protected Vector2 velocity;
        protected Vector2 scale;

        public Vector2 Position { get { return position; } set { position = value; } }

        public Vector2 Velocity { get { return velocity; } set { velocity = value; } }

        public Vector2 Scale { get { return scale; } set { scale = value; } }

        public Tile Tile { get { return tile; } set { tile = value; } }

        public RectangleF Bounds
        {
            get
            {
                return new RectangleF(position.X - (tile.source.Width * scale.X) / 2,
                position.Y - (tile.source.Height * scale.Y) / 2,
                tile.source.Width * scale.X, tile.source.Height * scale.Y);
            }
        }

        public override void Draw(float dt)
        {
            SpriteBatch.Please.DrawTexture(tile.texture, position, tile.source, Color4.White);
        }

        public override void Update(float dt)
        {
            position += velocity * dt;
        }

        public override void Load()
        {
        }
    }
}