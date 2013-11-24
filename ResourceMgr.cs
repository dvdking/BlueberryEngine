using System;
using System.Security.Policy;
using System.Collections.Generic;
using Blueberry.Graphics;
using System.Xml;

namespace Blueberry
{
	public static class ResourceMgr
    {
		private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
		private static List<string> _loadedResources = new List<string>();

		public static object GetTexture(string fieldValue)
		{
			return _textures[fieldValue];
		}



		public static void LoadResources(string path)
		{
			var file = new XmlDocument();
			file.Load(path);

			XmlNode main = file.SelectSingleNode("Resources");
			XmlNode texture = main.SelectSingleNode("Textures");

			string texturesBasePath = texture.Attributes["basePath"].Value;

			XmlNodeList textures = texture.SelectNodes("Texture");

			foreach (XmlNode tex in textures)
			{
				string texGameName = tex.Attributes["gameName"].Value;
				string texFileName = tex.Attributes["fileName"].Value;

				Texture t = new Texture(texturesBasePath + "//" + texFileName);
				_textures[texGameName] = t;
			}
		}

		public static void LoadResourcesXmlData(XmlNode main)
		{
			XmlNode resourceConfig = main.SelectSingleNode("ResourceConfigs");
			XmlNodeList resources = resourceConfig.SelectNodes("Resource");

			List<string> resourcesFilesNames = new List<string>();
			string basePath = resourceConfig.Attributes["basePath"].Value;
			foreach (XmlNode r in resources)
			{
				resourcesFilesNames.Add(r.Attributes["name"].Value);
			}

			foreach (var item in resourcesFilesNames)
			{
				LoadResources(basePath + "//" + item + ".xml");
				_loadedResources.Add(item);
			}
		}

		public static void UnloadResources()
		{
			foreach (var item in _textures)
			{
				item.Value.Dispose();
			}
			_textures.Clear();
			_loadedResources.Clear();
		}
    }
}

