using System;

namespace Blueberry
{
    public class Timer
    {
		public bool IsRunning { get { return _running; } }

		public EventHandler OnTick;

		public float Duration;

		public bool Repeat;

		private bool _running;
		private float _elapsed;

		public void Run()
		{
			_running = true;
		}

		public void Update(float dt)
		{
			if (!_running)
			{
				return;
			}

			_elapsed += dt;
			if (_elapsed >= Duration)
			{
				_elapsed = 0;
				_running = Repeat;
				if (OnTick != null)
				{
					OnTick(this, EventArgs.Empty);
				}
			}
		}
    }
}

