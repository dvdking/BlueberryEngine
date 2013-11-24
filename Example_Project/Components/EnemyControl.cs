using System;
using System.Collections;
using Blueberry.GameObjects;
using Blueberry.GameObjects.Components;
using Blueberry;
using OpenTK;
using System.Net.Mime;
using OpenTK.Input;
using System.Runtime.Remoting.Metadata.W3cXsd2001;


namespace Example_Project
{
	public class EnemyControl:Component, IUpdatable
    {
		public float Speed;
		public float BottomPos;
		public float UpperPos;

		private Timer _changeDirectionTimer;

		private Timer _shootingTimer;

		private int dir;

		public override void Init()
		{
			base.Init();
			_changeDirectionTimer = new Timer();
			_changeDirectionTimer.Duration = 3;
			_changeDirectionTimer.Run();
			_changeDirectionTimer.OnTick += (s, a) => dir *= -1;
			_changeDirectionTimer.Repeat = true;

			_shootingTimer = new Timer();
			_shootingTimer.Duration = 1.5f;
			_shootingTimer.Repeat = true;
			_shootingTimer.OnTick += (s, a) =>
			{
				var b = CreateInstance("Bullet", Transform.Position);
				var bc = b.GetComponent<BulletControl>();
				bc.Side = Side.Enemy;
				bc.Transform.RotationVector = new Vector2(-1, 0);
			};
			_shootingTimer.Run();

			dir = 1;
		}

		#region IUpdatable implementation
		public void Update(float dt)
		{
			if(dir == 1)
			{
				Transform.Y = MathUtils.Lerp(Transform.Y, BottomPos, Speed*GS.Delta);
			}
			else
			{
				Transform.Y = MathUtils.Lerp(Transform.Y, UpperPos, Speed*GS.Delta);
			}
			_shootingTimer.Update(GS.Delta);
			_changeDirectionTimer.Update(GS.Delta);
		}
		#endregion

		void OnTriggerEnter(GameObject collider)
		{
			var bc = collider.GetComponent<BulletControl>();
			if (bc != null)
			{
				if (bc.Side == Side.Player)
				{
					Destroy(Owner);
					Destroy(collider);
				}
			} 
		}
    }
}

