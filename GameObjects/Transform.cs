using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Blueberry.GameObjects
{
	public class Transform:Component
    {
		public float X, Y;

		public float Rotation;

        public Vector2 Position
        {
			get { return new Vector2(X, Y); }
            set
            {
				X = value.X;
				Y = value.Y;
            }
        }

		public float Width, Height;
		public SizeF Size
        {
			get { return new SizeF(Width, Height); }
			set { Width = value.Width; Height = value.Height; }
        }

        public virtual RectangleF Bounds
        {
            get
            {
                return new RectangleF((Position.X - Size.Width / 2), (Position.Y - Size.Height / 2), Size.Width, Size.Height);
            }
        }

        public Vector2 Center
        {
            get { return new Vector2(Position.X + Size.Width, Position.Y + Size.Height); }
        }

        public float Left { get { return Bounds.Left; } }
        public float Right { get { return Bounds.Right; } }
        public float Top { get { return Bounds.Top; } }
        public float Bottom { get { return Bounds.Bottom; } }
    }
}
