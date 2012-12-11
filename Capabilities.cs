
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
	public struct Capabilities
	{
		public GLExtensionSupport Framebuffers;
		public float OGLVersion;
		public int OGLVersionMajor;
		public int OGLVersionMinor;
		public float GLSLVersion;
		public int GLSLVersionMajor;
		public int GLSLVersionMinor;
		
		public void Test()
		{
			string tempStr;
			#region opengl version
			tempStr = GL.GetString(StringName.Version);
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
