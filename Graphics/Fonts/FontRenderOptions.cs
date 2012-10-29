using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using OpenTK;
using OpenTK.Graphics;

namespace Blueberry.Graphics.Fonts
{
    public enum FontAlignment { Left = 0, Right, Centre, Justify }

    public enum FontMonospacing { Natural = 0, Yes, No }

    public class FontRenderOptions
    {
        /// <summary>Spacing between characters in units of average glyph width</summary>
        public float CharacterSpacing = 0.05f;

        /// <summary>Spacing between words in units of average glyph width</summary>
        public float WordSpacing = 0.9f;

        /// <summary>Line spacing in units of max glyph width</summary>
        public float LineSpacing = 1.0f;

        /// <summary>
        /// Whether to render the font in monospaced mode. If set to "Natural", then
        /// monospacing will be used if the font loaded font was detected to be monospaced.
        /// </summary>
        public FontMonospacing Monospacing = FontMonospacing.Natural;

        #region Justify Options

        /// <summary>
        /// When a line of text is justified, space may be inserted between
        /// characters, and between words.
        ///
        /// This parameter determines how this choice is weighted:
        ///
        /// 0.0f => word spacing only
        /// 1.0f => "fairly" distributed between both
        /// > 1.0 => in favour of character spacing
        ///
        /// This applies to expansions only.
        ///
        /// </summary>
        public float JustifyCharacterWeightForExpand
        {
            get { return justifyCharWeightForExpand; }
            set
            {
                justifyCharWeightForExpand = value;

                if (justifyCharWeightForExpand < 0f)
                    justifyCharWeightForExpand = 0f;
                else if (justifyCharWeightForExpand > 1.0f)
                    justifyCharWeightForExpand = 1.0f;
            }
        }

        private float justifyCharWeightForExpand = 0.5f;

        /// <summary>
        /// When a line of text is justified, space may be removed between
        /// characters, and between words.
        ///
        /// This parameter determines how this choice is weighted:
        ///
        /// 0.0f => word spacing only
        /// 1.0f => "fairly" distributed between both
        /// > 1.0 => in favour of character spacing
        ///
        /// This applies to contractions only.
        ///
        /// </summary>
        public float JustifyCharacterWeightForContract
        {
            get { return justifyCharWeightForContract; }
            set
            {
                justifyCharWeightForContract = value;

                if (justifyCharWeightForContract < 0f)
                    justifyCharWeightForContract = 0f;
                else if (justifyCharWeightForContract > 1.0f)
                    justifyCharWeightForContract = 1.0f;
            }
        }

        private float justifyCharWeightForContract = 0.2f;

        /// <summary>Total justification cap as a fraction of the boundary width.</summary>
        public float JustifyCapExpand = 0.5f;

        /// <summary>Total justification cap as a fraction of the boundary width.</summary>
        public float JustifyCapContract = 0.1f;

        /// <summary>
        /// By what factor justification is penalized for being negative.
        ///
        /// (e.g. if this is set to 3, then a contraction will only happen
        /// over an expansion if it is 3 of fewer times smaller than the
        /// expansion).
        /// </summary>
        public float JustifyContractionPenalty = 2;

        #endregion Justify Options

        public FontRenderOptions CreateClone()
        {
            var clone = new FontRenderOptions();

            clone.CharacterSpacing = CharacterSpacing;
            clone.WordSpacing = WordSpacing;
            clone.LineSpacing = LineSpacing;
            clone.Monospacing = Monospacing;
            clone.JustifyCharacterWeightForExpand = JustifyCharacterWeightForExpand;
            clone.justifyCharWeightForExpand = justifyCharWeightForExpand;
            clone.JustifyCharacterWeightForContract = JustifyCharacterWeightForContract;
            clone.justifyCharWeightForContract = justifyCharWeightForContract;
            clone.JustifyCapExpand = JustifyCapExpand;
            clone.JustifyCapContract = JustifyCapContract;
            clone.JustifyContractionPenalty = JustifyContractionPenalty;

            return clone;
        }
    }
}