using System;
using System.Linq;
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


        public void SetParameter(string name, string type, string value)
        {
            switch (type)
            {
                case "float":
                    _values[name] = float.Parse(value);
                    break;
                case "int":
                    _values[name] = float.Parse(value);
                    break;
                case "texture":
                    _values[name] = ResourceMgr.GetTexture(value);
                    break;
                //todo add moar
                default:
                    break;
            }
        }

		public void SetParameter(string name, object value)
		{
			_values [name] = value;
		}

		internal void SetShaderUniforms()
		{
            int textureUnit = 1;

			foreach (var item in _values) 
			{
				//todo probably there is a better solution
                //probably not...
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
                if (v.GetType() == typeof(Texture))
                {
                    Texture t = (Texture)v;
                    t.Bind(textureUnit);
                    Shader.SetUniform(item.Key, textureUnit++);
                }
			}
		}

        public Material Clone()
        {
            var m = new Material(Shader);

            m._values = new Dictionary<string, object>(_values);
            m.Name = Name;

            return m;

        }
	}
}

