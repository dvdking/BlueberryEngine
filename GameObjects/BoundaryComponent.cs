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
        private float _x, _y;

        public Vector2 Position
        {
            get { return new Vector2(_x, _y); }
            set
            {
                _x = value.X;
                _y = value.Y;
            }
        }

        public Point Cell
        {
            get
            {
                return new Point((int)(Position.X / 64), (int)(Position.Y / 64));
            }
            set
            {

                _x = value.X * 64 + Size.Width / 2; _y = value.Y * 64 + Size.Height / 2;
            }
        }

        private Size _size;
        public Size Size
        {
            get { return new Size(_size.Width, _size.Height); }
            set { _size.Width = value.Width; _size.Height = value.Height; }
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


        
		public Transform()
        {
        }

        public override void ProccesMessage(Messages.IMessage message)
        {

        }
    }
}
