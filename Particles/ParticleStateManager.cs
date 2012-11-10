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
