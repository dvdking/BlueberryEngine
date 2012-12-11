
using System;
using Blueberry.Audio;
using Blueberry.Graphics;
using Blueberry.Input;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace Blueberry
{
	/// <summary>
	/// Description of BlueberryGame.
	/// </summary>
	public class BlueberryGame:IDisposable
	{
		private static BlueberryGame _current;
		internal static BlueberryGame CurrentGame{get {return _current;}}
		
		private GameWindow _window;
		protected KeyboardDevice Keyboard{get{return _window.Keyboard;}}
		protected MouseDevice Mouse {get{return _window.Mouse;}}
		
		protected GamepadDevice[] Gamepads{get{return _gamepads;}}
		protected GamepadDevice Gamepad{get{return _gamepads[0];}}
		
		GamepadDevice[] _gamepads;
		GamepadDevice _gamepad;
		
		public bool CursorVisible
		{
			get {return _window.CursorVisible; }
		}
		public VSyncMode VSync
		{
			get {return _window.VSync;}
			set {_window.VSync = value;}
		}
		public GameWindow Window
		{
			get{ return _window; }
		}
		
		internal Capabilities capabilities;
		int _framebuffer;
		
		public BlueberryGame(int width, int height, string name, bool fullscreen, double contextVersion)
		{
			_current = this;
			int major = (int)contextVersion;
			int minor = (int)((contextVersion - major)*10);
			_window = new GameWindow(width, height, 
			                         GraphicsMode.Default, name, 
			                         fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
			                         
			                         DisplayDevice.Default,major, minor, 
#if DEBUG
			                         GraphicsContextFlags.Default);
#else
									 GraphicsContextFlags.ForwardCompatible);
#endif
			_gamepad = new GamepadDevice(UserIndex.Any);
			_gamepads = new GamepadDevice[4];
			for (int i = 0; i < 4; i++) 
				_gamepads[i] = new GamepadDevice((UserIndex)i);
			
			_window.UpdateFrame += (a, b)=>Update((float)b.Time);
			_window.RenderFrame += (a, b)=>Render((float)b.Time);
			_window.Load += (a, b)=>Load();
			
			capabilities.Test();
			if (capabilities.Framebuffers == GLExtensionSupport.Core)
            {
                GL.GenFramebuffers(1, out _framebuffer);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            } else if (capabilities.Framebuffers == GLExtensionSupport.Extension)
            {
                GL.Ext.GenFramebuffers(1, out _framebuffer);
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
			GL.ClearColor(Color4.CornflowerBlue);
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public void SetRenderTarget(Texture target)
        {
            if (target == null)
            {
                if(capabilities.Framebuffers == GLExtensionSupport.Core)
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                else
                    GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                if(capabilities.Framebuffers == GLExtensionSupport.Core)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
                    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                }
                else
                {
                    GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, _framebuffer);
                    GL.Ext.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, target.ID, 0);
                }
                GL.Clear(ClearBufferMask.ColorBufferBit);
            }
        }
        protected virtual void Load()
        {
        	
        }
		protected virtual void Update(float dt)
		{
			
		}
		protected virtual void Render(float dt)
		{
			
			_window.SwapBuffers();
		}

		public void Run()
		{
			_window.Run();
		}
		public void Run(double updateRate)
		{
			_window.Run(updateRate);
		}
		public void Run(double ups, double fps)
		{
			_window.Run(ups, fps);
		}
		public void Exit()
		{
			Dispose();
			_window.Exit();
		}
		public void Dispose()
		{
			if(SpriteBatch.HasInstance)
				SpriteBatch.Instance.Dispose();
			if(AudioManager.HasInstance)
				AudioManager.Instance.Dispose();
			if (_framebuffer != -1)
            {
                if(capabilities.Framebuffers == GLExtensionSupport.Core)
                    GL.DeleteFramebuffers(1, ref _framebuffer);
                else
                    GL.Ext.DeleteFramebuffers(1, ref _framebuffer);
            }
		}
	}
}
