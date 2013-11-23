using Blueberry.Input;
using OpenTK.Input;
using System;
using System.Xml;
using Blueberry.GameObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;


namespace Blueberry
{
	public class DataGameFrame:GameFrame
    {
		private GameObjectsManager _goManager;
		private Camera _camera;

		public DataGameFrame(string name)
        {
			_camera = new Camera(new System.Drawing.Size(640, 480), new System.Drawing.Point(0, 0), true);//fix this shit
			_goManager = new GameObjectsManager(_camera);

			var path = name + ".xml";
			var file = new XmlDocument();
			file.Load(path);
		
			XmlNode main = file.SelectSingleNode("Frame");
			XmlNode gameObjectsNode = main.SelectSingleNode("GameObjects");
			XmlNodeList gameObjects = gameObjectsNode.SelectNodes("GameObject");
			foreach (XmlNode go in gameObjects)
			{
				string prefabName = go.Attributes["prefab"].Value;
				string objectName = go.Attributes["name"].Value;

				GameObject gameObject = PrefabMgr.Create(prefabName);
				gameObject.Name = objectName;

				_goManager.AddObject(gameObject);
			}
			_goManager.UpdateObjectsEnqueues();

			foreach (XmlNode go in gameObjects)
			{
				string objectName = go.Attributes["name"].Value;

				GameObject gameObject = _goManager.GetByName(objectName);

				XmlNodeList components = go.SelectNodes("Component");
				foreach (XmlNode c in components)
				{
					string componentName = c.Attributes["name"].Value;

					Component comp = gameObject.GetComponent(componentName);

					for (int i = 1; i < c.Attributes.Count; i++)
					{
						var attributeName = c.Attributes[i].Name;
						var attributeValue = c.Attributes[i].Value;

						comp.SetFieldValue(attributeName, attributeValue);
					}

					gameObject.AddComponents(comp);
				}
			}
        }

		public override void Update(float dt)
		{
			base.Update(dt);
			_goManager.Update(dt);
		}
		
		public override void Render(float dt)
		{
			GL.ClearColor(Color4.CornflowerBlue);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			_goManager.Draw(dt);
			base.Render(dt);
		}

    }
}

