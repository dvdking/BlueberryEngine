using System;
using Blueberry.Geometry;
using OpenTK;
using System.Drawing;


namespace Blueberry.GameObjects.Components
{
	public class CircleCollider:Component, IQuadTreeItem, IUpdatable, IDisposable
    {
		public Vector2 Offset;
		public float Radius;

		private CircleQuadTreeCollider _circle;
		private Vector2 _oldPosition;

		bool _hasCollisionListeners = false;

        public CircleCollider()
        {
			_circle = new CircleQuadTreeCollider();
        }

		public override void Init()
		{
			foreach (var item in MessagesMethods)
			{
				if (item.Name == "OnTriggerEnter")
				{
					_hasCollisionListeners = true;
					break;
				}
			}
		}

		private void UpdateCilliderData()
		{
			if (_oldPosition != Owner.Transform.Position)
			{
				_circle = new CircleQuadTreeCollider();
				_circle.Circle.Position = Owner.Transform.Position + Offset;
				_circle.Circle.Radius = Radius;
				_oldPosition = Owner.Transform.Position;
				OnPositionChange(this);
			}
		}

		#region IUpdatable implementation
		public void Update(float dt)
		{
			UpdateCilliderData();
			if (_hasCollisionListeners)
			{
				var collisions = Owner.GameObjectManager.QuadTree.Query(_circle.Circle);
				if (collisions.Count != 0)
				{
					Owner.SendMessage("OnTriggerEnter", this, collisions);
				}
			}
		}
		#endregion

		#region IQuadTreeItem implementation
		public event PositionChangeHandler OnPositionChange;
		public event RemoveFromSceneHandler OnRemoveFromScene;
		public IQuadTreeCollider Collider
		{
			get
			{
				UpdateCilliderData();// in case something changed position during collision checking
				return _circle;
			}
		}
		#endregion

		#region IDisposable implementation
		public void Dispose()
		{
			OnRemoveFromScene(this);
		}
		#endregion
    }

	public class CircleQuadTreeCollider:IQuadTreeCollider
	{
		public Circle Circle;

		public Rectangle Bounds{get{ return Circle.Rectangle;}}

		public bool Collides(IQuadTreeCollider collider)
		{
			if (collider is CircleQuadTreeCollider)
			{
				var circle = collider as CircleQuadTreeCollider;

				var otherCircle = circle.Circle;
				return otherCircle.IntersectsWith(this.Circle);
			}
			return false;
		}
		public bool Collides(Rectangle collider)
		{
			return Circle.IntersectsWith(collider);
		}

		public bool Collides(Circle collider)
		{
			return Circle.IntersectsWith(collider);
		}

		public bool Contains(IQuadTreeCollider collider)
		{
			if (collider is CircleQuadTreeCollider)
			{
				var circle = collider as CircleQuadTreeCollider;

				var otherCircle = circle.Circle;
				return otherCircle.Contains(this.Circle);
			}
			return false;
		}	
		public bool Contains(Point collider)
		{
			return Circle.Contains(collider);
		}

		public bool Contains(Vector2 collider)
		{
			return Circle.Contains(collider);
		}
	}
}


