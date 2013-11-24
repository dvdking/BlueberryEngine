using System;
using Blueberry.GameObjects;
using OpenTK.Graphics.OpenGL;
using Blueberry;
using OpenTK.Input;
using System.Diagnostics;

namespace Example_Project
{
	public class PlayerControl:Component, IUpdatable
    {
		public float Speed;
		
		#region IUpdatable implementation
		public void Update(float dt)
		{
			if (InputMgr.Keyboard[Key.D])
			{
				Owner.Transform.X += 100 * dt;
			}
			else if (InputMgr.Keyboard[Key.A])
			{
				Owner.Transform.X -= 100 * dt;
			}
		}
		#endregion
    }
}

