
using System;

namespace Blueberry
{
	/// <summary>
	/// Description of GameFrame.
	/// </summary>
	public class GameFrame
	{
		private bool _active;
		public bool Active { get; private set; }
		
		public GameFrame()
		{
			Active = false;
		}
		
		public virtual void Update(float dt)
		{
			
		}
		public virtual void Render(float dt)
		{
			
		}
		public virtual void Load()
		{
			Active = true;
		}
		public virtual void Unload()
		{
			Active = false;
		}
	}
}
