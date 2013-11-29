using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Blueberry.Graphics
{
    public class Shader : IDisposable
    {
        private int _program;
        private int _vertex;
        private int _fragment;
        private int _geometry;

        public int Program { get { return _program; } }

		public Dictionary<string, int> _uniformLocations;

        public static float Version 
        {
            get
            {
                string sver = GL.GetString(StringName.ShadingLanguageVersion);
                sver = sver.Substring(0, 4);
                float fver;
                if (!float.TryParse(sver, out fver))
                {
                    sver = sver.Replace('.', ',');
                    if (!float.TryParse(sver, out fver))
                        throw new Exception("incorrect shader version");
                }
                return fver;
            } 
        }

        public static byte MajorVersion
        {
            get
            {
                string sver = GL.GetString(StringName.ShadingLanguageVersion);
                return byte.Parse(sver[0].ToString());
            }
        }

        public static byte MinorVersion
        {
            get
            {
                string sver = GL.GetString(StringName.ShadingLanguageVersion);
                return byte.Parse(sver[2].ToString());
            }
        }

        public Shader()
        {
            _vertex = 0;
            _fragment = 0;
            _geometry = 0;
            _program = GL.CreateProgram();
			_uniformLocations = new Dictionary<string, int> ();
        }

        #region set uniforms

        public void SetUniform(string uniformName, int value)
        {
			int uniformLocation = GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.Uniform1(uniformLocation, value);
        }

        public void SetUniform(string uniformName, float value)
        {
			int uniformLocation =  GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.Uniform1(uniformLocation, value);
        }

        public void SetUniform(string uniformName, ref  Vector2 value)
        {
			int uniformLocation =  GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.Uniform2(uniformLocation, ref value);
        }

        public void SetUniform(string uniformName, ref  Vector3 value)
        {
			int uniformLocation =  GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.Uniform3(uniformLocation, ref value);
        }

        public void SetUniform(string uniformName, ref  Vector4 value)
        {
			int uniformLocation = GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.Uniform4(uniformLocation, ref value);
        }

        public void SetUniform(string uniformName, ref Matrix4 value)
        {
			int uniformLocation = GetUniformLocation (uniformName);
            if (uniformLocation != -1)
                GL.UniformMatrix4(uniformLocation, false, ref value);
        }


        public void SetUniform(int location, int value)
        {
            if (location != -1)
                GL.Uniform1(location, value);
        }

        public void SetUniform(int location, float value)
        {
            if (location != -1)
                GL.Uniform1(location, value);
        }

        public void SetUniform(int location, ref  Vector2 value)
        {
            if (location != -1)
                GL.Uniform2(location, ref value);
        }

        public void SetUniform(int location, ref  Vector3 value)
        {
            if (location != -1)
                GL.Uniform3(location, ref value);
        }

        public void SetUniform(int location, ref  Vector4 value)
        {
            if (location != -1)
                GL.Uniform4(location, ref value);
        }

        public void SetUniform(int location, ref Matrix4 value)
        {
            if (location != -1)
                GL.UniformMatrix4(location, false, ref value);
        }


        #endregion set uniforms

        public bool AttributeExists(string attribName)
        {
            int attribLocation = GL.GetAttribLocation(_program, attribName);
            return attribLocation != -1;
        }

        public void Use()
        {
            GL.UseProgram(_program);
        }

        public void Link()
        {
            GL.AttachShader(_program, _vertex);
            GL.AttachShader(_program, _fragment);
            GL.AttachShader(_program, _geometry);

            GL.LinkProgram(_program);

            int status;

            GL.GetProgram(_program, ProgramParameter.LinkStatus, out status);
            if (status != 1)
            {
                string buffer;
                GL.GetProgramInfoLog(_program, out buffer);
                throw new Exception(buffer);
            }
            GL.UseProgram(_program);

            GL.ValidateProgram(_program);

            GL.GetProgram(_program, ProgramParameter.ValidateStatus, out status);
            if (status != 1)
            {
                string buffer;
                GL.GetProgramInfoLog(_program, out buffer);
                throw new Exception(buffer);
            }
        }

        public int Handle { get { return _program; } }

        public void LoadVertexFile(string filename)
        {
            if (_vertex == 0)

                _vertex = GL.CreateShader(ShaderType.VertexShader);

            loadShader(File.ReadAllText(filename), ref _vertex);
        }

        public void LoadGeometryFile(string filename)
        {
            if (_geometry == 0)
                _geometry = GL.CreateShader(ShaderType.GeometryShader);
            loadShader(File.ReadAllText(filename), ref _geometry);
        }

        public void LoadFragmentFile(string filename)
        {
            if (_fragment == 0)
                _fragment = GL.CreateShader(ShaderType.FragmentShader);
            loadShader(File.ReadAllText(filename), ref _fragment);
        }

        public void LoadVertexSource(string source)
        {
            if (_vertex == 0)
                _vertex = GL.CreateShader(ShaderType.VertexShader);
            loadShader(source, ref _vertex);
        }

        public void LoadGeometrySource(string source)
        {
            if (_geometry == 0)
                _geometry = GL.CreateShader(ShaderType.GeometryShader);
            loadShader(source, ref _geometry);
        }

        public void LoadFragmentSource(string source)
        {
            if (_fragment == 0)
                _fragment = GL.CreateShader(ShaderType.FragmentShader);
            loadShader(source, ref _fragment);
        }

        private void loadShader(string source, ref int shader)
        {
            if (source == null || source.Length == 0)
                throw new FileLoadException("Empty shader file");

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            int status;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
            if (status != 1)
            {
                string buffer;
                GL.GetShaderInfoLog(shader, out buffer);
                throw new Exception(buffer);
            }
        }

		private int GetUniformLocation(string name)
		{
			if (_uniformLocations.ContainsKey (name))
				_uniformLocations [name] = GL.GetUniformLocation (Program, name);
			return _uniformLocations [name];
		}


        public void Dispose()
        {
            GL.DeleteProgram(_program);
            GL.DeleteShader(_vertex);
            GL.DeleteShader(_fragment);
            GL.DeleteShader(_geometry);
            Debug.WriteLine("Shader: {0} removed from memory", _program);
        }
    }
}