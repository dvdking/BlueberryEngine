using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Blueberry.Graphics
{
    public class GraphicsDevice
    {
        private static GraphicsDevice _instance;

        public static GraphicsDevice Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new GraphicsDevice();
                return _instance;
            }
        }

        uint framebuffer;

        public GraphicsDevice()
        {
        }

        public void Initialize(int width, int height)
        {
            GL.ClearColor(Color.CornflowerBlue);
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            /* Create the offscreen framebuffer*/
            //GL.GenFramebuffers(1, out framebuffer);
            //GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void SetRenderTarget(Texture target)
        {
            if (target == null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
    }
}