using System;
using Blueberry.Graphics;
using Blueberry.GameObjects;
using System.Drawing;
using System.Diagnostics;
using OpenTK.Graphics;

namespace Blueberry
{
	public class SpriteRenderer:Component, IDrawable
    {
		public Color4 Color = Color4.White;
		public Texture Texture;
        public Material Material;

        public SpriteRenderer()
        {
        }

		#region IDrawable implementation

		public void Draw(float dt)
		{
			var t = Owner.Transform;
			SpriteBatch.Please.DrawTexture(Texture, Material, t.Position.X, t.Position.Y, t.Size.Width, t.Size.Height, Texture.Bounds, Color, t.Rotation);
		}

		#endregion
    }
}

