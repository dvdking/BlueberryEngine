using System;
using Blueberry.Particles.Shapes;
using OpenTK;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of ParticleEmitter.
	/// </summary>
	public class ParticleEmitter
	{
		private readonly MetaParticle[] _buffer;
		private readonly int _bufferSize;
		private readonly float _term;
		private readonly IEmitterShape _shape;
		private readonly IParticleStateManager _stateManager;
        private Int32 _head;
        private Int32 _next;
		private Int32 _activeCount;
		
		public int ReleaseQuantity{get; set;}
		public int ActiveParticlesCount
        {
            get { return _activeCount; }
        }
		public float Term{get{return _term;}}
		public ParticleEmitter(int capacity, float term, IEmitterShape shape, IParticleStateManager stateManager)
        {
            _bufferSize = capacity;
            _term = term;

            _buffer = new MetaParticle[_bufferSize];

            _shape = shape;
            _stateManager = stateManager;

            unsafe
            {
                fixed (MetaParticle* particles = _buffer)
                {
                    for (var index = 0; index < _bufferSize; index++)
                    {
                        (particles + index)->Index = index;
                    }
                }
            }
        }
		
		public unsafe void Update(float totalSeconds, float deltaSeconds)
        {
            if (_activeCount == 0)
                return;

            fixed (MetaParticle* buffer = _buffer)
            {
                var expired = 0;
                var iteration = 0;

                do
                {
                    var meta = buffer + ((_head + iteration) % _bufferSize);

                    meta->Age = (totalSeconds - meta->ReleaseInfo.TotalSeconds) / _term;

                    if (meta->Age > 1f)
                        expired++;
                }
                while (++iteration < _activeCount);

                if (expired > 0)
                    Pop(expired);

                var iterator = new ParticleIterator(buffer, _bufferSize, _head, _activeCount);

                _stateManager.Update(ref iterator, deltaSeconds);
            }
        }

        public unsafe void Trigger(float totalSeconds, Vector2 position)
        {
            fixed (MetaParticle* buffer = _buffer)
            {
                var startIndex = _next;
                var released = 0;

                for (var i = 0; i < ReleaseQuantity; i++)
                {
                    var index = Push();

                    if (index < 0)
                        break;
					
                    Vector2 dir;
                    Vector2 off;
                   	_shape.GetOffsetAndDirection(out off, out dir);

                   	var releaseInfo = new ReleaseInformation(totalSeconds, position + off, dir);

                    (buffer + index)->ReleaseInfo = releaseInfo;

                    released++;
                }

                if (released > 0)
                {
                    var iterator = new ParticleIterator(buffer, _bufferSize, startIndex, released);

                    _stateManager.Trigger(ref iterator);
                }
            }
        }

        public unsafe void Render()
        {
            if (_activeCount > 0)
            {
                fixed (MetaParticle* buffer = _buffer)
                {
                    var iterator = new ParticleIterator(buffer, _bufferSize, _head, _activeCount);

                    _stateManager.Render(ref iterator);
                }
            }
        }
         private int Push()
        {
            if (_activeCount == _bufferSize)
                return -1;

            var index = _next;

            _next = (_next + 1) % _bufferSize;

            _activeCount++;

            return index;
        }

        private void Pop(Int32 count)
        {
            _head = (_head + count) % _bufferSize;

            _activeCount -= count;
        }
	}
}
