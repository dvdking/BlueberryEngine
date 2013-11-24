using System;
using Blueberry.GameObjects;
using Blueberry.Graphics;
using Blueberry.Graphics.Fonts;
using OpenTK;
using OpenTK.Graphics;
namespace Example_Project
{
	public class GameControlComponent:Component, IDrawable
    {
		public GameObject Player;
		public GameObject Enemy;

		public BitmapFont Font;

		public float PosX, PosY;

		#region IDrawable implementation
		public void Draw(float dt)
		{
			if (Player == null || Player.IsDestroyed)
			{
				SpriteBatch.Please.PrintText(Font, "You lost!", new Vector2(PosX, PosY), Color4.White);
				Player = null;
			}
			else if (Enemy == null || Enemy.IsDestroyed)
			{
				SpriteBatch.Please.PrintText(Font, "You won", new Vector2(PosX, PosY), Color4.White);
				Enemy = null;
			}
		}
		#endregion
    }
}

