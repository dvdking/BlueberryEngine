
using Blueberry.Input;
using OpenTK.Input;
using System;
using System.Xml;
using Blueberry.GameObjects;
namespace Blueberry
{
	/// <summary>
	/// Description of GameFrame.
	/// </summary>
	public class GameFrame
	{
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
