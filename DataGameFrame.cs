using Blueberry.Input;
using OpenTK.Input;
using System;
using System.Xml;
using Blueberry.GameObjects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using System.Collections.Generic;
using Blueberry.Graphics;


namespace Blueberry
{
	public class DataGameFrame:GameFrame
    {
		private GameObjectsManager _goManager;
		private Camera _camera;

		public DataGameFrame(string name)
        {
			var w = BlueberryGame.CurrentGame.Window.Width;
			var h = BlueberryGame.CurrentGame.Window.Height;

			_camera = new Camera(new System.Drawing.Size(w, h),
								 new System.Drawing.Point(0, 0), true);
			_goManager = new GameObjectsManager(_camera);

			var path = name + ".xml";
			var file = new XmlDocument();
			file.Load(path);
		
			XmlNode main = file.SelectSingleNode("Frame");

			ResourceMgr.LoadResourcesXmlData(main);
			LoadGameObjectsXmlData(main);

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
			SpriteBatch.Please.Begin(_camera.GetViewMatrix());
			_goManager.Draw(dt);
			SpriteBatch.Please.End();
			base.Render(dt);
		}
	

		void LoadGameObjectsXmlData(XmlNode main)
		{
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
    }
}

