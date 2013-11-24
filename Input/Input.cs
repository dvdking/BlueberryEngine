using System;
using System.Drawing;
using OpenTK.Input;
using System.Security.Cryptography.X509Certificates;
using OpenTK;
using Blueberry.Input;

namespace Blueberry
{
	/// <summary>
	/// Controls all input
	/// </summary>
	public static class InputMgr
    {
		public static KeyboardDevice Keyboard{get{return _window.Keyboard;}}
		public static MouseDevice Mouse {get{return _window.Mouse;}}

		public static GamepadDevice[] Gamepads{get{return _gamepads;}}
		public static GamepadDevice Gamepad{get{return _gamepads[0];}}

		private static GameWindow _window;
		private static GamepadDevice[] _gamepads;

		public static void Init(GameWindow window)
		{
			_window = window;

			_gamepads = new GamepadDevice[4];
			for (int i = 0; i < 4; i++) 
				_gamepads[i] = new GamepadDevice((UserIndex)i);
		}

		public static void Update()
		{
			Gamepad.Update(GS.Delta);
		}
    }
}

