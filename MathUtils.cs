using System;
using OpenTK;

namespace Blueberry
{
    public static class MathUtils
    {
        public const double EPS = 1E-9;

        public static float Determinant(float a, float b, float c, float d) { return (a * d - b * c); }

        public static float Cross(Vector2 v1, Vector2 v2)
        {
            return v1.X * v2.Y - v1.Y * v2.X;
        }
        public static void RotateVector2(ref Vector2 vec, float angle)
        {
            float x = (float)(vec.X * Math.Cos(angle) - vec.Y * Math.Sin(angle));
            float y = (float)(vec.X * Math.Sin(angle) + vec.Y * Math.Cos(angle));
            vec.X = x;
            vec.Y = y;
        }
        public static Vector2 RotateVector2(Vector2 vec, float angle)
        {
            float x = (float)(vec.X * Math.Cos(angle) - vec.Y * Math.Sin(angle));
            float y = (float)(vec.X * Math.Sin(angle) + vec.Y * Math.Cos(angle));
            return new Vector2(x,y);
        }
        public static Vector2 FromPolar(float length, float angle)
        {
            Vector2 v;
            v.X = length * (float)Math.Cos(angle);
            v.Y = length * (float)Math.Sin(angle);
            return v;
        }

        public static float AngleBetween(Vector2 v1, Vector2 v2)
        {
            return (float)Math.Atan2(Cross(new Vector2(v1.X, v1.Y), v2), Vector2.Dot(new Vector2(v1.X, v1.Y), v2));
        }

        public static Vector2 MakePerpTo(Vector2 p)
        {
            Vector2 v;
            v.Y = p.X;
            v.X = -p.Y;
            return v;
        }

        public static float AcuteAngle(float angle)
        {
            angle = Math.Abs(angle);
            if (angle > MathHelper.Pi) angle = MathHelper.TwoPi - angle;
            return angle;
        }

        public static int Clamp(int x, int min, int max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        public static float Clamp(float x, float min, float max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        public static double Clamp(double x, double min, double max)
        {
            if (x > max) return max;
            if (x < min) return min;
            return x;
        }

        public static float Lerp(float value1, float value2, float amount)
        {
            return value1 + (value2 - value1) * amount;
        }

        public static void Lerp(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 res)
        {
            res.X = value1.X + (value2.X - value1.X) * amount;
            res.Y = value1.Y + (value2.Y - value1.Y) * amount;
            //return value1 + (value2 - value1) * amount;
        }
        public static Vector2 Lerp(Vector2 value1,  Vector2 value2, float amount)
        {
            Vector2 res;
            res.X = value1.X + (value2.X - value1.X) * amount;
            res.Y = value1.Y + (value2.Y - value1.Y) * amount;
            return res;
            //return value1 + (value2 - value1) * amount;
        }

        public static bool IsRight(Vector2 point, Vector2 linePoint, Vector2 lineDirection)
        {
            Vector2 n;
            n.X = lineDirection.Y;
            n.Y = -(lineDirection.X);
            if (Vector2.Dot(n, point - linePoint) < 0)
                return false;
            else
                return true;
        }

        public static Vector2 Centroid(Vector2[] points)
        {
            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            for (int i = 0; i < points.Length; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = points[i];
                Vector2 p3 = i + 1 < points.Length ? points[i + 1] : points[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                c += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            c *= 1.0f / area;
            return c;
        }
    }
}