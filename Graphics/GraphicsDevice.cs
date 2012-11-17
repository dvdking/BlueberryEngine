using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Blueberry.Graphics
{
    public enum FeatureSupport
    {
        Core,
        Extension,
        None
    }
    public class GraphicsDevice : IDisposable
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

        public FeatureSupport FramebufferSupport
        {
            get; private set;
        }

        public float Version
        {
            get
            {
                string sver = GL.GetString(StringName.Version);
                sver = sver.Substring(0, 3);
                float fver;
                if (!float.TryParse(sver, out fver))
                {
                    sver = sver.Replace('.', ',');
                    if (!float.TryParse(sver, out fver))
                        throw new Exception("incorrect opengl version");
                }
                return fver;
            }
        }

        public byte MajorVersion
        {
            get
            {
                string sver = GL.GetString(StringName.Version);
                return byte.Parse(sver[0].ToString());
            }
        }
        public byte MinorVersion
        {
            get
            {
                string sver = GL.GetString(StringName.Version);
                return byte.Parse(sver[2].ToString());
            }
        }

        int framebuffer = -1;

        public GraphicsDevice()
        {
            if(Version >= 3f)
                FramebufferSupport = FeatureSupport.Core;
            else if(GL.GetString(StringName.Extensions).Contains("GL_EXT_framebuffer_object"))
                FramebufferSupport = FeatureSupport.Extension;
            else
                FramebufferSupport = FeatureSupport.None;
        }

        public void Initialize(int width, int height)
        {
            GL.ClearColor(Color.CornflowerBlue);
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.LineSmooth);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            /* Create the offscreen framebuffer*/
            if (FramebufferSupport == FeatureSupport.Core)
            {
                GL.GenFramebuffers(1, out framebuffer);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            } else if (FramebufferSupport == FeatureSupport.Extension)
            {
                GL.Ext.GenFramebuffers(1, out framebuffer);
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
        }

        public void SetRenderTarget(Texture target)
        {
            if (target == null)
            {
                if(FramebufferSupport == FeatureSupport.Core)
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                else
                    GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                if(FramebufferSupport == FeatureSupport.Core)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                }
                else
                {
                    GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer);
                    GL.Ext.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                }
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            if (framebuffer != -1)
            {
                if(FramebufferSupport == FeatureSupport.Core)
                    GL.DeleteFramebuffers(1, ref framebuffer);
                else
                    GL.Ext.DeleteFramebuffers(1, ref framebuffer);
            }
        }

        #endregion
    }
}