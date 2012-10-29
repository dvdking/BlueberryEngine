using System;
using System.Collections.Generic;
using OpenTK;
using Blueberry.Geometry;
using System.Drawing;
using OpenTK.Graphics.OpenGL;
using Blueberry.Graphics;
using System.Linq;
using OpenTK.Input;

namespace Blueberry
{
    public class Light
    {
        float _intensity;
        Vector2 _position;
        float _size;
        float _range;
        Color _color;
        public Vector2 Position { get { return _position; } set { _position = value; } }
        public float X { get { return _position.X; } set { _position.X = value; } }
        public float Y { get { return _position.Y; } set { _position.Y = value; } }
        public float Intencity { get { return _intensity; } set { _intensity = Math.Max(0, Math.Min(1, value)); } }
        public float Size { get { return _size; } set { _size = Math.Max(0, value); } }
        public float Range { get { return _range; } set { _range = Math.Abs(value); } }
        public Color Color { get { return _color; } set { _color = value; } }

        public Light()
            : this(Vector2.Zero, Color.White, 5, 128, 0.5f)
        { }

        public Light(Vector2 position, Color color)
            : this(position, color, 5, 128, 0.5f)
        { }

        public Light(Vector2 position, Color color, float size, float range, float intensity)
        {
            this._position = position;
            this._color = color;
            this._size = size;
            this._range = range;
            this._intensity = intensity;
        }

        public void DrawLight()
        {
            GL.Disable(EnableCap.Texture2D);
            GL.Begin(BeginMode.TriangleFan);

            GL.Color4(_color.R, _color.G, _color.B, _color.A * _intensity);
            GL.Vertex2(_position);
            GL.Color4(0, 0, 0, 0);
            for (int i = 0; i < 41; ++i)
                GL.Vertex2(_position + MathUtils.FromPolar(_range, (float)((2 * Math.PI) * i / 40)));

            GL.End();
            GL.Enable(EnableCap.Texture2D);
        }

        //Gets the direction that marks the beginning of total shadow
        //for the given point.
        Vector2 getTotalShadowStartDirection(Vector2 shadowCasterPosition, Vector2 at)
        {
            return extendDir(at - (Position + getLightDisplacement(shadowCasterPosition, at)));
        }

        //Displaces the light pos by sourceradius orthogonal to the line from
        //reference to the light's position. Used for calculating penumbra size.
        Vector2 getLightDisplacement(Vector2 shadowCasterPosition, Vector2 reference)
        {

            Vector2 lightdisp = MathUtils.MakePerpTo(reference - Position);
            lightdisp.Normalize();
            lightdisp *= Size;
            if (Vector2.Dot(lightdisp, reference - shadowCasterPosition) < 0)
                lightdisp *= -1;
            return lightdisp;
        }
        /**
                scales a vector with respect to the light radius
                used for penumbra and umbra lights where the tips
                are not supposed to be visible
            **/
        Vector2 extendDir(Vector2 dir)
        {
            return Vector2.Multiply(Vector2.Normalize(dir), Range * 1.5f);
        }
        public void CastShadow(Vector2[] shadowLine, bool penumbra)
        {

            // Calculate centroid

            Vector2 position = Vector2.Zero;
            float area = 0.0f;

            const float inv3 = 1.0f / 3.0f;
            Vector2 pRef = Vector2.Zero;
            for (int i = 0; i < shadowLine.Length; ++i)
            {
                // Triangle vertices.
                Vector2 p1 = pRef;
                Vector2 p2 = shadowLine[i];
                Vector2 p3 = i + 1 < shadowLine.Length ? shadowLine[i + 1] : shadowLine[0];

                Vector2 e1 = p2 - p1;
                Vector2 e2 = p3 - p1;

                float D = MathUtils.Cross(e1, e2);

                float triangleArea = 0.5f * D;
                area += triangleArea;

                // Area weighted centroid
                position += triangleArea * inv3 * (p1 + p2 + p3);
            }

            // Centroid
            position *= 1.0f / area;

        
            //// if the light source is completely surrounded by the blocker, don't draw its shadow
            //if (shadowLine.Length == shadowLine.Length)
            //    return;
            //
            // build penumbrae (soft shadows), cast from the edges
            //

            Penumbra rightpenumbra;
            {
                Vector2 startdir = extendDir(shadowLine[0] - (this.Position - getLightDisplacement(position, shadowLine[0])));
                rightpenumbra.sections = new List<Penumbra.Section>();
                rightpenumbra.sections.Add(new Penumbra.Section(shadowLine[0], startdir, 0));



                for (int i = 0; i < shadowLine.Length - 1; ++i)
                {
                    float wanted = Math.Abs(MathUtils.AngleBetween(startdir, getTotalShadowStartDirection(position, shadowLine[i])));
                    float available = Math.Abs(MathUtils.AngleBetween(startdir, shadowLine[i + 1] - shadowLine[i]));

                    if (wanted < available)
                    {
                        rightpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[i],
                            getTotalShadowStartDirection(position, shadowLine[i]),
                            1));
                        break;
                    }
                    else
                    {
                        rightpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[i + 1],
                            extendDir(shadowLine[i + 1] - shadowLine[i]),
                            available / wanted));
                    }
                }
            }

            Penumbra leftpenumbra;
            {
                Vector2 startdir = extendDir(shadowLine[shadowLine.Length - 1] - (this.Position - getLightDisplacement(position, shadowLine[shadowLine.Length - 1])));
                leftpenumbra.sections = new List<Penumbra.Section>();
                leftpenumbra.sections.Add(new Penumbra.Section(
                    shadowLine[shadowLine.Length - 1],
                    startdir,
                    0));
                for (int i = 0; i < shadowLine.Length - 1; ++i)
                {
                    float wanted = Math.Abs(MathUtils.AngleBetween(startdir, getTotalShadowStartDirection(position, shadowLine[shadowLine.Length - i - 1])));
                    float available = Math.Abs(MathUtils.AngleBetween(startdir, shadowLine[shadowLine.Length - i - 2] - shadowLine[shadowLine.Length - i - 1]));

                    if (wanted < available)
                    {
                        leftpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[shadowLine.Length - i - 1],
                            getTotalShadowStartDirection(position, shadowLine[shadowLine.Length - i - 1]),
                            1));
                        break;
                    }
                    else
                    {
                        leftpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[shadowLine.Length - i - 2],
                            extendDir(shadowLine[shadowLine.Length - i - 2] - shadowLine[shadowLine.Length - i - 1]),
                            available / wanted));
                    }
                }
            }

            //
            // build umbrae (hard shadows), cast between the insides of penumbrae
            //

            Umbra umbra;
            umbra.sections = new List<Umbra.Section>();

            umbra.sections.Add(new Umbra.Section(rightpenumbra.sections.Last()._base, rightpenumbra.sections.Last().direction));

            for (int i = rightpenumbra.sections.Count - 1; i < shadowLine.Length - leftpenumbra.sections.Count + 1; i++)
                umbra.sections.Add(new Umbra.Section(shadowLine[i], extendDir(Vector2.Multiply(leftpenumbra.sections.Last().direction + rightpenumbra.sections.Last().direction, 0.5f))));

            umbra.sections.Add(new Umbra.Section(leftpenumbra.sections.Last()._base, leftpenumbra.sections.Last().direction));

            //
            // draw shadows to alpha
            //
            umbra.draw();
            rightpenumbra.draw();
            leftpenumbra.draw();
        }
        public void CastShadow(ConvexPolygon poly, Vector2 position, bool penumbra)
        {
            // get the line that blocks light for the blocker and light combination
            // move the light position towards blocker by its sourceradius to avoid
            // popping of penumbrae
            int[] edgeIndices = poly.GetBackfacingEdgeIndices((this.Position + Vector2.Multiply(Vector2.Normalize(position - this.Position), this.Size)) - position);

            Vector2[] shadowLine = new Vector2[edgeIndices.Length + 1];
            shadowLine[0] = position + poly[edgeIndices[0]];
            for (int i = 0; i < edgeIndices.Length; i++)
                shadowLine[i + 1] = position + poly.NextVertex(edgeIndices[i]);


            // if the light source is completely surrounded by the blocker, don't draw its shadow
            if (shadowLine.Length == poly.Count + 1)
                return;
            //
            // build penumbrae (soft shadows), cast from the edges
            //

            Penumbra rightpenumbra;
            {
                Vector2 startdir = extendDir(shadowLine[0] - (this.Position - getLightDisplacement(position, shadowLine[0])));
                rightpenumbra.sections = new List<Penumbra.Section>();
                rightpenumbra.sections.Add(new Penumbra.Section(shadowLine[0], startdir, 0));



                for (int i = 0; i < shadowLine.Length - 1; ++i)
                {
                    float wanted = Math.Abs(MathUtils.AngleBetween(startdir, getTotalShadowStartDirection(position, shadowLine[i])));
                    float available = Math.Abs(MathUtils.AngleBetween(startdir, shadowLine[i + 1] - shadowLine[i]));

                    if (wanted < available)
                    {
                        rightpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[i],
                            getTotalShadowStartDirection(position, shadowLine[i]),
                            1));
                        break;
                    }
                    else
                    {
                        rightpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[i + 1],
                            extendDir(shadowLine[i + 1] - shadowLine[i]),
                            available / wanted));
                    }
                }
            }

            Penumbra leftpenumbra;
            {
                Vector2 startdir = extendDir(shadowLine[shadowLine.Length - 1] - (this.Position - getLightDisplacement(position, shadowLine[shadowLine.Length - 1])));
                leftpenumbra.sections = new List<Penumbra.Section>();
                leftpenumbra.sections.Add(new Penumbra.Section(
                    shadowLine[shadowLine.Length - 1],
                    startdir,
                    0));
                for (int i = 0; i < shadowLine.Length - 1; ++i)
                {
                    float wanted = Math.Abs(MathUtils.AngleBetween(startdir, getTotalShadowStartDirection(position, shadowLine[shadowLine.Length - i - 1])));
                    float available = Math.Abs(MathUtils.AngleBetween(startdir, shadowLine[shadowLine.Length - i - 2] - shadowLine[shadowLine.Length - i - 1]));

                    if (wanted < available)
                    {
                        leftpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[shadowLine.Length - i - 1],
                            getTotalShadowStartDirection(position, shadowLine[shadowLine.Length - i - 1]),
                            1));
                        break;
                    }
                    else
                    {
                        leftpenumbra.sections.Add(new Penumbra.Section(
                            shadowLine[shadowLine.Length - i - 2],
                            extendDir(shadowLine[shadowLine.Length - i - 2] - shadowLine[shadowLine.Length - i - 1]),
                            available / wanted));
                    }
                }
            }

            //
            // build umbrae (hard shadows), cast between the insides of penumbrae
            //

            Umbra umbra;
            umbra.sections = new List<Umbra.Section>();

            umbra.sections.Add(new Umbra.Section(rightpenumbra.sections.Last()._base, rightpenumbra.sections.Last().direction));

            for (int i = rightpenumbra.sections.Count - 1; i < shadowLine.Length - leftpenumbra.sections.Count + 1; i++)
                umbra.sections.Add(new Umbra.Section(shadowLine[i], extendDir(Vector2.Multiply(leftpenumbra.sections.Last().direction + rightpenumbra.sections.Last().direction, 0.5f))));

            umbra.sections.Add(new Umbra.Section(leftpenumbra.sections.Last()._base, leftpenumbra.sections.Last().direction));

            //
            // draw shadows to alpha
            //
            umbra.draw();
            rightpenumbra.draw();
            leftpenumbra.draw();
        }
    }
    //    Penumbrae are the regions of half-shadow generated by voluminous
    //    light sources.

    //    They are represented by a series of sections, each containing a line
    //    and an intensity. The intensity gives the strength of the shadow on
    //    that line between 0. (fully lit) and 1. (complete shadow).
    public struct Penumbra
    {
        // line line between 'base' and 'base + direction' has the
        // shadow intensity 'intensity'
        public struct Section
        {
            public Vector2 _base;
            public Vector2 direction;
            public float intensity;

            public Section(Vector2 _base, Vector2 direction, float intensity)
            {
                this._base = _base;
                this.direction = direction;
                this.intensity = intensity;
            }
        }
        public List<Section> sections;

        public void draw()
        {
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, LightingSystem.Instance.penumbra_tex.ID);

            GL.Begin(BeginMode.Triangles);

            for (int i = 0; i < sections.Count - 1; i++)
            {
                GL.TexCoord2(0f, 1f);
                GL.Vertex2(sections[i]._base);

                GL.TexCoord2(sections[i].intensity, 0);
                GL.Vertex2(sections[i]._base + sections[i].direction);

                GL.TexCoord2(sections[i + 1].intensity, 0);
                GL.Vertex2(sections[i + 1]._base + sections[i + 1].direction);
            }

            GL.End();

            GL.Disable(EnableCap.Texture2D);
        }
    }
    //    Umbrae are the regions of full shadow behind light blockers.
    //    Represented by a series of lines.
    public struct Umbra
    {
        public struct Section
        {
            public Vector2 _base;
            public Vector2 direction;
            public Section(Vector2 _base, Vector2 direction)
            {
                this._base = _base;
                this.direction = direction;
            }
        }
        public List<Section> sections;

        public void draw()
        {
            BeginMode style = BeginMode.TriangleStrip;
            // the umbra draw regions (if considered quads) can sometimes 
            // be concave, so use triangles and start once from left and 
            // once from right to minimize problems
            GL.Begin(style);
            for (int i = 0; i < sections.Count / 2 + 1; i++)
            {
                GL.Vertex2(sections[i]._base);
                GL.Vertex2(sections[i]._base + sections[i].direction);
            }
            GL.End();

            GL.Begin(style);
            for (int i = sections.Count - 1; i >= sections.Count / 2; i--)
            {
                GL.Vertex2(sections[i]._base);
                GL.Vertex2(sections[i]._base + sections[i].direction);
            }
            GL.End();
        }
    }
}
