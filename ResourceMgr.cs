using System;
using System.Security.Policy;
using System.Collections.Generic;
using Blueberry.Graphics;
using System.Xml;
using Blueberry.Graphics.Fonts;

namespace Blueberry
{
	public static class ResourceMgr
    {
		private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
		private static Dictionary<string, BitmapFont> _fonts = new Dictionary<string, BitmapFont>();
        private static Dictionary<string, Material> _materials = new Dictionary<string, Material>();

		private static List<string> _loadedResources = new List<string>();

		public static Texture GetTexture(string fieldValue)
		{
			return _textures[fieldValue];
		}
		public static BitmapFont GetFont(string fieldValue)
		{
			return _fonts[fieldValue];
		}
        public static Material GetMaterial(string name)
        {
            return _materials[name].Clone();
        }


		public static void LoadResources(string path)
		{
			var file = new XmlDocument();
			file.Load(path);

			XmlNode main = file.SelectSingleNode("Resources");

            LoadTextures(main);
            LoadFonts(main);
            LoadMaterials(main);

		}

        private static void LoadTextures(XmlNode main)
        {
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

        private static void LoadFonts(XmlNode main)
        {
            XmlNode fontNode = main.SelectSingleNode("Fonts");
            string fontBasePath = fontNode.Attributes["basePath"].Value;
            XmlNodeList fonts = fontNode.SelectNodes("Font");
            foreach (XmlNode f in fonts)
            {
                string gameName = f.Attributes["gameName"].Value;
                string fileName = f.Attributes["fileName"].Value;
                string sizestr = f.Attributes["size"].Value;
                float size = float.Parse(sizestr);
                BitmapFont font = new BitmapFont(fontBasePath + "//" + fileName, size);
                _fonts[gameName] = font;
            }
        }

        private static void LoadMaterials(XmlNode main)
        {
            XmlNode material = main.SelectSingleNode("Materials");
            XmlNodeList materials = material.SelectNodes("Material");
            string basePath = material.Attributes["basePath"].Value;

            foreach (XmlNode item in materials)
            {
                string name = item.Attributes["gameName"].Value;
                string vshader = item.Attributes["vshader"].Value;
                string fshader = item.Attributes["fshader"].Value;

                XmlNodeList @params = item.SelectNodes("Param");

                Shader shader = new Shader();
                shader.LoadVertexFile(basePath + "//" + vshader);
                shader.LoadFragmentFile(basePath + "//" + fshader);
                shader.Link();
                Material mat = new Material(shader);
                foreach (XmlNode param in @params)
                {
                    string paramName = param.Attributes["name"].Value;
                    string paramType = param.Attributes["type"].Value;
                    string paramValue = param.Attributes["value"].Value;

                    mat.SetParameter(paramName, paramType, paramValue);
                }

                _materials[name] = mat;
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

				if (!_loadedResources.Contains(item))
				{
					_loadedResources.Add(item);
				}
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

