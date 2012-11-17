using System;
using OpenTK;

namespace Blueberry.Particles.Shapes
{
	/// <summary>
	/// Description of LineShape.
	/// </summary>
	public class LineShape:IEmitterShape
	{
		private float _angle;
		public float Angle {get{return _angle;} set{_angle = value; _direction = MathUtils.RotateVector2(Vector2.UnitX, _angle);}}
		private float length;
		private float halfLength;
		public float Lenght{get{return length;}set{length = value; halfLength = value/2;}}
		public Vector2 Direction{get;set;}
		public bool BothWays{get;set;}
		private Vector2 _direction;
		
		private LineShape(float length, Vector2 direction, bool bothWays = false)
		{
			this.length = length;
			this.halfLength = length/2;
			this.Direction = direction;
			this.BothWays = bothWays;
		}
		
		public LineShape(float angle, float length, Vector2 direction, bool bothWays = false):this(length, direction, bothWays)
		{
			this.Angle = angle;
		}
		public LineShape(Vector2 lineDirection, float length, Vector2 direction, bool bothWays = false):this(length, direction, bothWays)
		{
			_angle = (float)Math.Atan2(lineDirection.Y, lineDirection.X);
			_direction = lineDirection;
		}
		public void GetOffsetAndDirection(out Vector2 offset, out Vector2 direction)
		{
			float displacement = RandomTool.NextSingle(-halfLength, halfLength);
			offset = _direction * displacement;
			
			if(Direction == Vector2.Zero)
				direction = RandomTool.NextUnitVector2();
			else
				direction = BothWays && RandomTool.NextBool() ? -Direction : Direction;
		}
	}
}
