/*
 * Created by SharpDevelop.
 * User: Denis
 * Date: 09.11.2012
 * Time: 13:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Blueberry.Particles
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
