/*
 * Created by SharpDevelop.
 * User: Denis
 * Date: 09.11.2012
 * Time: 18:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of ParticleStateManager.
	/// </summary>
	public class ParticleStateManager<T> where T:struct
	{
		protected T[] _states;
		
		protected ParticleStateManager(int capacity)
		{
			_states = new T[capacity];
		}
	}
}
