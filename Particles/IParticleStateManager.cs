using System;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of IParticleSystemImplementation.
	/// </summary>
	public interface IParticleStateManager
	{
		unsafe void Trigger(ref ParticleIterator iterator);
	    unsafe void Update(ref ParticleIterator iterator, float deltaSeconds);
        unsafe void Render(ref ParticleIterator iterator);
	}
}
