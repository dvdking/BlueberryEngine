using System;

namespace Blueberry.Graphics
{
	public class Material
	{
		private Shader _shader;

		internal Material (Shader shader)
		{
			_shader = shader;
		}


	}
}

