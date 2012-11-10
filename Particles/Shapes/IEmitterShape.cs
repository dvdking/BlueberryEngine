using System;
using OpenTK;

namespace Blueberry.Particles.Shapes
{
	/// <summary>
	/// Description of IEmitterShape.
	/// </summary>
	public interface IEmitterShape
	{
		void GetOffsetAndDirection(out Vector2 offset, out Vector2 direction);
	}
}
