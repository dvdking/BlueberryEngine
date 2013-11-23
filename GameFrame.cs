﻿
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

        internal KeyboardDevice _keyboard;
        internal MouseDevice _mouse;
        internal GamepadDevice _gamepad;
        internal GamepadDevice[] _gamepads;

        protected KeyboardDevice Keyboard { get { return _keyboard; }}
        protected MouseDevice Mouse { get { return _mouse; } }

        protected GamepadDevice[] Gamepads { get { return _gamepads; } }
        protected GamepadDevice Gamepad { get { return _gamepad; } }

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
