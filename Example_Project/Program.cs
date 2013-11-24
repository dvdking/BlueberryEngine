using System;
using Blueberry;
using System.Threading;
using System.Threading.Tasks;

namespace Example_Project
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var game = new BlueberryGame (640, 480, "Example project", false, 0);
			PrefabMgr.Load("Content//Prefabs.xml",System.Reflection.Assembly.GetAssembly(typeof(MainClass)));
			DataGameFrame frame = new DataGameFrame("Content//GameFrames//GameplayFrame");
			game.SetFrame (frame);

			game.Run (60.0, 60.0);
		}
	}
}
