using System;
using Blueberry.Geometry;
using OpenTK;
using System.Drawing;


namespace Blueberry.GameObjects.Components
{
	public class CircleCollider:ColliderComponent
    {
		public float Radius;

		private CircleQuadTreeCollider _circle{get { return QuadTreeCollider as CircleQuadTreeCollider;}}

        public CircleCollider()
        {
			QuadTreeCollider = new CircleQuadTreeCollider();
        }

		protected override void UpdateCollider()
		{
			QuadTreeCollider = new CircleQuadTreeCollider();
			_circle.Circle.Position = Owner.Transform.Position + Offset;
			_circle.Circle.Radius = Radius;
		}
    }

	public class CircleQuadTreeCollider:IQuadTreeCollider
	{
		public Circle Circle;

		public Rectangle Bounds{get{ return Circle.Rectangle;}}
		Circle IQuadTreeCollider.Circle{get{ return Circle;}}
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


