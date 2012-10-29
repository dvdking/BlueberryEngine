using System;
using OpenTK;

namespace Blueberry.Geometry
{
    public struct Line
    {
        public float a, b, c;

        public Line(Vector2 p, Vector2 q)
        {
            a = p.Y - q.Y;
            b = q.X - p.X;
            c = -a * p.X - b * p.Y;
            Normalize();
        }
        public Line(float x1, float y1, float x2, float y2)
        {
            a = y1 - y2;
            b = x2 - x1;
            c = -a * x1 - b * y1;
            Normalize();
        }
        
        public static Vector2 IntersectionPoint(Line n, Line m)
        {
            float zn = MathUtils.Determinant(m.a, m.b, n.a, n.b);
            if (Math.Abs(zn) < MathUtils.EPS)
                return new Vector2(float.NaN, float.NaN);
            Vector2 res;
            res.X = -MathUtils.Determinant(m.c, m.b, n.c, n.b) / zn;
            res.Y = -MathUtils.Determinant(m.a, m.c, n.a, n.c) / zn;
            return res;
        }
        public void Normalize()
        {
            float z = (float)Math.Sqrt(a * a + b * b);
            if (Math.Abs(z) > MathUtils.EPS)
                a /= z; b /= z; c /= z;
        }

        public double Distance(Vector2 p)
        {
            return a * p.X + b * p.Y + c;
        }
    }
}
