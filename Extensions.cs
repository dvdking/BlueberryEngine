using System;
using System.Drawing;
using OpenTK;
using Blueberry.Geometry;

namespace Blueberry
{
    public static class Extentions
    {
        public static bool Contains(this Rectangle r, Circle circle)
        {
            return (r.X > circle.X - circle.Radius || r.Right < circle.X + circle.Radius || r.Y > circle.Y - circle.Radius || r.Bottom < circle.Y - circle.Radius);
        }
        public static void Contains(this Rectangle r, ref Circle circle, out bool result)
        {
            result = (r.X > circle.X - circle.Radius || r.Right < circle.X + circle.Radius || r.Y > circle.Y - circle.Radius || r.Bottom < circle.Y - circle.Radius);
        }
        public static bool Contains(this Rectangle r, Vector2 point)
        {
            if (r.X <= point.X && point.X < r.X + r.Width && r.Y <= point.Y)
                return point.Y < r.Y + r.Height;
            else
                return false;
        }

        public static void Contains(this Rectangle r, ref Vector2 value, out bool result)
        {
            result = r.X <= value.X && value.X < r.X + r.Width && r.Y <= value.Y && value.Y < r.Y + r.Height;
        }
        public static Vector2 Center(this Rectangle r)
        {
            return new Vector2(r.X + r.Width / 2, r.Y + r.Height / 2);
        }
        public static Vector2 Transform(this Vector2 v, Matrix4 matrix)
        {
            float num1 = (float)((double)v.X * (double)matrix.M11 + (double)v.Y * (double)matrix.M21) + matrix.M41;
            float num2 = (float)((double)v.X * (double)matrix.M12 + (double)v.Y * (double)matrix.M22) + matrix.M42;
            return new Vector2(num1,num2);
        }
        public static bool IntersectsWith(this Rectangle rect, Circle circle)
        {
            float testX = circle.X;
            float testY = circle.Y;
            if (testX < rect.Left) testX = rect.Left;
            if (testX > rect.Right) testX = rect.Right;
            if (testY < rect.Top) testY = rect.Top;
            if (testY > rect.Bottom) testY = rect.Bottom;
            return ((circle.X - testX) * (circle.X - testX) + (circle.Y - testY) * (circle.Y - testY)) < circle.Radius * circle.Radius;
        }
        public static void IntersectsWith(this Rectangle rect, ref Circle circle, out bool result)
        {
            float testX = circle.X;
            float testY = circle.Y;
            if (testX < rect.Left) testX = rect.Left;
            if (testX > rect.Right) testX = rect.Right;
            if (testY < rect.Top) testY = rect.Top;
            if (testY > rect.Bottom) testY = rect.Bottom;
            result = ((circle.X - testX) * (circle.X - testX) + (circle.Y - testY) * (circle.Y - testY)) < circle.Radius * circle.Radius;
        }
        public static Rectangle CreateRect(Vector2 p1, Vector2 p2)
        {
            Rectangle rect = Rectangle.Empty; 
            rect.X = (int)(p1.X < p2.X ? p1.X : p2.X);
            rect.Y = (int)(p1.Y < p2.Y ? p1.Y : p2.Y);
            rect.Width = (int)Math.Abs(p1.X - p2.X);
            rect.Height = (int)Math.Abs(p1.Y - p2.Y);
            return rect;
        }
        public static Rectangle CreateRect(Point p1, Point p2)
        {
            Rectangle rect = Rectangle.Empty;
            rect.X = p1.X < p2.X ? p1.X : p2.X;
            rect.Y = p1.Y < p2.Y ? p1.Y : p2.Y;
            rect.Width = Math.Abs(p1.X - p2.X);
            rect.Height = Math.Abs(p1.Y - p2.Y);
            return rect;
        }


    }
}