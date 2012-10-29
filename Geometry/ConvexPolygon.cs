using System;
using System.Collections.Generic;
using OpenTK;
using System.Drawing;

namespace Blueberry.Geometry
{
    public class ConvexPolygon : List<Vector2>
    {
        public ConvexPolygon(params Vector2[] points)
        {
            this.AddRange(points);
        }
        public void AddPoints(params Vector2[] points)
        {
            this.AddRange(points);
        }
        public int NextIndex(int index)
        {
            if (index == Count - 1)
            {
                return 0;
            }
            return index + 1;
        }
        public Vector2 NextVertex(int index)
        {
            return this[NextIndex(index)];
        }
        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return Count - 1;
            }
            return index - 1;
        }
        public Vector2 PreviousVertex(int index)
        {
            return this[PreviousIndex(index)];
        }

        public Vector2 GetEdge(int index)
        {
            return this[index] - NextVertex(index);
        }
        public Vector2 GetEdgeNormal(int index)
        {
            Vector2 n;
            n.X = NextVertex(index).Y - this[index].Y;
            n.Y = -(NextVertex(index).X - this[index].X);
            return n;
        }


        //Finds the edges that face away from a given location 'from'.
        //Returns:
        //A list of indices into 'edges'. In ccw order.
        public int[] GetBackfacingEdgeIndices(Vector2 from)
        {
            //assert(isValid());

            List<int> result = new List<int>();

            // find the indices of the two edges that face away from 'from' and that
            // have one adjacent edge facing towards 'from'
            int firstbackfacing = int.MaxValue, lastbackfacing = int.MaxValue;

            {
                bool prev_edge_front = false, cur_edge_front = false;
                for (int i = 0; i < Count; i++)
                {
                    if (Vector2.Dot(GetEdgeNormal(i), from - this[i]) < 0)
                        cur_edge_front = true;
                    else
                        cur_edge_front = false;

                    if (i != 0)
                    {
                        if (cur_edge_front && !prev_edge_front)
                            firstbackfacing = i;
                        else if (!cur_edge_front && prev_edge_front)
                            lastbackfacing = (i - 1);
                    }

                    prev_edge_front = cur_edge_front;
                }
            }

            // if no change between front and backfacing vertices was found,
            // we are inside the polygon, consequently all edges face backwards
            if (firstbackfacing == int.MaxValue && lastbackfacing == int.MaxValue)
            {
                for (int i = 0; i < Count; ++i)
                    result.Add(i);
                return result.ToArray();
            }
            // else, if one one of the changes was found, we missed the one at 0
            else if (firstbackfacing == int.MaxValue)
                firstbackfacing = 0;
            else if (lastbackfacing == int.MaxValue)
                lastbackfacing = (Count - 1);

            // if this is true, we can just put the indices in result in order
            if (firstbackfacing <= lastbackfacing)
            {
                for (int i = firstbackfacing; i <= lastbackfacing; ++i)
                    result.Add(i);
            }
            // else we must go from first to $ and from 0 to last
            else
            {
                for (int i = firstbackfacing; i < Count; ++i)
                    result.Add(i);
                for (int i = 0; i <= lastbackfacing; ++i)
                    result.Add(i);
            }

            return result.ToArray();
        }

        public Vector2[] GetBackfacingBorder(Vector2 from)
        {
            List<Vector2> result = new List<Vector2>();

            // find the indices of the two edges that face away from 'from' and that
            // have one adjacent edge facing towards 'from'
            int firstbackfacing = int.MaxValue, lastbackfacing = int.MaxValue;

            {
                bool prev_edge_front = false, cur_edge_front = false;
                for (int i = 0; i < Count; i++)
                {
                    if (Vector2.Dot(GetEdgeNormal(i), from - this[i]) < 0)
                        cur_edge_front = true;
                    else
                        cur_edge_front = false;

                    if (i != 0)
                    {
                        if (cur_edge_front && !prev_edge_front)
                            firstbackfacing = i;
                        else if (!cur_edge_front && prev_edge_front)
                            lastbackfacing = (i - 1);
                    }

                    prev_edge_front = cur_edge_front;
                }
            }

            // if no change between front and backfacing vertices was found,
            // we are inside the polygon, consequently all edges face backwards
            if (firstbackfacing == int.MaxValue && lastbackfacing == int.MaxValue)
            {
                for (int i = 0; i < Count; ++i)
                    result.Add(this[i]);
                return result.ToArray();
            }
            // else, if one one of the changes was found, we missed the one at 0
            else if (firstbackfacing == int.MaxValue)
                firstbackfacing = 0;
            else if (lastbackfacing == int.MaxValue)
                lastbackfacing = (Count - 1);

            // if this is true, we can just put the indices in result in order
            if (firstbackfacing <= lastbackfacing)
            {
                for (int i = firstbackfacing; i <= lastbackfacing; ++i)
                    result.Add(this[i]);
            }
            // else we must go from first to $ and from 0 to last
            else
            {
                for (int i = firstbackfacing; i < Count; ++i)
                    result.Add(this[i]);
                for (int i = 0; i <= lastbackfacing; ++i)
                    result.Add(this[i]);
            }
            result.Add(NextVertex(lastbackfacing));
            return result.ToArray();
        }

        public void GetBoundaryPoints(Vector2 point, out Vector2 min, out Vector2 max)
        {
            min = max = Vector2.Zero;
            bool end = false;
            bool start = false;
            bool begin = Vector2.Dot(this.GetEdgeNormal(0), this[0] - point) > 0;
            int i = 0;
            while (!end)
            {
                bool facing = Vector2.Dot(this.GetEdgeNormal(i), this[i] - point) > 0;
                if (!start)
                {
                    if (facing != begin)
                    {
                        start = true;
                        if (!facing)
                            min = this[i];
                        else
                            max = this[i];
                    }
                }
                else
                {
                    if (facing == begin)
                    {
                        end = true;
                        if (!facing)
                            min = this[i];
                        else
                            max = this[i];
                    }
                }
                i = NextIndex(i);
            }
        }

        public void RadialProjection(Vector2 point, out float min, out float max)
        {
            Vector2 p = this[0] - point;
            //p.Normalize();
            Vector2 axis = point.X > GetCentroid().X ? new Vector2(-1, 0) : new Vector2(1, 0);
            float angle = (float)Math.Atan2(MathUtils.Cross(p, axis), Vector2.Dot(p, axis));
            min = max = angle;
            for (int i = 1; i < Count; i++)
            {
                p = this[i] - point;
                //p.Normalize();
                angle = (float)Math.Atan2(MathUtils.Cross(p, axis), Vector2.Dot(p, axis));
                if (angle > max)
                    max = angle;
                else if (angle < min)
                    min = angle;
            }
            if (axis == new Vector2(-1, 0))
            {
                max += MathHelper.Pi;
                min += MathHelper.Pi;
            }
            while (max < 0)
                max += MathHelper.TwoPi;
            while (min < 0)
                min += MathHelper.TwoPi;
            while (max > MathHelper.TwoPi)
                max -= MathHelper.TwoPi;
            while (min > MathHelper.TwoPi)
                min -= MathHelper.TwoPi;
        }
        public void RadialProjection(Vector2 point, out float min, out float max, out Vector2 point_min, out Vector2 point_max)
        {
            Vector2 p = this[0] - point;
            //p.Normalize();
            Vector2 axis = point.X > GetCentroid().X ? new Vector2(-1, 0) : new Vector2(1, 0);
            float angle = (float)Math.Atan2(MathUtils.Cross(p, axis), Vector2.Dot(p, axis));
            min = max = angle;
            point_min = point_max = this[0];
            for (int i = 1; i < Count; i++)
            {
                p = this[i] - point;
                //p.Normalize();
                angle = (float)Math.Atan2(MathUtils.Cross(p, axis), Vector2.Dot(p, axis));
                if (angle > max)
                {
                    max = angle;
                    point_max = this[i];
                }
                else if (angle < min)
                {
                    min = angle;
                    point_min = this[i];
                }
            }
            if (axis == new Vector2(-1, 0))
            {
                max += MathHelper.Pi;
                min += MathHelper.Pi;
            }
            while (max < 0)
                max += MathHelper.TwoPi;
            while (min < 0)
                min += MathHelper.TwoPi;
            while (max > MathHelper.TwoPi)
                max -= MathHelper.TwoPi;
            while (min > MathHelper.TwoPi)
                min -= MathHelper.TwoPi;
        }


        /// <summary>Gets the signed area.</summary>
        /// <returns></returns>
        public float GetSignedArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return area;
        }

        /// <summary>Gets the area.</summary>
        /// <returns></returns>
        public float GetArea()
        {
            int i;
            float area = 0;

            for (i = 0; i < Count; i++)
            {
                int j = (i + 1) % Count;
                area += this[i].X * this[j].Y;
                area -= this[i].Y * this[j].X;
            }
            area /= 2.0f;
            return (area < 0 ? -area : area);
        }

        /// <summary>Gets the centroid.</summary>
        /// <returns></returns>
        public Vector2 GetCentroid()
        {
            // Same algorithm is used by Box2D

            Vector2 c = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            for (int i = 0; i < Count; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = this[i];
                Vector2 p3 = i + 1 < Count ? this[i + 1] : this[0];

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

        /// <summary>Gets the radius based on area.</summary>
        /// <returns></returns>
        public float GetRadius()
        {
            float area = GetSignedArea();

            double radiusSqrd = (double)area / MathHelper.Pi;
            if (radiusSqrd < 0)
            {
                radiusSqrd *= -1;
            }

            return (float)Math.Sqrt(radiusSqrd);
        }

        public Rectangle GetBoundingRect()
        {
            Vector2 lowerBound = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 upperBound = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < Count; ++i)
            {
                if (this[i].X < lowerBound.X)
                {
                    lowerBound.X = this[i].X;
                }
                if (this[i].X > upperBound.X)
                {
                    upperBound.X = this[i].X;
                }

                if (this[i].Y < lowerBound.Y)
                {
                    lowerBound.Y = this[i].Y;
                }
                if (this[i].Y > upperBound.Y)
                {
                    upperBound.Y = this[i].Y;
                }
            }
            return Extentions.CreateRect(lowerBound, upperBound);
        }

        public void Translate(Vector2 vector)
        {
            Translate(ref vector);
        }
      
        /// <summary>Translates the vertices with the specified vector.</summary>
        /// <param name="vector">The vector.</param>
        public void Translate(ref Vector2 vector)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Add(this[i], vector);
        }

        /// <summary>Scales the vertices with the specified vector.</summary>
        /// <param name="value">The Value.</param>
        public void Scale(ref Vector2 value)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Multiply(this[i], value);
        }

        /// <summary>Rotate the vertices with the defined value in radians.</summary>
        /// <param name="value">The amount to rotate by in radians.</param>
        public void Rotate(float value)
        {
            for (int i = 0; i < Count; i++)
                this[i] = Vector2.Transform(this[i], Quaternion.FromAxisAngle(Vector3.UnitZ,value));
        }

        /// <summary>
        /// Assuming the polygon is simple; determines whether the polygon is convex.
        /// NOTE: It will also return false if the input contains colinear edges.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if it is convex; otherwise, <c>false</c>.
        /// </returns>
        public bool IsConvex()
        {
            // Ensure the polygon is convex and the interior
            // is to the left of each edge.
            for (int i = 0; i < Count; ++i)
            {
                int i1 = i;
                int i2 = i + 1 < Count ? i + 1 : 0;
                Vector2 edge = this[i2] - this[i1];

                for (int j = 0; j < Count; ++j)
                {
                    // Don't check vertices on the current edge.
                    if (j == i1 || j == i2)
                    {
                        continue;
                    }

                    Vector2 r = this[j] - this[i1];

                    float s = edge.X * r.Y - edge.Y * r.X;

                    if (s <= 0.0f)
                        return false;
                }
            }
            return true;
        }

        public bool IsCounterClockWise()
        {
            //We just return true for lines
            if (Count < 3)
                return true;

            return (GetSignedArea() > 0.0f);
        }

        /// <summary>
        /// Forces counter clock wise order.
        /// </summary>
        public void ForceCounterClockWise()
        {
            if (!IsCounterClockWise())
            {
                Reverse();
            }
        }

        /// <summary>
        /// Projects to axis.
        /// </summary>
        /// <param name="axis">The axis.</param>
        /// <param name="min">The min.</param>
        /// <param name="max">The max.</param>
        public void ProjectToAxis(ref Vector2 axis, out float min, out float max)
        {
            // To project a point on an axis use the dot product
            float dotProduct = Vector2.Dot(axis, this[0]);
            min = dotProduct;
            max = dotProduct;

            for (int i = 0; i < Count; i++)
            {
                dotProduct = Vector2.Dot(this[i], axis);
                if (dotProduct < min)
                {
                    min = dotProduct;
                }
                else
                {
                    if (dotProduct > max)
                    {
                        max = dotProduct;
                    }
                }
            }
        }
        /// <summary>
        /// Returns a positive number if c is to the left of the line going from a to b.
        /// </summary>
        /// <returns>Positive number if point is left, negative if point is right, 
        /// and 0 if points are collinear.</returns>
        public static float Area(ref Vector2 a, ref Vector2 b, ref Vector2 c)
        {
            return a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y);
        }
        /// <summary>
        /// Winding number test for a point in a polygon.
        /// </summary>
        /// See more info about the algorithm here: http://softsurfer.com/Archive/algorithm_0103/algorithm_0103.htm
        /// <param name="point">The point to be tested.</param>
        /// <returns>-1 if the winding number is zero and the point is outside
        /// the polygon, 1 if the point is inside the polygon, and 0 if the point
        /// is on the polygons edge.</returns>
        public int PointInPolygon(ref Vector2 point)
        {
            // Winding number
            int wn = 0;

            // Iterate through polygon's edges
            for (int i = 0; i < Count; i++)
            {
                // Get points
                Vector2 p1 = this[i];
                Vector2 p2 = this[NextIndex(i)];

                // Test if a point is directly on the edge
                Vector2 edge = p2 - p1;
                float area = Area(ref p1, ref p2, ref point);
                if (area == 0f && Vector2.Dot(point - p1, edge) >= 0f && Vector2.Dot(point - p2, edge) <= 0f)
                {
                    return 0;
                }
                // Test edge for intersection with ray from point
                if (p1.Y <= point.Y)
                {
                    if (p2.Y > point.Y && area > 0f)
                    {
                        ++wn;
                    }
                }
                else
                {
                    if (p2.Y <= point.Y && area < 0f)
                    {
                        --wn;
                    }
                }
            }
            return (wn == 0 ? -1 : 1);
        }
    }
}
