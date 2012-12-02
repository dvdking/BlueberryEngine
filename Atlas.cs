using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using Blueberry.Graphics;

namespace Blueberry
{
    public class Atlas
    {
        Dictionary<string, Rectangle> content;
        Texture tex;

        public Rectangle this[string name] { get { return content[name]; } }

        public Rectangle this[int index] { get { return content.Values.ElementAt(index); } }

        public int Count { get { return content.Count; } }

        public Texture Texture { get { return tex; } }

        public Atlas()
        {
            content = new Dictionary<string, Rectangle>();
        }

        public void Load(Texture atlas, string declaration_file)
        {
            XmlTextReader r = new XmlTextReader(declaration_file);
            r.WhitespaceHandling = WhitespaceHandling.None;
            string[] tmp;
            Rectangle rectangle = Rectangle.Empty;
            this.tex = atlas;

            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element && r.Name == "part")
                {
                    string name = r.GetAttribute("name");
                    string tl = r.GetAttribute("tl");
                    string br = r.GetAttribute("br");
                    string rect = r.GetAttribute("rect");
                    string border = r.GetAttribute("border");

                    if (tl != null)
                    {
                        tmp = tl.Split(' ');
                        rectangle.X = int.Parse(tmp[0]);
                        rectangle.Y = int.Parse(tmp[1]);
                    }
                    if (br != null)
                    {
                        tmp = br.Split(' ');
                        rectangle.Width = int.Parse(tmp[0]) - rectangle.X;
                        rectangle.Height = int.Parse(tmp[1]) - rectangle.Y;
                    }
                    if (rect != null)
                    {
                        tmp = rect.Split(' ');
                        rectangle.X = int.Parse(tmp[0]);
                        rectangle.Y = int.Parse(tmp[1]);
                        rectangle.Width = int.Parse(tmp[2]);
                        rectangle.Height = int.Parse(tmp[3]);
                    }
                    if (border != null)
                    {
                        int b = int.Parse(border);
                        rectangle.Inflate(-b, -b);
                    }

                    content.Add(name, rectangle);
                }
            }
        }

        public Tile GetTile(string name)
        {
            return new Tile() { name = name, texture = tex, source = this[name] };
        }
    }
}