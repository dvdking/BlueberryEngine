using System;
using Blueberry.GameObjects;
using OpenTK;
using System.Diagnostics;
using Blueberry;

namespace Example_Project
{
	public class BulletControl:Component, IUpdatable
    {
		public float Speed;
		public Side Side;
		#region IUpdatable implementation

		public void Update(float dt)
		{
			Owner.Transform.Position += Transform.RotationVector * Speed * GS.Delta;
		}

		#endregion


    }
}

