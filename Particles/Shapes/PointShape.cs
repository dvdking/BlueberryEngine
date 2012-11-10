using System;
using OpenTK;

namespace Blueberry.Particles.Shapes
{
	/// <summary>
	/// Description of PointShape.
	/// </summary>
	public class PointShape : IEmitterShape
	{
		public PointShape()
		{
		}
		
		public void GetOffsetAndDirection(out OpenTK.Vector2 offset, out OpenTK.Vector2 direction)
		{
			offset = Vector2.Zero;
			direction = RandomTool.NextUnitVector2();
		}
	}
}
