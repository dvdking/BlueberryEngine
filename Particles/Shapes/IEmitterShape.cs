/*
 * Created by SharpDevelop.
 * User: Denis
 * Date: 09.11.2012
 * Time: 13:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of IEmitterShape.
	/// </summary>
	public interface IEmitterShape
	{
		void GetOffsetAndDirection(out Vector2 offset, out Vector2 direction);
	}
}
