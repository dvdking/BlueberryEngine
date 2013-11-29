using System;
using OpenTK;
using System.Collections.Generic;

namespace Blueberry.Graphics
{
	public class Material
	{
        public string Name { get; internal set; }

		internal Shader Shader{get; private set;}

		Dictionary<string, object> _values = new Dictionary<string, object>();


		internal Material (Shader shader)
		{
			Shader = shader;
		}
		
		public void SetParameter(string name, object value)
		{
			_values [name] = value;
		}

		internal void SetShaderUniforms()
		{
			foreach (var item in _values) 
			{
				//todo probably there is a better solution
                object v = item.Value;
				if (v.GetType() == typeof(int)) 
				{
					Shader.SetUniform (item.Key, (int)item.Value);
					continue;
				}
				if (v.GetType() == typeof(float)) 
				{
					Shader.SetUniform (item.Key, (float)item.Value);
					continue;
				}
				if (v.GetType() == typeof(Vector2)) 
				{
					var v2 = (Vector2)item.Value;
					Shader.SetUniform (item.Key,ref v2);
					continue;
				}
				if (v.GetType() == typeof(Vector3)) 
				{
					var v3 = (Vector3)item.Value;
					Shader.SetUniform (item.Key, ref v3);
					continue;
				}
				if (v.GetType() == typeof(Vector4)) 
				{
					var v4 = (Vector4)item.Value;
					Shader.SetUniform (item.Key, ref v4);
					continue;
				}
				if (v.GetType() == typeof(Matrix4)) 
				{
					Matrix4 m = (Matrix4)item.Value;
					Shader.SetUniform (item.Key, ref m);
					continue;
				}
			}
			_values.Clear ();
		}



		public void SetParameter(string  name, int value)
		{
			Shader.SetUniform (name, value);
		}

		public void SetParameter(string name, float value)
		{
			Shader.SetUniform (name, value);
		}

		public void SetParameter(string name, ref  Vector2 value)
		{
			Shader.SetUniform (name, ref value);
		}

		public void SetParameter(string name, ref  Vector3 value)
		{
			Shader.SetUniform (name, ref value);
		}

		public void SetParameter(string name, ref  Vector4 value)
		{
			Shader.SetUniform (name, ref value);
		}

		public void SetParameter(string name, ref Matrix4 value)
		{
			Shader.SetUniform (name, ref value);
		}


	}
}

