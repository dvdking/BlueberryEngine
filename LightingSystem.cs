using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Blueberry.Geometry;
using OpenTK;
using System.Drawing;
using Blueberry.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Blueberry
{
    public class LightingSystem
    {
        struct ShadowCaster
        {
            public ConvexPolygon poly;
            public Vector2 position;
        }
        private static LightingSystem _instance;
        //-------------

        Rectangle[] array;

        //-------------


        public static LightingSystem Instance
        {
            get 
            {
                if (_instance == null)
                    _instance = new LightingSystem();
                return _instance;
            }
        }

        private List<Light> lights;
        private List<ShadowCaster> casters;

        private uint _rendertex;
        private int width;
        private int height;
        public Texture umbra_tex;
        public Texture penumbra_tex;
        public LightingSystem()
        {
            lights = new List<Light>();
            casters = new List<ShadowCaster>();

            // initialize dynamic texture
            int[] view = new int[4];
            GL.GetInteger(GetPName.Viewport, view);
            width = view[2];
            height = view[3];
            byte[] texdata = new byte[width * height * 4];
            for (int i = 0; i < texdata.Length; i++) texdata[i] = 255;
            GL.GenTextures(1, out _rendertex);
            GL.BindTexture(TextureTarget.Texture2D, _rendertex);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Four, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, texdata);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        public void AddShadowCaster(ConvexPolygon poly, Vector2 position)
        {
            casters.Add(new ShadowCaster() { poly = poly, position = position });
        }
        public void AddShadowCaster(Rectangle rect)
        {
            ConvexPolygon poly = new ConvexPolygon( new Vector2(-rect.Width/2, -rect.Height/2), 
                                                    new Vector2(rect.Width/2, -rect.Height/2), 
                                                    new Vector2(rect.Width/2, rect.Height/2), 
                                                    new Vector2(-rect.Width/2, rect.Height/2));
            casters.Add(new ShadowCaster() { poly = poly, position = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2) });
        }
        public void AddShadowCaster(Circle circle)
        {
            throw new NotImplementedException();
        }
        public void AddLight(Light light)
        {
            lights.Add(light);
        }
        public void TestTile(Rectangle[] array)
        {
            this.array = array;
        }
        public void ClearSystem()
        {
            lights.Clear();
            casters.Clear();
        }
        public void ClearLights()
        {
            lights.Clear();
        }
        public void ClearShadowCasters()
        {
            casters.Clear();
        }

        public void Begin()
        {
            Begin(Matrix4.Identity);
        }

        // прорисовка освещения в текстуру
        public void Begin(Matrix4 transform)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadMatrix(ref transform);

            float[] val = new float[3];
            GL.GetFloat(GetPName.ColorClearValue, val);
            GL.ClearColor(0f, 0f, 0f, 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.Blend);

            foreach (var light in lights)
            {
                // очищение альфа канала
                GL.ColorMask(false, false, false, true);
                GL.Clear(ClearBufferMask.ColorBufferBit);
                // прорисовка теней в альфа канал
                GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.Blend);
                GL.Color4(0f, 0f, 0f, 1f);
                foreach (var blocker in casters)
                    light.CastShadow(blocker.poly, blocker.position, true);
                GL.Color4(0f, 0f, 0f, 0f);
                GL.Disable(EnableCap.Blend);
                if(array != null)
                    foreach (var tile in array)
                    {
                        GL.Begin(BeginMode.Quads);
                        GL.Vertex2(tile.X, tile.Y);
                        GL.Vertex2(tile.Right, tile.Top);
                        GL.Vertex2(tile.Right, tile.Bottom);
                        GL.Vertex2(tile.Left, tile.Bottom);
                        GL.End();
                    }
                GL.Enable(EnableCap.Blend);
                // отрисовка света
                GL.ColorMask(true, true, true, false);

                GL.BlendFunc(BlendingFactorSrc.OneMinusDstAlpha, BlendingFactorDest.One);
                light.DrawLight();
            }


            // копирование нарисованого в текстуру
            GL.BindTexture(TextureTarget.Texture2D, _rendertex);
            GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb8, 0, 0, width, height, 0);
            GL.ColorMask(true, true, true, true);
            GL.ClearColor(val[0], val[1], val[2], 0f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            GL.PopMatrix();
            
        }

        // применение освещения путем отрисовки результирующей текстуры поверх сцены
        public void End()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.BlendFunc(BlendingFactorSrc.DstColor, BlendingFactorDest.Zero);
            GL.Enable(EnableCap.Texture2D);
            GL.BindTexture(TextureTarget.Texture2D, _rendertex);
            GL.Color3(Color.White);
            GL.Begin(BeginMode.Quads);
            GL.TexCoord2(0, 1); GL.Vertex2(0, 0);
            GL.TexCoord2(0, 0); GL.Vertex2(0, height);
            GL.TexCoord2(1, 0); GL.Vertex2(width,height);
            GL.TexCoord2(1, 1); GL.Vertex2(width, 0);
            GL.End();
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }
    }
}
