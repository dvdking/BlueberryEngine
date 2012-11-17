using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Blueberry.Graphics.Fonts
{
    public class BitmapFont
    {
        private FontRenderOptions options = new FontRenderOptions();
        internal FontData fontData;

        public FontRenderOptions Options { get { return options; } }

        #region Constructors and font builders

        private BitmapFont() { }

        internal BitmapFont(FontData fontData) { this.fontData = fontData; }

        public BitmapFont(Font font) : this(font, null) { }

        public BitmapFont(Font font, FontBuilderConfiguration config)
        {
            if (config == null)
                config = new FontBuilderConfiguration();

            fontData = BuildFont(font, config, null);
        }

        public BitmapFont(string fileName, float size) : this(fileName, size, FontStyle.Regular, null) { }

        public BitmapFont(string fileName, float size, FontStyle style) : this(fileName, size, style, null) { }

        public BitmapFont(string fileName, float size, FontBuilderConfiguration config) : this(fileName, size, FontStyle.Regular, config) { }

        public BitmapFont(string fileName, float size, FontStyle style, FontBuilderConfiguration config)
        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fileName);
            var fontFamily = pfc.Families[0];

            if (!fontFamily.IsStyleAvailable(style))
                throw new ArgumentException("Font file: " + fileName + " does not support style: " + style);

            if (config == null)
                config = new FontBuilderConfiguration();

            float fontScale = 1f;

            using (var font = new Font(fontFamily, size * fontScale * config.SuperSampleLevels, style))
            {
                fontData = BuildFont(font, config, null);
            }
        }

        public static void CreateTextureFontFiles(Font font, string newFontName) { CreateTextureFontFiles(font, newFontName, new FontBuilderConfiguration()); }

        public static void CreateTextureFontFiles(Font font, string newFontName, FontBuilderConfiguration config)
        {
            var fontData = BuildFont(font, config, newFontName);
            Builder.SaveQFontDataToFile(fontData, newFontName);
        }

        public static void CreateTextureFontFiles(string fileName, float size, string newFontName) { CreateTextureFontFiles(fileName, size, FontStyle.Regular, null, newFontName); }

        public static void CreateTextureFontFiles(string fileName, float size, FontStyle style, string newFontName) { CreateTextureFontFiles(fileName, size, style, null, newFontName); }

        public static void CreateTextureFontFiles(string fileName, float size, FontBuilderConfiguration config, string newFontName) { CreateTextureFontFiles(fileName, size, FontStyle.Regular, config, newFontName); }

        public static void CreateTextureFontFiles(string fileName, float size, FontStyle style, FontBuilderConfiguration config, string newFontName)
        {
            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fileName);
            var fontFamily = pfc.Families[0];

            if (!fontFamily.IsStyleAvailable(style))
                throw new ArgumentException("Font file: " + fileName + " does not support style: " + style);

            FontData fontData = null;
            if (config == null)
                config = new FontBuilderConfiguration();

            using (var font = new Font(fontFamily, size * config.SuperSampleLevels, style))
            {
                fontData = BuildFont(font, config, newFontName);
            }

            Builder.SaveQFontDataToFile(fontData, newFontName);
        }

        public static BitmapFont FromQFontFile(string filePath) { return FromQFontFile(filePath, 1.0f, null); }

        public static BitmapFont FromQFontFile(string filePath, FontKerningConfiguration kerningConfig) { return FromQFontFile(filePath, 1.0f, kerningConfig); }

        public static BitmapFont FromQFontFile(string filePath, float downSampleFactor) { return FromQFontFile(filePath, downSampleFactor, null); }

        public static BitmapFont FromQFontFile(string filePath, float downSampleFactor, FontKerningConfiguration kerningConfig)
        {
            if (kerningConfig == null)
                kerningConfig = new FontKerningConfiguration();

            float fontScale = 1f;

            BitmapFont qfont = new BitmapFont();
            qfont.fontData = Builder.LoadQFontDataFromFile(filePath, downSampleFactor * fontScale, kerningConfig);

            return qfont;
        }

        private static FontData BuildFont(Font font, FontBuilderConfiguration config, string saveName)
        {
            Builder builder = new Builder(font, config);
            return builder.BuildFontData(saveName);
        }

        #endregion Constructors and font builders

        public float LineSpacing
        {
            get { return (float)Math.Ceiling(fontData.maxGlyphHeight * Options.LineSpacing); }
        }

        public bool IsMonospacingActive
        {
            get { return fontData.IsMonospacingActive(Options); }
        }

        public float MonoSpaceWidth
        {
            get { return fontData.GetMonoSpaceWidth(Options); }
        }

        internal float MeasureNextlineLength(string text)
        {
            float xOffset = 0;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                if (c == '\r' || c == '\n')
                {
                    break;
                }

                if (IsMonospacingActive)
                {
                    xOffset += MonoSpaceWidth;
                }
                else
                {
                    //space
                    if (c == ' ')
                    {
                        xOffset += (float)Math.Ceiling(fontData.meanGlyphWidth * Options.WordSpacing);
                    }
                    //normal character
                    else if (fontData.CharSetMapping.ContainsKey(c))
                    {
                        FontGlyph glyph = fontData.CharSetMapping[c];
                        xOffset += (float)Math.Ceiling(glyph.rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, text, null));
                    }
                }
            }
            return xOffset;
        }

        /// <summary>Measures the actual width and height of the block of text.</summary>
        /// <param name="text"></param>
        /// <param name="bounds"></param>
        /// <param name="alignment"></param>
        /// <returns></returns>
        public SizeF Measure(string text, float maxWidth, bool justify)
        {
            var a = typeof(Size);
            var processedText = ProcessText(text, maxWidth, justify);
            return Measure(processedText);
        }

        public SizeF MeasureSymbol(char symbol)
        {
            FontGlyph glyph = fontData.CharSetMapping[symbol];
            return new SizeF(glyph.rect.Width, glyph.rect.Height);
        }
        public SizeF Measure(string text)
        {
            float maxXpos = float.MinValue;
            float minXPos = float.MaxValue;

            float maxYPos = 0;
            float xOffset = 0f;
            float yOffset = 0f;

            text = text.Replace("\r\n", "\r");

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];

                //newline
                if (c == '\r' || c == '\n')
                {
                    yOffset += LineSpacing;
                    xOffset = 0f;
                    maxYPos = 0;
                }
                else
                {
                    minXPos = Math.Min(xOffset, minXPos);
                    FontGlyph glyph = null;
                    if(c != ' ')
                    {
                        glyph = fontData.CharSetMapping[c];
                        maxYPos = Math.Max(maxYPos, glyph.yOffset);
                    }
                    if (IsMonospacingActive)
                        xOffset += MonoSpaceWidth;
                    else
                    {
                        if (c == ' ')
                            xOffset += (float)Math.Ceiling(fontData.meanGlyphWidth * Options.WordSpacing);
                        //normal character
                        else if (fontData.CharSetMapping.ContainsKey(c))
                        {
                            xOffset += (float)Math.Ceiling(glyph.rect.Width + fontData.meanGlyphWidth * Options.CharacterSpacing + fontData.GetKerningPairCorrection(i, text, null));
                        }
                    }

                    maxXpos = Math.Max(xOffset, maxXpos);
                }
            }

            float maxWidth = 0f;

            if (minXPos != float.MaxValue)
                maxWidth = maxXpos - minXPos;

            return new SizeF(maxWidth, yOffset + LineSpacing + maxYPos);
        }

        /// <summary>Computes the length of the next line, and whether the line is valid for justification.</summary>
        /// <param name="node"></param>
        /// <param name="maxLength"></param>
        /// <param name="justifable"></param>
        /// <returns></returns>
        internal float TextNodeLineLength(TextNode node, float maxLength)
        {
            if (node == null)
                return 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            for (; node != null; node = node.Next)
            {
                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (SkipTrailingSpace(node, length, maxLength) && atLeastOneNodeCosumedOnLine)
                    break;

                if (length + node.Length <= maxLength || !atLeastOneNodeCosumedOnLine)
                {
                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                }
                else
                {
                    break;
                }
            }
            return length;
        }

        internal bool CrumbledWord(TextNode node)
        {
            return (node.Type == TextNodeType.Word && node.Next != null && node.Next.Type == TextNodeType.Word);
        }

        /// <summary>Computes the length of the next line, and whether the line is valid for justification.</summary>
        internal void JustifyLine(TextNode node, float targetLength)
        {
            bool justifiable = false;

            if (node == null)
                return;

            var headNode = node; //keep track of the head node

            //start by finding the length of the block of text that we know will actually fit:

            int charGaps = 0;
            int spaceGaps = 0;

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0;
            var expandEndNode = node; //the node at the end of the smaller list (before adding additional word)
            for (; node != null; node = node.Next)
            {
                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (SkipTrailingSpace(node, length, targetLength) && atLeastOneNodeCosumedOnLine)
                {
                    justifiable = true;
                    break;
                }

                if (length + node.Length < targetLength || !atLeastOneNodeCosumedOnLine)
                {
                    expandEndNode = node;

                    if (node.Type == TextNodeType.Space)
                        spaceGaps++;

                    if (node.Type == TextNodeType.Word)
                    {
                        charGaps += (node.Text.Length - 1);

                        //word was part of a crumbled word, so there's an extra char cap between the two words
                        if (CrumbledWord(node))
                            charGaps++;
                    }

                    atLeastOneNodeCosumedOnLine = true;
                    length += node.Length;
                }
                else
                {
                    justifiable = true;
                    break;
                }
            }

            //now we check how much additional length is added by adding an additional word to the line
            float extraLength = 0f;
            int extraSpaceGaps = 0;
            int extraCharGaps = 0;
            bool contractPossible = false;
            TextNode contractEndNode = null;
            for (node = expandEndNode.Next; node != null; node = node.Next)
            {
                if (node.Type == TextNodeType.LineBreak)
                    break;

                if (node.Type == TextNodeType.Space)
                {
                    extraLength += node.Length;
                    extraSpaceGaps++;
                }
                else if (node.Type == TextNodeType.Word)
                {
                    contractEndNode = node;
                    contractPossible = true;
                    extraLength += node.Length;
                    extraCharGaps += (node.Text.Length - 1);
                    break;
                }
            }

            if (justifiable)
            {
                //last part of this condition is to ensure that the full contraction is possible (it is all or nothing with contractions, since it looks really bad if we don't manage the full)
                bool contract = contractPossible && (extraLength + length - targetLength) * Options.JustifyContractionPenalty < (targetLength - length) &&
                    ((targetLength - (length + extraLength + 1)) / targetLength > -Options.JustifyCapContract);

                if ((!contract && length < targetLength) || (contract && length + extraLength > targetLength))  //calculate padding pixels per word and char
                {
                    if (contract)
                    {
                        length += extraLength + 1;
                        charGaps += extraCharGaps;
                        spaceGaps += extraSpaceGaps;
                    }

                    int totalPixels = (int)(targetLength - length); //the total number of pixels that need to be added to line to justify it
                    int spacePixels = 0; //number of pixels to spread out amongst spaces
                    int charPixels = 0; //number of pixels to spread out amongst char gaps

                    if (contract)
                    {
                        if (totalPixels / targetLength < -Options.JustifyCapContract)
                            totalPixels = (int)(-Options.JustifyCapContract * targetLength);
                    }
                    else
                    {
                        if (totalPixels / targetLength > Options.JustifyCapExpand)
                            totalPixels = (int)(Options.JustifyCapExpand * targetLength);
                    }

                    //work out how to spread pixles between character gaps and word spaces
                    if (charGaps == 0)
                    {
                        spacePixels = totalPixels;
                    }
                    else if (spaceGaps == 0)
                    {
                        charPixels = totalPixels;
                    }
                    else
                    {
                        if (contract)
                            charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForContract * charGaps / spaceGaps);
                        else
                            charPixels = (int)(totalPixels * Options.JustifyCharacterWeightForExpand * charGaps / spaceGaps);

                        if ((!contract && charPixels > totalPixels) ||
                            (contract && charPixels < totalPixels))
                            charPixels = totalPixels;

                        spacePixels = totalPixels - charPixels;
                    }

                    int pixelsPerChar = 0;  //minimum number of pixels to add per char
                    int leftOverCharPixels = 0; //number of pixels remaining to only add for some chars

                    if (charGaps != 0)
                    {
                        pixelsPerChar = charPixels / charGaps;
                        leftOverCharPixels = charPixels - pixelsPerChar * charGaps;
                    }

                    int pixelsPerSpace = 0; //minimum number of pixels to add per space
                    int leftOverSpacePixels = 0; //number of pixels remaining to only add for some spaces

                    if (spaceGaps != 0)
                    {
                        pixelsPerSpace = spacePixels / spaceGaps;
                        leftOverSpacePixels = spacePixels - pixelsPerSpace * spaceGaps;
                    }

                    //now actually iterate over all nodes and set tweaked length
                    for (node = headNode; node != null; node = node.Next)
                    {
                        if (node.Type == TextNodeType.Space)
                        {
                            node.LengthTweak = pixelsPerSpace;
                            if (leftOverSpacePixels > 0)
                            {
                                node.LengthTweak += 1;
                                leftOverSpacePixels--;
                            }
                            else if (leftOverSpacePixels < 0)
                            {
                                node.LengthTweak -= 1;
                                leftOverSpacePixels++;
                            }
                        }
                        else if (node.Type == TextNodeType.Word)
                        {
                            int cGaps = (node.Text.Length - 1);
                            if (CrumbledWord(node))
                                cGaps++;

                            node.LengthTweak = cGaps * pixelsPerChar;

                            if (leftOverCharPixels >= cGaps)
                            {
                                node.LengthTweak += cGaps;
                                leftOverCharPixels -= cGaps;
                            }
                            else if (leftOverCharPixels <= -cGaps)
                            {
                                node.LengthTweak -= cGaps;
                                leftOverCharPixels += cGaps;
                            }
                            else
                            {
                                node.LengthTweak += leftOverCharPixels;
                                leftOverCharPixels = 0;
                            }
                        }

                        if ((!contract && node == expandEndNode) || (contract && node == contractEndNode))
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Checks whether to skip trailing space on line because the next word does not fit.
        /// We only check one space - the assumption is that if there is more than one,
        /// it is a deliberate attempt to insert spaces.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="lengthSoFar"></param>
        /// <param name="boundWidth"></param>
        /// <returns></returns>
        internal bool SkipTrailingSpace(TextNode node, float lengthSoFar, float boundWidth)
        {
            if (node.Type == TextNodeType.Space && node.Next != null && node.Next.Type == TextNodeType.Word && node.ModifiedLength + node.Next.ModifiedLength + lengthSoFar > boundWidth)
                return true;
            return false;
        }

        /// <summary>Creates node list object associated with the text.</summary>
        /// <returns></returns>
        public ProcessedText ProcessText(string text, float maxWidth, bool justify)
        {
            var nodeList = new TextNodeList(text);
            nodeList.MeasureNodes(fontData, Options);

            //we "crumble" words that are two long so that that can be split up
            var nodesToCrumble = new List<TextNode>();
            foreach (TextNode node in nodeList)
                if (node.Length >= maxWidth && node.Type == TextNodeType.Word)
                    nodesToCrumble.Add(node);

            foreach (var node in nodesToCrumble)
                nodeList.Crumble(node, 1);

            //need to measure crumbled words
            nodeList.MeasureNodes(fontData, Options);

            var processedText = new ProcessedText();
            processedText.textNodeList = nodeList;
            processedText.maxWidth = maxWidth;
            processedText.justify = justify;

            return processedText;
        }

        /// <summary>Measures the actual width and height of the block of text</summary>
        /// <param name="processedText"></param>
        /// <returns></returns>
        internal SizeF Measure(ProcessedText processedText)
        {
            float maxMeasuredWidth = 0f;

            float maxWidth = processedText.maxWidth;

            float yOffset = 0;

            var nodeList = processedText.textNodeList;
            for (TextNode node = nodeList.Head; node != null; node = node.Next)
                node.LengthTweak = 0f;  //reset tweaks

            bool atLeastOneNodeCosumedOnLine = false;
            float length = 0f;
            for (TextNode node = nodeList.Head; node != null; node = node.Next)
            {
                bool newLine = false;

                if (node.Type == TextNodeType.LineBreak)
                    newLine = true;
                else
                {
                    if (SkipTrailingSpace(node, length, maxWidth) && atLeastOneNodeCosumedOnLine)
                    {
                        newLine = true;
                    }
                    else if (length + node.ModifiedLength <= maxWidth || !atLeastOneNodeCosumedOnLine)
                    {
                        atLeastOneNodeCosumedOnLine = true;
                        length += node.ModifiedLength;
                        maxMeasuredWidth = Math.Max(length, maxMeasuredWidth);
                    }
                    else
                    {
                        newLine = true;
                        if (node.Previous != null)
                            node = node.Previous;
                    }
                }

                if (newLine)
                {
                    yOffset += LineSpacing;
                    length = 0f;
                    atLeastOneNodeCosumedOnLine = false;
                }
            }

            return new SizeF(maxMeasuredWidth, yOffset + LineSpacing);
        }
    }
}