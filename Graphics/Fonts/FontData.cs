using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Xml;

namespace Blueberry.Graphics.Fonts
{
    internal class FontData
    {
        /// <summary>Mapping from a pair of characters to a pixel offset</summary>
        public Dictionary<String, int> KerningPairs;

        /// <summary>List of texture pages</summary>
        public TexturePage[] Pages;

        /// <summary>Mapping from character to glyph index</summary>
        public Dictionary<char, FontGlyph> CharSetMapping;

        /// <summary>The average glyph width</summary>
        public float meanGlyphWidth;

        /// <summary>The maximum glyph height</summary>
        public int maxGlyphHeight;

        /// <summary>Whether the original font (from ttf) was detected to be monospaced</summary>
        public bool naturallyMonospaced = false;

        public bool IsMonospacingActive(FontRenderOptions options)
        {
            return (options.Monospacing == FontMonospacing.Natural && naturallyMonospaced) || options.Monospacing == FontMonospacing.Yes;
        }

        public float GetMonoSpaceWidth(FontRenderOptions options)
        {
            return (float)Math.Ceiling(1 + (1 + options.CharacterSpacing) * meanGlyphWidth);
        }

        public string Serialize()
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter w = new XmlTextWriter(new StringWriter(sb));
            w.WriteStartDocument();
            w.WriteStartElement("FontData");
            w.WriteAttributeString("Pages", Pages.Length.ToString());
            w.WriteAttributeString("CharSetLen", CharSetMapping.Count.ToString());

            foreach (var glyphChar in CharSetMapping)
            {
                var chr = glyphChar.Key;
                var glyph = glyphChar.Value;
                w.WriteStartElement("Glyph");
                w.WriteAttributeString("char", chr.ToString());
                w.WriteAttributeString("page", glyph.page.ToString());
                w.WriteAttributeString("rect", glyph.rect.X + " " + glyph.rect.Y + " " + glyph.rect.Width + " " + glyph.rect.Height);
                w.WriteAttributeString("yoffset", glyph.yOffset.ToString());
                w.WriteEndElement();
            }

            w.WriteEndElement();
            w.WriteEndDocument();
            w.Close();
            return sb.ToString();
        }

        public void Deserialize(string input, out int pageCount, out char[] charSet)
        {
            CharSetMapping = new Dictionary<char, FontGlyph>();
            var charSetList = new List<char>();
            pageCount = 0;

            XmlTextReader r = new XmlTextReader(new StringReader(input));

            while (r.Read())
            {
                if (r.NodeType == XmlNodeType.Element && r.Name == "FontData")
                {
                    pageCount = int.Parse(r.GetAttribute("Pages"));
                    int glyphCount = int.Parse(r.GetAttribute("CharSetLen"));

                    while (r.Read() && r.NodeType == XmlNodeType.Element && r.Name == "Glyph")
                    {
                        if (CharSetMapping.Count < glyphCount)
                        {
                            char c = r.GetAttribute("char")[0];
                            var vals = r.GetAttribute("rect").Split(' ');
                            var glyph = new FontGlyph(int.Parse(r.GetAttribute("page")),
                                new Rectangle(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]), int.Parse(vals[3])),
                                int.Parse(r.GetAttribute("yoffset")), c);

                            CharSetMapping.Add(c, glyph);
                            charSetList.Add(c);
                        }
                    }
                }
            }
            charSet = charSetList.ToArray();
        }

        public void CalculateMeanWidth()
        {
            meanGlyphWidth = 0f;
            foreach (var glyph in CharSetMapping)
                meanGlyphWidth += glyph.Value.rect.Width;

            meanGlyphWidth /= CharSetMapping.Count;
        }

        public void CalculateMaxHeight()
        {
            maxGlyphHeight = 0;
            foreach (var glyph in CharSetMapping)
                maxGlyphHeight = Math.Max(glyph.Value.rect.Height, maxGlyphHeight);
        }

        /// <summary>
        /// Returns the kerning length correction for the character at the given index in the given string.
        /// Also, if the text is part of a textNode list, the nextNode is given so that the following
        /// node can be checked incase of two adjacent word nodes.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="text"></param>
        /// <param name="textNode"></param>
        /// <returns></returns>
        public int GetKerningPairCorrection(int index, string text, TextNode textNode)
        {
            if (KerningPairs == null)
                return 0;

            var chars = new char[2];

            if (index + 1 == text.Length)
            {
                if (textNode != null && textNode.Next != null && textNode.Next.Type == TextNodeType.Word)
                    chars[1] = textNode.Next.Text[0];
                else
                    return 0;
            }
            else
            {
                chars[1] = text[index + 1];
            }

            chars[0] = text[index];

            String str = new String(chars);

            if (KerningPairs.ContainsKey(str))
                return KerningPairs[str];

            return 0;
        }

        public void Dispose()
        {
            foreach (var page in Pages)
                page.Dispose();
        }
    }
}