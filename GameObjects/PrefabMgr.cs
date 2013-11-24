using System;
using System.Collections.Generic;
using System.Reflection;
using Blueberry.GameObjects;
using System.Linq;
using System.Xml;
using System.Diagnostics;

namespace Blueberry
{
	public static class PrefabMgr
    {
		private static List<GameObject> _prefabs = new List<GameObject>();

		public static void Load(string path, Assembly assembly)
		{
			var components = System.Reflection.Assembly.GetAssembly(typeof(Component)).GetTypes();
			Dictionary<string, ConstructorInfo> _components = new Dictionary<string, ConstructorInfo>();
			foreach (var item in components)
			{
				if (item.IsSubclassOf(typeof(Component)))
				{
					_components.Add(item.Name, item.GetConstructor(new Type[]{}));
				}
			}

			components = assembly.GetTypes();
			foreach (var item in components)
			{
				if (item.IsSubclassOf(typeof(Component)))
				{
					_components.Add(item.Name, item.GetConstructor(new Type[]{}));
				}
			}

			var file = new XmlDocument();
			file.Load(path);

			XmlNode main = file.SelectSingleNode("Prefabs");

			ResourceMgr.LoadResourcesXmlData(main);

			XmlNodeList gameObjects = main.SelectNodes("Prefab");
			foreach (XmlNode go in gameObjects)
			{
				string prefabName = go.Attributes["name"].Value;

				GameObject gameObject = new GameObject();
				gameObject.Name = prefabName;
				gameObject.IsPrefab = true;
				_prefabs.Add(gameObject);
			}

			foreach (XmlNode go in gameObjects)
			{
				string objectName = go.Attributes["name"].Value;

				GameObject gameObject = _prefabs.Find(p => p.Name == objectName);

				XmlNodeList componentsNodes = go.SelectNodes("Component");
				foreach (XmlNode c in componentsNodes)
				{
					string componentName = c.Attributes["name"].Value;
					ConstructorInfo constructor = _components[componentName];
					if (constructor == null)
					{
						Debug.Assert(false, "CONSTRUCTOR WAS NOT FOUND");
						continue;
					}
					Component comp = (Component)constructor.Invoke(null);

					if (comp == null)
					{
						Debug.Fail("the component was not created");
					}

					for (int i = 1; i < c.Attributes.Count; i++)
					{
						var attributeName = c.Attributes[i].Name;
						var attributeValue = c.Attributes[i].Value;

						comp.SetFieldValue(attributeName, attributeValue);
					}

					if (comp is Transform)
					{
						gameObject.Transform = (Transform)comp;
					}

					gameObject.AddComponents(comp);
				}
			}
		}

		public static GameObject Create(string name)
		{
			var go = _prefabs.Find(p => p.Name == name);

			if (go == null)
			{
				return null;
			}

			return go.Clone();
		}
    }
}

