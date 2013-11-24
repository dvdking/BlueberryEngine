using System;
using Blueberry.GameObjects;
using OpenTK.Graphics.OpenGL;
using Blueberry;
using OpenTK.Input;
using System.Diagnostics;
using System.Collections.Generic;

namespace Example_Project
{
	public class PlayerControl:Component, IUpdatable
    {
		public float Speed;

		public Timer _shootingTimer;

		public override void Init()
		{
			base.Init();
			_shootingTimer = new Timer();
			_shootingTimer.Duration = 1.0f;
		}

		#region IUpdatable implementation
		public void Update(float dt)
		{
			if (InputMgr.Keyboard[Key.W])
			{
				Transform.Y -= dt * Speed;
			}
			else if (InputMgr.Keyboard[Key.S])
			{
				Transform.Y += dt * Speed;
			}
			if (InputMgr.Keyboard[Key.Space])
			{
				if (!_shootingTimer.IsRunning)
				{
					var bullet = CreateInstance("Bullet");
					var bc = bullet.GetComponent<BulletControl>();
					bc.Side = Side.Player;
					bullet.Transform.Rotation = 0;
					bullet.Transform.Position = Transform.Position;
					_shootingTimer.Run();
				}
			}

			_shootingTimer.Update(dt);
		}
		#endregion

		void OnTriggerEnter(GameObject collider)
		{
			var bc = collider.GetComponent<BulletControl>();
			if (bc != null)
			{
				if (bc.Side == Side.Enemy)
				{
					Destroy(Owner);
					Destroy(collider);
				}
			} 
		}
    }


}

