using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Blueberry.Diagnostics;
using Blueberry.Graphics.Fonts;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;

namespace Blueberry.Graphics
{
	public class SpriteBatch : IDisposable, IDiagnosable
    {
        private struct BatchItem
        {
            public int texture;
            public int startIndex;
            public BeginMode mode;
        }

        int pixelTex = -1;
        int framebuffer = -1;
        Shader defaultShader;
        Shader current;

        private static SpriteBatch instance;

        public static SpriteBatch Instance
        {
            get
            {
                if (instance == null)
                    instance = new SpriteBatch();
                return instance;
            }
        }

        private VertexBuffer vbuffer;

        Queue<BatchItem> dipQueue;
        
        private RectangleF tempRect = RectangleF.Empty;
        private Vector2 texCoordTL = Vector2.Zero;
        private Vector2 texCoordBR = Vector2.Zero;
        private Matrix4 trans;
        private Matrix4 proj;
        public Matrix4 Projection { get { return proj; } }

        private int proj_uniform_loc;
        private int view_uniform_loc;
		
        #region Statistic
        private int elementsCounter = 0;
        private int elementsProcessed = 0;
         
        public int GetProcessedElementsCount()
        {     	
        	return elementsProcessed;	
        }
        internal void FrameCheckPoint()
        {
        	elementsProcessed = elementsCounter;
        	elementsCounter = 0;
        }
        #endregion
        
        public SpriteBatch()
        {
            dipQueue = new Queue<SpriteBatch.BatchItem>(25);
            
            vbuffer = new VertexBuffer(1024);
            Diagnostics.DiagnosticsCenter.Instance.Add(vbuffer);
            defaultShader = new Shader();
            
            if (Shader.Version < 3.3f)
            {
                defaultShader.LoadVertexSource(@"#version 120
                uniform mat4 projection, view;
                attribute vec2 vposition; 
                attribute vec4 vcolor; 
                attribute vec2 vtexcoord;
                varying vec4 fcolor; 
                varying vec2 ftexcoord;
                void main(void) 
                {
                    fcolor = vcolor;
                    ftexcoord = vtexcoord;
                    gl_Position = projection * view * vec4(vposition, 0, 1); 
                }");
                
                defaultShader.LoadFragmentSource(@"#version 120
                                          uniform sampler2D colorTexture;
                                          varying vec4 fcolor; varying vec2 ftexcoord;

                                          void main(void) { gl_FragColor = texture2D(colorTexture, ftexcoord) * fcolor; }");
                
            }
            else
            {
                defaultShader.LoadVertexSource(@"#version 330 core
                                    uniform mat4 projection, view;
                                    uniform int flip;
                                    in vec2 vposition; in vec4 vcolor; in vec2 vtexcoord;
                                    out vec4 fcolor; out vec2 ftexcoord;
                                    void main(void) {
                                    fcolor = vcolor;
                                    ftexcoord = vtexcoord;
                                    gl_Position = projection * view * vec4(vposition, 0, 1); 
                                    if(flip == 1) gl_Position.y *= -1; 
									}");
                defaultShader.LoadFragmentSource(@"#version 330 core
                                          uniform sampler2D colorTexture;
                                          in vec4 fcolor; in vec2 ftexcoord;
                                          out vec4 color;
                                          void main(void) { color = texture(colorTexture, ftexcoord) * fcolor; }");
            }
            defaultShader.Link();
            BindShader(this.defaultShader, "vposition", "vcolor", "vtexcoord", "projection", "view");


            int[] p = new int[4];
            GL.GetInteger(GetPName.Viewport, p);

            proj = Matrix4.CreateOrthographicOffCenter(p[0], p[2], p[3], p[1], 1, -1);

            GL.GenTextures(1, out pixelTex);
            GL.BindTexture(TextureTarget.Texture2D, pixelTex);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 0);

            float[] pix = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 1, 1, 0,
               PixelFormat.Bgra, PixelType.Float, pix);
        }
        public void Dispose()
		{
        	vbuffer.Dispose();
        }

        private void FlushBuffer(BeginMode mode, int offset, int count)
        {
            if (vbuffer.VertexDataLength != 0)
                GL.DrawElements(mode, count, DrawElementsType.UnsignedInt, offset * sizeof(float));
        }

        public void BindShader(Shader shader, string positionAttrib, string colorAttrib, string texcoordAttrib, string projectionUniform, string viewUniform)
        {
            vbuffer.ClearAttributeDeclarations();

            vbuffer.DeclareNextAttribute(positionAttrib, 2);
            vbuffer.DeclareNextAttribute(colorAttrib, 4);
            vbuffer.DeclareNextAttribute(texcoordAttrib, 2);

            vbuffer.Attach(shader);

            proj_uniform_loc = GL.GetUniformLocation(shader.Program, projectionUniform);
            view_uniform_loc = GL.GetUniformLocation(shader.Program, viewUniform);

            shader.Use();
            current = shader;
        }
        public void ResetShader()
        {
            BindShader(this.defaultShader, "vposition", "vcolor", "vtexcoord", "projection", "view");
        }
        
		private bool began = false; // if begin was called
        public void Begin()
        {
            Begin(Matrix4.Identity);
        }
        public void Begin(Matrix4 transform)
        {
        	if(began) throw new Exception("Call end first");
        	began = true;
        	
            trans = transform;
            vbuffer.ClearBuffer();
        }
		private bool flip = false;
        public void End()
        {
        	if(!began) throw new Exception("Call begin first");
        		
            int pr;
            GL.GetInteger(GetPName.CurrentProgram, out pr);
            if(current == null)
            {
                current = defaultShader;
                current.Use();
            } 
            if (current.Program != pr)
                current.Use();
            current.SetUniform(proj_uniform_loc, ref proj);
            current.SetUniform(view_uniform_loc, ref trans);
            current.SetUniform("flip", flip ? 1 : 0);
            // nothing to do
            if (dipQueue.Count != 0)
            {
	
	            vbuffer.Bind();
	            vbuffer.UpdateVertexBuffer();
	            vbuffer.UpdateIndexBuffer();
	
				do
				{
					var b = dipQueue.Dequeue();
					int count = (dipQueue.Count > 0 ? dipQueue.Peek().startIndex : vbuffer.IndexOffset) - b.startIndex;
					GL.BindTexture(TextureTarget.Texture2D, b.texture);
					FlushBuffer(b.mode, b.startIndex, count);
					elementsCounter += count;
				} while(dipQueue.Count > 0);
				last = new SpriteBatch.BatchItem() { texture = -1, startIndex = -1, mode = BeginMode.Triangles};
            }
			flip = false;
			began = false;
        }
		
        public void End(Texture target, bool clear, bool use_back_buffer = false)
        {
            flip = true;
            if (use_back_buffer)
            {
                if (clear)
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                End();
                GL.BindTexture(TextureTarget.Texture2D, target.ID);
                GL.CopyTexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 0, 0, target.Size.Width, target.Size.Height, 0);
                return;
            }
            if (GraphicsDevice.Instance.FramebufferSupport == FeatureSupport.Core)
            {
                if (framebuffer == -1)
                    GL.GenFramebuffers(1, out framebuffer);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                if (clear)
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                End();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            } else
            {
                if (framebuffer == -1)
                    GL.Ext.GenFramebuffers(1, out framebuffer);
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.Ext.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                if (clear)
                    GL.Clear(ClearBufferMask.ColorBufferBit);
                End();
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }
		
        BatchItem last;
        
        private bool TryPush(int texId, BeginMode mode)
        {
        	if(last.texture != texId || last.mode != mode)
        	{
        		last = new BatchItem(){ texture = texId, mode = mode, startIndex = vbuffer.IndexOffset};
        		dipQueue.Enqueue(last);
        		return true;
        	}
        	return false;
        }
        
        #region DrawTexture

        public unsafe void DrawTexture(Texture texture, float x, float y, float width, float height, RectangleF sourceRectangle, Color4 color,
                                float rotation = 0.0f, float xOrigin = 0.5f, float yOrigin = 0.5f,
                                bool flipHorizontally = false, bool flipVertically = false)
        {
            if (texture == null)
                throw new ArgumentException("texture");
            TryPush(texture.ID, BeginMode.Triangles);

            if (sourceRectangle.IsEmpty)
            {
                tempRect.X = 0;
                tempRect.Y = 0;
                tempRect.Width = texture.Size.Width;
                tempRect.Height = texture.Size.Height;
            }
            else
                tempRect = sourceRectangle;

            texCoordTL = texture.GetTextureCoordinate(new Vector2(tempRect.X, tempRect.Y));
            texCoordBR = texture.GetTextureCoordinate(new Vector2(tempRect.Right, tempRect.Bottom));

            if (flipVertically)
            {
                float temp = texCoordBR.Y;
                texCoordBR.Y = texCoordTL.Y;
                texCoordTL.Y = temp;
            }
            if (flipHorizontally)
            {
                float temp = texCoordBR.X;
                texCoordBR.X = texCoordTL.X;
                texCoordTL.X = temp;
            }

            #region Add vertices

            float dx = -xOrigin * width;
            float dy = -yOrigin * height;
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            int* ind = vbuffer.GetIndexPointerToFill(6);
            float* vert = vbuffer.GetVertexPointerToFill(4);
            *(ind++) = offset;
            *(ind++) = offset + 1;
            *(ind++) = offset + 2;
            *(ind++) = offset;
            *(ind++) = offset + 2;
            *(ind++) = offset + 3;            
            
            
            *(vert++) = x + dx * cos - dy * sin;
            *(vert++) = y + dx * sin + dy * cos;
            *(vert++) = color.R;
            *(vert++) = color.G;
            *(vert++) = color.B;
            *(vert++) = color.A;
            *(vert++) = texCoordTL.X;
            *(vert++) = texCoordTL.Y;
            	
            *(vert++) = x + (dx + width) * cos - dy * sin;
            *(vert++) = y + (dx + width) * sin + dy * cos;
            *(vert++) = color.R;
            *(vert++) = color.G;
            *(vert++) = color.B;
            *(vert++) = color.A;
            *(vert++) = texCoordBR.X;
            *(vert++) = texCoordTL.Y;
            
            *(vert++) =x + (dx + width) * cos - (dy + height) * sin;
            *(vert++) = y + (dx + width) * sin + (dy + height) * cos;
            *(vert++) = color.R;
            *(vert++) = color.G;
            *(vert++) =color.B;
            *(vert++) = color.A;
            *(vert++) = texCoordBR.X;
            *(vert++) = texCoordBR.Y;
            	
            *(vert++) =x + dx * cos - (dy + height) * sin;
            *(vert++) = y + dx * sin + (dy + height) * cos;
            *(vert++) = color.R;
            *(vert++) = color.G;
            *(vert++) =color.B;
            *(vert++) = color.A;
            *(vert++) = texCoordTL.X;
            *(vert++) = texCoordBR.Y;

            #endregion Add vertices
        }

        public void DrawTexture(Texture texture, float x, float y, RectangleF sourceRectangle, Color4 color,
                                float rotation = 0.0f, float xOrigin = 0.5f, float yOrigin = 0.5f, float xScale = 1, float yScale = 1,
                                bool flipHorizontally = false, bool flipVertically = false)
        {
            DrawTexture(texture, x, y,
                sourceRectangle != RectangleF.Empty ? sourceRectangle.Width * xScale : texture.Size.Width * xScale,
                sourceRectangle != RectangleF.Empty ? sourceRectangle.Height * yScale : texture.Size.Height * yScale,
                sourceRectangle, color, rotation, xOrigin, yOrigin, flipHorizontally, flipVertically);
        }

        public void DrawTexture(Texture texture, Vector2 position, RectangleF sourceRectangle, Color4 color,
                                float rotation, Vector2 origin, Vector2 scale,
                                bool flipHorizontally = false, bool flipVertically = false)
        {
            DrawTexture(texture, position.X, position.Y, sourceRectangle, color, rotation, origin.X, origin.Y, scale.X, scale.Y, flipHorizontally, flipVertically);
        }

        public void DrawTexture(Texture texture, RectangleF destinationRectangle, RectangleF sourceRectangle, Color4 color,
                                float rotation, Vector2 origin,
                                bool flipHorizontally = false, bool flipVertically = false)
        {
            DrawTexture(texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, sourceRectangle, color, rotation, origin.X, origin.Y, flipHorizontally, flipVertically);
        }

        public void DrawTexture(Texture texture, Vector2 position, RectangleF sourceRectangle, Color4 color)
        {
            DrawTexture(texture, position.X, position.Y, sourceRectangle, color);
        }

        public void DrawTexture(Texture texture, RectangleF destinationRectangle, RectangleF sourceRectangle, Color4 color)
        {
            DrawTexture(texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, sourceRectangle, color);
        }

        public void DrawTexture(Texture texture, Vector2 position, Color4 color)
        {
            DrawTexture(texture, position.X, position.Y, RectangleF.Empty, color);
        }

        public void DrawTexture(Texture texture, RectangleF destinationRectangle, Color4 color)
        {
            DrawTexture(texture, destinationRectangle.X, destinationRectangle.Y, destinationRectangle.Width, destinationRectangle.Height, RectangleF.Empty, color);
        }

        #endregion DrawTexture

        #region DrawLine
        public unsafe void DrawLine(float x1, float y1, float x2, float y2, float thickness, Color4 color)
        {
        	TryPush(this.pixelTex, BeginMode.Triangles);
            
        	Vector2 dir = new Vector2(x2 - x1, y2 - y1);
        	dir.NormalizeFast();
        	Vector2 perp = dir.PerpendicularLeft;
        	//perp.NormalizeFast();
        	float hth = thickness / 2f;
        	
        	#region Add vertices

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            int* ind = vbuffer.GetIndexPointerToFill(6);
            float* vert = vbuffer.GetVertexPointerToFill(4);
            *(ind++) = offset; *(ind++) = offset+1; *(ind++) = offset+2;
            *(ind++) = offset; *(ind++) = offset+2; *(ind++) = offset+3;
            
            *(vert++) = x1 + perp.X * hth; *(vert++) = y1 + perp.Y * hth; 
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = 0; *(vert++) = 0; 
			
            *(vert++) = x2 + perp.X * hth; *(vert++) = y2 + perp.Y * hth; 
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = 0; *(vert++) = 0; 
                       
            *(vert++) = x2 - perp.X * hth; *(vert++) = y2 - perp.Y * hth; 
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = 0; *(vert++) = 0; 
			
            *(vert++) = x1 - perp.X * hth; *(vert++) = y1 - perp.Y * hth; 
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = 0; *(vert++) = 0; 

            #endregion Add vertices
        }

        public void DrawLine(Vector2 start, Vector2 end, float thickness, Color4 color)
        {
            DrawLine(start.X, start.Y, end.X, end.Y,thickness, color);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color4 color)
        {
            DrawLine(start.X, start.Y, end.X, end.Y, 1.0f, color);
        }

        public void DrawLine(Vector2 start, float angle, float length, Color4 color)
        {
            DrawLine(start.X, start.Y, start.X + (float)(Math.Sin(angle + MathHelper.PiOver2) * length), start.Y + (float)(Math.Cos(angle + MathHelper.PiOver2) * length), 1.0f, color);
        }

        public void DrawLine(Vector2 start, float angle, float length, float thickness, Color4 color)
        {
            DrawLine(start.X, start.Y, start.X + (float)(Math.Sin(angle + MathHelper.PiOver2) * length), start.Y + (float)(Math.Cos(angle + MathHelper.PiOver2) * length), thickness, color);
        }

        public void DrawLine(Vector2 start, Vector2 direction, float length, float thickness, Color4 color)
        {
            DrawLine(start.X, start.Y, start.X + direction.X * length, start.Y + direction.Y * length, thickness, color);
        }
		
        #endregion DrawLine

        #region FillPolygon

        public void FillPolygon(Vector2[] points, Vector2 position, float rotation, float scale, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, rotation, scale, color);
        }

        public void FillPolygon(Vector2[] points, Vector2 position, float rotation, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, rotation, 1, color);
        }

        public void FillPolygon(Vector2[] points, Vector2 position, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, 0, 1, color);
        }

        public void FillPolygon(Vector2[] points, Color4 color)
        {
            FillPolygon(points, 0, 0, 0, 1, color);
        }

        public void FillPolygon(IEnumerable<Vector2> points, Vector2 position, float rotation, float scale, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, rotation, scale, color);
        }

        public void FillPolygon(IEnumerable<Vector2> points, Vector2 position, float rotation, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, rotation, 1, color);
        }

        public void FillPolygon(IEnumerable<Vector2> points, Vector2 position, Color4 color)
        {
            FillPolygon(points, position.X, position.Y, 0, 1, color);
        }

        public void FillPolygon(IEnumerable<Vector2> points, Color4 color)
        {
            FillPolygon(points, 0, 0, 0, 1, color);
        }

        public unsafe void FillPolygon(IEnumerable<Vector2> points, float x, float y, float rotation, float scale, Color4 color)
        {
            TryPush(this.pixelTex, BeginMode.Triangles);
            #region Add vertices

            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            int* ind = vbuffer.GetIndexPointerToFill(3 + (points.Count()-3)*3);
            float* vert = vbuffer.GetVertexPointerToFill(points.Count());
            for (int i = 0; i < points.Count(); i++) 
            {
                *(vert++) = x + points.ElementAt(i).X * cos * scale - points.ElementAt(i).Y * sin * scale;
                *(vert++) = y + points.ElementAt(i).X * sin * scale + points.ElementAt(i).Y * cos * scale;
                *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
                *(vert++) = 0; *(vert++) = 0;
            }
            *(ind++) = offset;
            *(ind++) = offset+1;
            *(ind++) = offset+2;

            for (int i = 3; i < points.Count(); i++)
            {              
                *(ind++) = offset;
            	*(ind++) = offset+i-1;
            	*(ind++) = offset+i;
            }

            #endregion Add vertices
        }

        public void FillRegularPolygon(int vertices, float size, Vector2 position, float rotation, Color4 color)
        {
            TryPush(this.pixelTex, BeginMode.Triangles);

            if (vertices < 3 || vertices > 360)
                throw new ArgumentException("Vertices must be in range from 3 to 360", "vertices");

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            int j = 0;
            for (int i = 2; i < vertices + 1; i++, j += 3)
                vbuffer.AddIndices(offset, offset + i - 1, offset + i);
            vbuffer.AddIndices(offset, offset + vertices, offset + 1);

            vbuffer.AddVertex(position.X, position.Y,
                                color.R, color.G, color.B, color.A, 0, 0);
            float degInRad;

            for (int i = 1; i < vertices + 1; i++)
            {
                degInRad = MathHelper.DegreesToRadians((360 / vertices) * (i - 1)) + rotation;

                vbuffer.AddVertex(position.X + (float)(Math.Cos(degInRad) * size / 2),
                                position.Y + (float)(Math.Sin(degInRad) * size / 2),
                                color.R, color.G, color.B, color.A, 0, 0);
            }
        }

        #endregion FillPolygon

        #region OutlinePolygon

        public void OutlinePolygon(Vector2[] points, Vector2 position, float rotation, float scale, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, rotation, scale, color);
        }

        public void OutlinePolygon(Vector2[] points, Vector2 position, float rotation, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, rotation, 1, color);
        }

        public void OutlinePolygon(Vector2[] points, Vector2 position, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, 0, 1, color);
        }

        public void OutlinePolygon(Vector2[] points, Color4 color)
        {
            OutlinePolygon(points, 0, 0, 0, 1, color);
        }

        public void OutlinePolygon(IEnumerable<Vector2> points, Vector2 position, float rotation, float scale, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, rotation, scale, color);
        }

        public void OutlinePolygon(IEnumerable<Vector2> points, Vector2 position, float rotation, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, rotation, 1, color);
        }

        public void OutlinePolygon(IEnumerable<Vector2> points, Vector2 position, Color4 color)
        {
            OutlinePolygon(points, position.X, position.Y, 0, 1, color);
        }

        public void OutlinePolygon(IEnumerable<Vector2> points, Color4 color)
        {
            OutlinePolygon(points, 0, 0, 0, 1, color);
        }

        public void OutlinePolygon(IEnumerable<Vector2> points, float x, float y, float rotation, float scale, Color4 color)
        {
            TryPush(this.pixelTex, BeginMode.Triangles);
            #region Add vertices

            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);

            vbuffer.AddVertex(x + points.ElementAt(0).X * cos * scale - points.ElementAt(0).Y * sin * scale,
                              y + points.ElementAt(0).X * sin * scale + points.ElementAt(0).Y * cos * scale,
                                color.R, color.G, color.B, color.A, 0, 0);

            for (int i = 1; i < points.Count(); i++)
            {
                vbuffer.AddVertex(x + points.ElementAt(i).X * cos * scale - points.ElementAt(i).Y * sin * scale,
                                  y + points.ElementAt(i).X * sin * scale + points.ElementAt(i).Y * cos * scale,
                                color.R, color.G, color.B, color.A, 0, 0);

                vbuffer.AddIndices(offset + i - 1, offset + i);
            }
            vbuffer.AddIndices(offset + points.Count() - 1, offset);

            #endregion Add vertices
        }

        public void OutlineRegularPolygon(int vertices, float size, float x, float y, float rotation, Color4 color)
        {
            TryPush(this.pixelTex, BeginMode.Lines);
            if (vertices < 3 || vertices > 360)
                throw new ArgumentException("Vertices must be in range from 3 to 360", "vertices");

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            for (int i = 0; i < vertices - 1; i++)
                vbuffer.AddIndices(offset + i, offset + i + 1);
            vbuffer.AddIndices(offset + vertices - 1, offset);

            float degInRad;

            for (int i = 0; i < vertices; i++)
            {
                degInRad = MathHelper.DegreesToRadians((360 / vertices) * i);

                vbuffer.AddVertex(x + (float)(Math.Cos(degInRad) * size / 2),
                    y + (float)(Math.Sin(degInRad) * size / 2),
                    color.R, color.G, color.B, color.A, 0, 0);
            }
        }

        public void OutlineRegularPolygon(int vertices, float size, Vector2 position, float rotation, Color4 color)
        {
            OutlineRegularPolygon(vertices, size, position.X, position.Y, rotation, color);
        }

        #endregion OutlinePolygon

        #region FillRectangle

        public void FillRectangle(float x, float y, float width, float height, Color4 color, float rotation = 0.0f, float xOrigin = 0.0f, float yOrigin = 0.0f)
        {
            TryPush(this.pixelTex, BeginMode.Triangles);
            #region Add vertices

            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            float dx = -xOrigin * width;
            float dy = -yOrigin * height;

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            vbuffer.AddIndices(offset, offset + 1, offset + 2, offset, offset + 2, offset + 3);

            vbuffer.AddVertices(4, x + dx * cos - dy * sin, y + dx * sin + dy * cos,
                                color.R, color.G, color.B, color.A, 0, 0,

                                x + (dx + width) * cos - dy * sin, y + (dx + width) * sin + dy * cos,
                                color.R, color.G, color.B, color.A, 0, 0,
                                
                                x + (dx + width) * cos - (dy + height) * sin, y + (dx + width) * sin + (dy + height) * cos,
                                color.R, color.G, color.B, color.A, 0, 0,
                                
                                x + dx * cos - (dy + height) * sin, y + dx * sin + (dy + height) * cos,
                                color.R, color.G, color.B, color.A, 0, 0);

            #endregion Add vertices
        }

        public void FillRectangle(RectangleF rectangle, Color4 color, float rotation, Vector2 origin)
        {
            FillRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, color, rotation, origin.X, origin.Y);
        }

        public void FillRectangle(RectangleF rectangle, Color4 color)
        {
            FillRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, color, 0.0f, 0.0f, 0.0f);
        }

        public void FillRectangle(PointF position, SizeF size, Color4 color, float rotation, Vector2 origin)
        {
            FillRectangle(position.X, position.Y, size.Width, size.Height, color, rotation, origin.X, origin.Y);
        }

        public void FillRectangle(PointF position, SizeF size, Color4 color)
        {
            FillRectangle(position.X, position.Y, size.Width, size.Height, color, 0.0f, 0.0f, 0.0f);
        }

        public void FillRectangle(Vector2 position, SizeF size, Color4 color, float rotation, Vector2 origin)
        {
            FillRectangle(position.X, position.Y, size.Width, size.Height, color, rotation, origin.X, origin.Y);
        }

        public void FillRectangle(Vector2 position, SizeF size, Color4 color)
        {
            FillRectangle(position.X, position.Y, size.Width, size.Height, color, 0.0f, 0.0f, 0.0f);
        }

        #endregion FillRectangle

        #region OutlineRectangle

        public void OutlineRectangle(float x, float y, float width, float height, Color4 color, float thickness, float rotation, float xOrigin, float yOrigin)
        {
            TryPush(this.pixelTex, BeginMode.Lines);

            #region Add vertices

            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            float dx = -xOrigin * width;
            float dy = -yOrigin * height;

            int offset = vbuffer.VertexOffset / vbuffer.Stride;
            vbuffer.AddIndices(offset, offset + 1, offset + 1, offset + 2, offset + 2, offset + 3, offset + 3, offset);
            vbuffer.AddVertices(4, x + dx * cos - dy * sin, y + dx * sin + dy * cos,
                                color.R, color.G, color.B, color.A, 0, 0,
                                
                                x + (dx + width) * cos - dy * sin, y + (dx + width) * sin + dy * cos,
                                color.R, color.G, color.B, color.A, 0, 0,
                                
                                x + (dx + width) * cos - (dy + height) * sin, y + (dx + width) * sin + (dy + height) * cos,
                                color.R, color.G, color.B, color.A, 0, 0,
                                
                                x + dx * cos - (dy + height) * sin, y + dx * sin + (dy + height) * cos,
                                color.R, color.G, color.B, color.A, 0, 0);

            #endregion Add vertices
        }

        public void OutlineRectangle(RectangleF rectangle, Color4 color, float thickness, float rotation, Vector2 origin)
        {
            OutlineRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, color, thickness, rotation, origin.X, origin.Y);
        }

        public void OutlineRectangle(RectangleF rectangle, Color4 color)
        {
            OutlineRectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, color, 1.0f, 0.0f, 0.0f, 0.0f);
        }

        public void OutlineRectangle(PointF position, SizeF size, Color4 color, float thickness, float rotation, Vector2 origin)
        {
            OutlineRectangle(position.X, position.Y, size.Width, size.Height, color, thickness, rotation, origin.X, origin.Y);
        }

        public void OutlineRectangle(PointF position, SizeF size, Color4 color)
        {
            OutlineRectangle(position.X, position.Y, size.Width, size.Height, color, 1.0f, 0.0f, 0.0f, 0.0f);
        }

        #endregion OutlineRectangle

        #region FillCircle

        public void FillEllipse(float x, float y, float xRadius, float yRadius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            TryPush(this.pixelTex, BeginMode.Triangles);

            if (vertices < 3 || vertices > 360)
                throw new ArgumentException("Vertices must be in range from 3 to 360", "vertices");

            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            vbuffer.AddVertex(x, y, centralColor.R, centralColor.G, centralColor.B, centralColor.A, 0, 0);

            float degInRad;

            for (int i = 1; i <= vertices; i++)
            {
                degInRad = MathHelper.DegreesToRadians((360 / vertices) * (i - 1));

                vbuffer.AddVertex(x + (float)(Math.Cos(degInRad) * xRadius),
                    y + (float)(Math.Sin(degInRad) * yRadius),
                    outerColor.R, outerColor.G, outerColor.B, outerColor.A, 0, 0);
            }
            int j = 0;
            for (int i = 2; i < vertices + 1; i++, j += 3)
                vbuffer.AddIndices(offset, offset + i - 1, offset + i);
            vbuffer.AddIndices(offset, offset + vertices, offset + 1);
        }

        public void FillEllipse(Vector2 position, float xRadius, float yRadius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            FillEllipse(position.X, position.Y, xRadius, yRadius, centralColor, outerColor, vertices);
        }

        public void FillEllipse(PointF position, float xRadius, float yRadius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            FillEllipse(position.X, position.Y, xRadius, yRadius, centralColor, outerColor, vertices);
        }

        public void FillEllipse(Vector2 position, float xRadius, float yRadius, Color4 color, int vertices)
        {
            FillEllipse(position.X, position.Y, xRadius, yRadius, color, color, vertices);
        }

        public void FillEllipse(PointF position, float xRadius, float yRadius, Color4 color, int vertices)
        {
            FillEllipse(position.X, position.Y, xRadius, yRadius, color, color, vertices);
        }

        public void FillCircle(float x, float y, float radius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            FillEllipse(x, y, radius, radius, centralColor, outerColor, vertices);
        }

        public void FillCircle(float x, float y, float radius, Color4 color, int vertices)
        {
            FillEllipse(x, y, radius, radius, color, color, vertices);
        }

        public void FillCircle(Vector2 position, float radius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            FillEllipse(position.X, position.Y, radius, radius, centralColor, outerColor, vertices);
        }

        public void FillCircle(PointF position, float radius, Color4 centralColor, Color4 outerColor, int vertices)
        {
            FillEllipse(position.X, position.Y, radius, radius, centralColor, outerColor, vertices);
        }

        public void FillCircle(Vector2 position, float radius, Color4 color, int vertices)
        {
            FillEllipse(position.X, position.Y, radius, radius, color, color, vertices);
        }

        public void FillCircle(PointF position, float radius, Color4 color, int vertices)
        {
            FillEllipse(position.X, position.Y, radius, radius, color, color, vertices);
        }

        #endregion FillCircle

        #region OutlineCircle

        public void OutlineEllipse(float x, float y, float xRadius, float yRadius, Color4 color, float thickness, int vertices)
        {
            
            if (vertices < 3 || vertices > 360)
                throw new ArgumentException("Vertices must be in range from 3 to 360", "vertices");

            TryPush(this.pixelTex, BeginMode.Lines);
            
            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            float degInRad;

            for (int i = 0; i < vertices; i++)
            {
                degInRad = MathHelper.DegreesToRadians((360 / vertices) * i);

                vbuffer.AddVertex(x + (float)(Math.Cos(degInRad) * xRadius),
                    y + (float)(Math.Sin(degInRad) * yRadius),
                    color.R, color.G, color.B, color.A, 0, 0);
            }

            for (int i = 0; i < vertices - 1; i++)
                vbuffer.AddIndices(offset + i, offset + i + 1);
            vbuffer.AddIndices(offset + vertices - 1, offset);
        }

        public void OutlineEllipse(Vector2 position, float xRadius, float yRadius, Color4 color, float thickness, int vertices)
        {
            OutlineEllipse(position.X, position.Y, xRadius, yRadius, color, thickness, vertices);
        }

        public void OutlineEllipse(PointF position, float xRadius, float yRadius, Color4 color, float thickness, int vertices)
        {
            OutlineEllipse(position.X, position.Y, xRadius, yRadius, color, thickness, vertices);
        }

        public void OutlineEllipse(Vector2 position, float xRadius, float yRadius, Color4 color, int vertices)
        {
            OutlineEllipse(position.X, position.Y, xRadius, yRadius, color, 1, vertices);
        }

        public void OutlineEllipse(PointF position, float xRadius, float yRadius, Color4 color, int vertices)
        {
            OutlineEllipse(position.X, position.Y, xRadius, yRadius, color, 1, vertices);
        }

        public void OutlineCircle(float x, float y, float radius, Color4 color, float thickness, int vertices)
        {
            OutlineEllipse(x, y, radius, radius, color, thickness, vertices);
        }

        public void OutlineCircle(float x, float y, float radius, Color4 color, int vertices)
        {
            OutlineEllipse(x, y, radius, radius, color, 1, vertices);
        }

        public void OutlineCircle(Vector2 position, float radius, Color4 color, float thickness, int vertices)
        {
            OutlineEllipse(position.X, position.Y, radius, radius, color, thickness, vertices);
        }

        public void OutlineCircle(PointF position, float radius, Color4 centralColor, float thickness, int vertices)
        {
            OutlineEllipse(position.X, position.Y, radius, radius, centralColor, thickness, vertices);
        }

        public void OutlineCircle(Vector2 position, float radius, Color4 color, int vertices)
        {
            OutlineEllipse(position.X, position.Y, radius, radius, color, 1, vertices);
        }

        public void OutlineCircle(PointF position, float radius, Color4 color, int vertices)
        {
            OutlineEllipse(position.X, position.Y, radius, radius, color, 1, vertices);
        }

        #endregion OutlineCircle

        #region PrintText

        public void PrintText(BitmapFont font, string text,
            float x, float y,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.0f, float yOrigin = 0.0f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            if (font == null)
                throw new ArgumentException("font");
            SizeF size = font.Measure(text);
            Matrix4 trans = Matrix4.Identity *
                Matrix4.CreateRotationZ(rotation) *
                Matrix4.Scale(scale) *
                Matrix4.CreateTranslation(x, y, 0.0f);

            float xOffset = 0f;
            float yOffset = 0f;

            text = text.Replace("\r\n", "\r");

            xOffset -= (int)(xOrigin * font.MeasureNextlineLength(text));
            yOffset -= (int)(yOrigin * size.Height);

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //newline
                if (c == '\r' || c == '\n')
                {
                    yOffset += font.LineSpacing;
                    xOffset = 0f;
                    xOffset -= (int)(xOrigin * font.MeasureNextlineLength(text.Substring(i + 1)));
                }
                else
                {
                    //normal character
                    if (c != ' ' && font.fontData.CharSetMapping.ContainsKey(c))
                        RenderGlyph(font, c, x, y, xOffset, yOffset, color,rotation, scale, flipHorizontally, flipVertically);

                    if (font.IsMonospacingActive)
                        xOffset += font.MonoSpaceWidth;
                    else
                    {
                        if (c == ' ')
                            xOffset += (float)Math.Ceiling(font.fontData.meanGlyphWidth * font.Options.WordSpacing);
                        //normal character
                        else if (font.fontData.CharSetMapping.ContainsKey(c))
                        {
                            FontGlyph glyph = font.fontData.CharSetMapping[c];
                            xOffset += (float)Math.Ceiling(glyph.rect.Width + font.fontData.meanGlyphWidth * font.Options.CharacterSpacing + font.fontData.GetKerningPairCorrection(i, text, null));
                        }
                    }
                }
            }
        }

        public void PrintText(BitmapFont font, string text,
            Vector2 position,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            PrintText(font, text, position.X, position.Y, color, rotation, scale, xOrigin, yOrigin, flipHorizontally, flipVertically);
        }

        public void PrintText(BitmapFont font, string text,
            PointF position,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            PrintText(font, text, position.X, position.Y, color, rotation, scale, xOrigin, yOrigin, flipHorizontally, flipVertically);
        }

        //This overload allows justification, but it is quite slow.. maybe) Recomended for use only in case, when justification is necessary
        public void PrintText(BitmapFont font, string text,
            float x, float y, float width, bool justify,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            if (font == null)
                throw new ArgumentException("font");

            float maxMeasuredWidth = 0f;
            ProcessedText processedText = font.ProcessText(text, width, justify);
            float maxWidth = processedText.maxWidth;

            float xOffset = 0f;
            float yOffset = 0f;
            SizeF size = font.Measure(processedText);

            var nodeList = processedText.textNodeList;
            for (TextNode node = nodeList.Head; node != null; node = node.Next)
                node.LengthTweak = 0f;  //reset tweaks

            yOffset -= yOrigin * size.Height;
            if (justify)
            {
                font.JustifyLine(nodeList.Head, maxWidth);
                xOffset -= (int)(xOrigin * maxWidth);
            }
            else
                xOffset -= (float)Math.Ceiling(xOrigin * font.TextNodeLineLength(nodeList.Head, maxWidth));

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0f;
            for (TextNode node = nodeList.Head; node != null; node = node.Next)
            {
                bool newLine = false;

                if (node.Type == TextNodeType.LineBreak)
                {
                    newLine = true;
                }
                else
                {
                    if (font.SkipTrailingSpace(node, length, maxWidth) && atLeastOneNodeCosumedOnLine)
                    {
                        newLine = true;
                    }
                    else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine)
                    {
                        atLeastOneNodeCosumedOnLine = true;
                        RenderWord(font, node, x, y, xOffset + length, yOffset, color, rotation, scale, flipHorizontally, flipVertically);
                        length += node.ModifiedLength;

                        maxMeasuredWidth = Math.Max(length, maxMeasuredWidth);
                    }
                    else
                    {
                        newLine = true;
                        if (node.Previous != null)
                            node = node.Previous;
                    }
                }

                if (newLine)
                {
                    yOffset += font.LineSpacing;
                    xOffset = 0f;
                    length = 0f;
                    atLeastOneNodeCosumedOnLine = false;

                    if (node.Next != null)
                    {
                        if (justify)
                        {
                            font.JustifyLine(node.Next, maxWidth);
                            xOffset -= (int)(xOrigin * maxWidth);
                        }
                        else
                            xOffset -= (float)Math.Ceiling(xOrigin * font.TextNodeLineLength(node.Next, maxWidth));
                    }
                }
            }
        }

        private void RenderWord(BitmapFont font, TextNode node, float x, float y, float xOffset, float yOffset, Color4 color, float rotation, float scale, bool flipHorizontally, bool flipVertically)
        {
            if (node.Type != TextNodeType.Word)
                return;

            int charGaps = node.Text.Length - 1;
            bool isCrumbleWord = font.CrumbledWord(node);
            if (isCrumbleWord)
                charGaps++;

            int pixelsPerGap = 0;
            int leftOverPixels = 0;

            if (charGaps != 0)
            {
                pixelsPerGap = (int)node.LengthTweak / charGaps;
                leftOverPixels = (int)node.LengthTweak - pixelsPerGap * charGaps;
            }

            for (int i = 0; i < node.Text.Length; i++)
            {
                char c = node.Text[i];
                if (font.fontData.CharSetMapping.ContainsKey(c))
                {
                    var glyph = font.fontData.CharSetMapping[c];

                    RenderGlyph(font, c, x, y, xOffset, yOffset, color, rotation, scale, flipHorizontally, flipVertically);

                    if (font.IsMonospacingActive)
                        xOffset += font.MonoSpaceWidth;
                    else
                        xOffset += (int)Math.Ceiling(glyph.rect.Width + font.fontData.meanGlyphWidth * font.Options.CharacterSpacing + font.fontData.GetKerningPairCorrection(i, node.Text, node));

                    xOffset += pixelsPerGap;
                    if (leftOverPixels > 0)
                    {
                        xOffset += 1.0f;
                        leftOverPixels--;
                    }
                    else if (leftOverPixels < 0)
                    {
                        xOffset -= 1.0f;
                        leftOverPixels++;
                    }
                }
            }
        }

        public unsafe void PrintSymbol(BitmapFont font, char symbol,
            float x, float y,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            FontGlyph glyph = font.fontData.CharSetMapping[symbol];
            TexturePage sheet = font.fontData.Pages[glyph.page];
            TryPush(sheet.GLTexID, BeginMode.Triangles);
            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            float tx1 = (float)(glyph.rect.X) / sheet.Width;
            float ty1 = (float)(glyph.rect.Y) / sheet.Height;
            float tx2 = (float)(glyph.rect.X + glyph.rect.Width) / sheet.Width;
            float ty2 = (float)(glyph.rect.Y + glyph.rect.Height) / sheet.Height;

            if (flipVertically)
            {
                float temp = ty2;
                ty2 = ty1;
                ty1 = temp;
            }
            if (flipHorizontally)
            {
                float temp = tx2;
                tx2 = tx1;
                tx1 = temp;
            }
            
            float dx = -(xOrigin * (glyph.rect.Width + (font.IsMonospacingActive ? ((font.MonoSpaceWidth - glyph.rect.Width) / 2) : 0))) * scale;
            float dy = -yOrigin * glyph.rect.Height * scale;
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            float width = glyph.rect.Width * scale;
            float height = glyph.rect.Height * scale;

                        int* ind = vbuffer.GetIndexPointerToFill(6);
            float* vert = vbuffer.GetVertexPointerToFill(4);
            
            *(ind++) = offset;
            *(ind++) = offset + 1;
            *(ind++) = offset + 2;
            *(ind++) = offset;
            *(ind++) = offset + 2;
            *(ind++) = offset + 3;            
            
            *(vert++) = x + dx * cos - dy * sin;
            *(vert++) = y + dx * sin + dy * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx1; *(vert++) = ty1;
                     	
            *(vert++) = x + (dx + width) * cos - dy * sin;
            *(vert++) = y + (dx + width) * sin + dy * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx2; *(vert++) = ty1;
                       
            *(vert++) =x + (dx + width) * cos - (dy + height) * sin;
            *(vert++) = y + (dx + width) * sin + (dy + height) * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx2; *(vert++) = ty2;
                       	
            *(vert++) =x + dx * cos - (dy + height) * sin;
            *(vert++) = y + dx * sin + (dy + height) * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx1; *(vert++) = ty2;
        }
        public void PrintSymbol(BitmapFont font, char symbol,
            Vector2 position,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            PrintSymbol(font, symbol, position.X, position.Y, color, rotation, scale, xOrigin, yOrigin, flipHorizontally, flipVertically);
        }

        public void PrintSymbol(BitmapFont font, char symbol,
            PointF position,
            Color4 color,
            float rotation = 0.0f, float scale = 1.0f,
            float xOrigin = 0.5f, float yOrigin = 0.5f,
            bool flipHorizontally = false, bool flipVertically = false)
        {
            PrintSymbol(font, symbol, position.X, position.Y, color, rotation, scale, xOrigin, yOrigin, flipHorizontally, flipVertically);
        }

        private unsafe void RenderGlyph(BitmapFont font, char c, float x, float y, float xOffset, float yOffset, Color4 color, float rotation, float scale, bool flipHorizontally, bool flipVertically)
        {
            FontGlyph glyph = font.fontData.CharSetMapping[c];
            TexturePage sheet = font.fontData.Pages[glyph.page];

            TryPush(sheet.GLTexID, BeginMode.Triangles);
            int offset = vbuffer.VertexOffset / vbuffer.Stride;

            float tx1 = (float)(glyph.rect.X) / sheet.Width;
            float ty1 = (float)(glyph.rect.Y) / sheet.Height;
            float tx2 = (float)(glyph.rect.X + glyph.rect.Width) / sheet.Width;
            float ty2 = (float)(glyph.rect.Y + glyph.rect.Height) / sheet.Height;

            float dx = xOffset * scale;
            float dy = (yOffset + glyph.yOffset) * scale;
            float sin = (float)Math.Sin(rotation);
            float cos = (float)Math.Cos(rotation);
            float width = glyph.rect.Width *scale;
            float height = glyph.rect.Height *scale;
            
            int* ind = vbuffer.GetIndexPointerToFill(6);
            float* vert = vbuffer.GetVertexPointerToFill(4);
            
            *(ind++) = offset;
            *(ind++) = offset + 1;
            *(ind++) = offset + 2;
            *(ind++) = offset;
            *(ind++) = offset + 2;
            *(ind++) = offset + 3;            
            
            *(vert++) = x + dx * cos - dy * sin; *(vert++) = y + dx * sin + dy * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx1; *(vert++) = ty1;
            
            *(vert++) = x + (dx + width) * cos - dy * sin;
            *(vert++) = y + (dx + width) * sin + dy * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx2;*(vert++) = ty1;
            
            *(vert++) =x + (dx + width) * cos - (dy + height) * sin;
            *(vert++) = y + (dx + width) * sin + (dy + height) * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx2; *(vert++) = ty2;
            
            *(vert++) =x + dx * cos - (dy + height) * sin;
            *(vert++) = y + dx * sin + (dy + height) * cos;
            *(vert++) = color.R; *(vert++) = color.G; *(vert++) = color.B; *(vert++) = color.A;
            *(vert++) = tx1; *(vert++) = ty2;
        }
        
        #endregion PrintText
		
		string IDiagnosable.DebugName {
			get {
				return "SpriteBatch";
			}
		}
		
		string IDiagnosable.DebugInfo(int i)
		{
			int r = this.GetProcessedElementsCount();
			switch (i)
			{
				case 0: return "Elements: " + r;
				default: return ";";
			}
		}
    }
}