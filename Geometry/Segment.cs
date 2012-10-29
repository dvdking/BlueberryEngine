using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;

namespace Blueberry.Geometry
{
    public struct Segment
    {
        public Line Line { get { return new Line(point1, point2); } }

        public Vector2 point1, point2;

        public Segment(Vector2 p, Vector2 q)
        {
            point1 = p;
            point2 = q;
        }

        public Segment(float x1, float y1, float x2, float y2)
        {
            point1.X = x1;
            point1.Y = y1;
            point2.X = x2;
            point2.Y = y2;
        }

        public static bool Intersection(Vector2 start1, Vector2 end1, Vector2 start2, Vector2 end2, out Vector2 intersection)
        {
            Vector2 dir1 = end1 - start1;
            Vector2 dir2 = end2 - start2;

            //считаем уравнения прямых проходящих через отрезки
            float a1 = -dir1.Y;
            float b1 = +dir1.X;
            float d1 = -(a1 * start1.X + b1 * start1.Y);

            float a2 = -dir2.Y;
            float b2 = +dir2.X;
            float d2 = -(a2 * start2.X + b2 * start2.Y);

            //подставляем концы отрезков, для выяснения в каких полуплоскотях они
            float seg1_line2_start = a2 * start1.X + b2 * start1.Y + d2;
            float seg1_line2_end = a2 * end1.X + b2 * end1.Y + d2;

            float seg2_line1_start = a1 * start2.X + b1 * start2.Y + d1;
            float seg2_line1_end = a1 * end2.X + b1 * end2.Y + d1;

            //если концы одного отрезка имеют один знак, значит он в одной полуплоскости и пересечения нет.
            if (seg1_line2_start * seg1_line2_end >= 0 || seg2_line1_start * seg2_line1_end >= 0)
            {
                intersection.X = float.NaN;
                intersection.Y = float.NaN;
                return false;
            }
            float u = seg1_line2_start / (seg1_line2_start - seg1_line2_end);
            intersection = start1 + u * dir1;

            return true;
        }

        //public static Vector2 IntersectionPoint(Line n, Line m)
        //{
        //    float zn = MathUtils.Determinant(m.a, m.b, n.a, n.b);
        //    if (Math.Abs(zn) < MathUtils.EPS)
        //        return new Vector2(float.NaN, float.NaN);
        //    Vector2 res;
        //    res.X = -MathUtils.Determinant(m.c, m.b, n.c, n.b) / zn;
        //    res.Y = -MathUtils.Determinant(m.a, m.c, n.a, n.c) / zn;
        //    return res;
        //}
    }
}