using System;
using System.Drawing;
using OpenTK;

namespace Blueberry.Geometry
{
    public struct Circle
    {
        public float X;
        public float Y;
        public float Radius;

        public Vector2 Position
        {
            get { return new Vector2(X, Y); }
        }

        public Rectangle Rectangle
        {
            get { return new Rectangle((int)(X - Radius), (int)(Y - Radius), (int)(Radius * 2), (int)(Radius * 2)); }
        }

        public bool IsEmpty
        {
            get { return this.Radius == 0; }
        }

        public static Circle Empty
        {
            get { return new Circle(); }
        }

        public Circle (float x, float y, float radius)
		{
			this.X = x;
			this.Y = y;
			this.Radius = radius;
		}
		public Circle (Point position, float radius)
		{
			this.X = position.X;
			this.Y = position.Y;
			this.Radius = radius;
		}
		public Circle (Vector2 position, float radius)
		{
			this.X = position.X;
			this.Y = position.Y;
			this.Radius = radius;
		}
		
        public void Offset(Point amount)
        {
            this.X += amount.X;
            this.Y += amount.Y;
        }
        public void Offset(Vector2 amount)
        {
            this.X += (float)Math.Round(amount.X);
            this.Y += (float)Math.Round(amount.Y);
        }
        public void Offset(int offsetX, int offsetY)
        {
            this.X += offsetX;
            this.Y += offsetY;
        }
        public void Inflate(int amount)
		{
			this.Radius += amount;
		}
		
		#region Contains
        public bool Contains(Circle circle)
        {
            return Math.Sqrt((X - circle.X) * (X - circle.X) + (Y - circle.Y) * (Y - circle.Y)) + circle.Radius <= Radius;
        }
        public bool Contains(Rectangle rect)
        {
            return Contains(rect.Left, rect.Top) && Contains(rect.Right, rect.Top) && Contains(rect.Right, rect.Bottom) && Contains(rect.Left, rect.Bottom);
        }
        public bool Contains(Point point)
        {
            return (X - point.X) * (X - point.X) + (Y - point.Y) * (Y - point.Y) <= Radius * Radius;
        }
        public bool Contains(Vector2 point)
        {
            return (X - point.X) * (X - point.X) + (Y - point.Y) * (Y - point.Y) <= Radius * Radius;
        }
        public bool Contains(int x, int y)
		{
			return (X - x) * (X - x) + (Y - y) * (Y - y) <= Radius * Radius;
		}
		#endregion
		
		#region IntersectsWith
        public bool IntersectsWith(Circle circle)
        {
            return Radius + circle.Radius >= Math.Sqrt((X - circle.X) * (X - circle.X) + (Y - circle.Y) * (Y - circle.Y));
        }
        public bool IntersectsWith(Rectangle rect)
        {
            float testX = X;
            float testY = Y;
            if (testX < rect.Left) testX = rect.Left;
            if (testX > rect.Right) testX = rect.Right;
            if (testY < rect.Top) testY = rect.Top;
            if (testY > rect.Bottom) testY = rect.Bottom;
            return ((X - testX) * (X - testX) + (Y - testY) * (Y - testY)) < Radius * Radius;
        }
		#endregion
        public override string ToString()
        {
            return "Radius = " + Radius + " Position = " + Position;
        }
		
    }
}
