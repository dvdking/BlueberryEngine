using System;
using System.Drawing;
using OpenTK;
using Blueberry.Geometry;

namespace Blueberry
{
	public interface IQuadTreeCollider
    {
		Rectangle Bounds { get;}

		bool Collides(IQuadTreeCollider collider);

		bool Collides(Rectangle collider);

		bool Collides(Circle collider);

		bool Contains(IQuadTreeCollider collider);
		bool Contains(Point collider);
		bool Contains(Vector2 collider);
    }
}

