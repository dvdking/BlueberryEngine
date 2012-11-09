using System;
using OpenTK;

namespace Blueberry.Particles
{
	/// <summary>
	/// Description of ReleaseInfo.
	/// </summary>
	public struct ReleaseInformation
	{
		public Single TotalSeconds;
		public Vector2 Position;
		public Vector2 Direction;
		public ReleaseInformation(float totalSeconds, Vector2 position, Vector2 direction)
		{
			this.TotalSeconds = totalSeconds;
			this.Position = position;
			this.Direction = direction;
		}	
	}
}
