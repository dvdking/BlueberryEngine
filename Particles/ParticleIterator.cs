using System;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of ParticleIterator.
	/// </summary>
	public unsafe struct ParticleIterator
	{
		private readonly MetaParticle* _buffer;
        private readonly Int32 _bufferSize;
        private readonly Int32 _startIndex;
        private readonly Int32 _count;
        private Int32 _currentIteration;

        public readonly MetaParticle* First;

        public ParticleIterator(MetaParticle* buffer, int bufferSize, int startIndex, int count)
        {
            _buffer     = buffer;
            _bufferSize       = bufferSize;
            _startIndex = startIndex;
            _count      = count;

            _currentIteration = 0;

            First = buffer + _startIndex;
        }

        public bool MoveNext(MetaParticle** particle)
        {
            if (++_currentIteration > (_count - 1))
                return false;

            (*particle) = _buffer + ((_startIndex + _currentIteration) % _bufferSize);

            return true;
        }

        public void Reset()
        {
            _currentIteration = 0;
        }
	}
}
