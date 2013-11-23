
using System;
using System.Collections.Generic;
using Blueberry.Audio;
using Blueberry.Diagnostics;
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
		public static BlueberryGame CurrentGame{get {return _current;}}
		
		private GameWindow _window;
		protected KeyboardDevice Keyboard{get{return _window.Keyboard;}}
		protected MouseDevice Mouse {get{return _window.Mouse;}}
		
		protected GamepadDevice[] Gamepads{get{return _gamepads;}}
		protected GamepadDevice Gamepad{get{return _gamepads[0];}}
		
		GamepadDevice[] _gamepads;
		
        //public bool CursorVisible
        //{
        //    get {return _window.CursorVisible; }
        //}
		public VSyncMode VSync
		{
			get {return _window.VSync;}
			set {_window.VSync = value;}
		}
		public GameWindow Window
		{
			get{ return _window; }
		}
		
		int _framebuffer = -1;
		
		GameFrame _currentFrame;
		GameFrame _nextFrame;
		
		public GameFrame CurrentFrame {get{return _currentFrame;} set {_nextFrame = value;}}

		public BlueberryGame(int width, int height, string name, bool fullscreen, double contextVersion = 0)
		{
			_current = this;
            if (contextVersion == 0)
            {
                int major = (int)contextVersion;
                int minor = (int)((contextVersion - major) * 10);
                _window = new GameWindow(width, height,
                                         GraphicsMode.Default, name,
                                         fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,
                                         DisplayDevice.Default);
            }
            else
            {
                int major = (int)contextVersion;
                int minor = (int)((contextVersion - major) * 10);
                _window = new GameWindow(width, height,
                                         GraphicsMode.Default, name,
                                         fullscreen ? GameWindowFlags.Fullscreen : GameWindowFlags.Default,

                                         DisplayDevice.Default, major, minor,
#if DEBUG
                                        GraphicsContextFlags.Default);
#else
									 GraphicsContextFlags.ForwardCompatible);
#endif
            }
#if DEBUG
			new DiagnosticsCenter();
#endif
#if (WAV || OGG)
            new AudioManager(16, 8, 4096, true);
#endif
			_gamepads = new GamepadDevice[4];
			for (int i = 0; i < 4; i++) 
				_gamepads[i] = new GamepadDevice((UserIndex)i);
			//_window.VSync = VSyncMode.On;
			_window.UpdateFrame += InternalUpdate;
			_window.RenderFrame += InternalRender;
			_window.Load += (a, b)=>Load();
			
			Capabilities.Test();
			if (Capabilities.Framebuffers == GLExtensionSupport.Core)
            {
                GL.GenFramebuffers(1, out _framebuffer);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            } else if (Capabilities.Framebuffers == GLExtensionSupport.Extension)
            {
                GL.Ext.GenFramebuffers(1, out _framebuffer);
                GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
			GL.ClearColor(Color4.CornflowerBlue);
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

		public void SetFrame(GameFrame frame)
		{
			_nextFrame = frame;
		}
		
        public void SetRenderTarget(Texture target)
        {
            if (target == null)
            {
                if (Capabilities.Framebuffers == GLExtensionSupport.Core)
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                else
                    GL.Ext.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            else
            {
                if (Capabilities.Framebuffers == GLExtensionSupport.Core)
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
        protected virtual void Update()
        {
            
        }
		internal virtual void InternalUpdate(object sender, FrameEventArgs e)
		{
		    GS.Delta = (float)e.Time;
            GS.Total += (float)e.Time;

            Gamepad.Update(GS.Delta);
            
			if(_currentFrame != null)
                _currentFrame.Update(GS.Delta);
            
			if(_nextFrame != null)
			{
				if(_currentFrame != null)
					_currentFrame.Unload();
				_currentFrame = _nextFrame;
                _currentFrame._keyboard = Keyboard;
                _currentFrame._mouse = Mouse;
                _currentFrame._gamepad = Gamepad;
                _currentFrame._gamepads = Gamepads;
				_currentFrame.Load();
				_nextFrame = null;
			}
			#if DEBUG
            DiagnosticsCenter.Instance.Update(GS.Delta);
			if (Keyboard[Key.Tilde])
                if (DiagnosticsCenter.Instance.Visible) DiagnosticsCenter.Instance.Hide();
                else DiagnosticsCenter.Instance.Show();
            #endif
            Update();

		}
        protected virtual void Render()
        {
            
        }
		internal virtual void InternalRender(object sender, FrameEventArgs e)
		{
		    GS.Delta = (float)e.Time;
			if(_currentFrame != null)
                _currentFrame.Render(GS.Delta);
            Render();
			#if DEBUG
            DiagnosticsCenter.Instance.Draw(GS.Delta);
			#endif
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
            if (_currentFrame != null)
                _currentFrame.Unload();
			if(SpriteBatch.HasInstance)
				SpriteBatch.Please.Dispose();
			if(AudioManager.HasInstance)
				AudioManager.Instance.Dispose();
			if (_framebuffer != -1)
            {
                if(Capabilities.Framebuffers == GLExtensionSupport.Core)
                    GL.DeleteFramebuffers(1, ref _framebuffer);
                else
                    GL.Ext.DeleteFramebuffers(1, ref _framebuffer);
                _framebuffer = -1;
            }
		}
	}
}
