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
			Task t = new Task(new Action(() =>
			{
					//PrefabMgr.Load("Content//Prefabs.xml",System.Reflection.Assembly.GetAssembly(typeof(MainClass)));
				}));
			//t.Start();

			PrefabMgr.Load("Content//Prefabs.xml",System.Reflection.Assembly.GetAssembly(typeof(MainClass)));
			var game = new BlueberryGame (640, 480, "Example project", false, 0);
			DataGameFrame frame = new DataGameFrame("Content//GameplayFrame");


			game.SetFrame (frame);
			game.Run (60.0, 60.0);
		}
	}
}
