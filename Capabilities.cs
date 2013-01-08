
using System;
using OpenTK.Graphics.OpenGL;

namespace Blueberry
{
	public enum GLExtensionSupport
    {
        Core,
        Extension,
        None
    }
    public static class Capabilities
	{
        public static GLExtensionSupport Framebuffers { get; private set; }
        public static float OGLVersion { get; private set; }
        public static int OGLVersionMajor { get; private set; }
        public static int OGLVersionMinor { get; private set; }


        public static float GLSLVersion { get; private set; }
        public static int GLSLVersionMajor { get; private set; }
        public static int GLSLVersionMinor { get; private set; }

        public static void Test()
		{
            #region opengl version
			string tempStr = GL.GetString(StringName.Version);
            tempStr = tempStr.Substring(0, 3);
            float fver;
            if (!float.TryParse(tempStr, out fver))
            {
                tempStr = tempStr.Replace('.', ',');
                if (!float.TryParse(tempStr, out fver))
                    throw new Exception("incorrect opengl version");
            }
            OGLVersion = fver; 
            OGLVersionMajor = int.Parse(tempStr[0].ToString());
            OGLVersionMinor = int.Parse(tempStr[2].ToString());
            #endregion

            #region framebuffers
            if(OGLVersion >= 3f)
                Framebuffers = GLExtensionSupport.Core;
            else if(GL.GetString(StringName.Extensions).Contains("GL_EXT_framebuffer_object"))
                Framebuffers = GLExtensionSupport.Extension;
            else
                Framebuffers = GLExtensionSupport.None;
            #endregion
		}
	}
}
