using System;
using System.Drawing;
using OpenTK;

namespace Blueberry.Particles.Shapes
{
	/// <summary>
	/// Description of RectangleShape.
	/// </summary>
	public class RectangleShape:IEmitterShape
	{
		public Size Rectangle{get; set;}
		private float _angle;
		public float Angle {get{return _angle;} set{_angle = value; _direction = MathUtils.RotateVector2(Vector2.UnitX, _angle);}}
		public Vector2 Direction{get;set;}
		public bool BothWays{get;set;}
		private Vector2 _direction;
		
		public RectangleShape(Size rect, Vector2 direction, bool bothWays)
		{
			this.Rectangle = rect;
			this.Direction = direction;
			this.BothWays = bothWays;
		}
		
		public void GetOffsetAndDirection(out Vector2 offset, out Vector2 direction)
		{
			offset.X = RandomTool.NextSingle(-Rectangle.Width/2, Rectangle.Width/2);
			offset.Y = RandomTool.NextSingle(-Rectangle.Height/2, Rectangle.Height/2);
			
			if(Direction == Vector2.Zero)
				direction = RandomTool.NextUnitVector2();
			else
				direction = BothWays && RandomTool.NextBool() ? -Direction : Direction;
		}
	}
}
