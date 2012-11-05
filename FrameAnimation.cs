using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public class FrameAnimation : IntAnimation
    {
        bool horisontal;
        Rectangle current;
        public Rectangle CurrentFrame 
        {
            get { return current; }
        }
        public readonly int FrameWidth;
        public readonly int FrameHeight;

        public FrameAnimation(bool horisontal, int frameWidth, int frameHeight, int startFrame, int endFrame, float fps):base(startFrame, endFrame,1/fps, LoopMode.None)
        {
            this.horisontal = horisontal;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            current.Width = FrameWidth;
            current.Height = FrameHeight;

        }
        public override void Animate(double dt)
        {
            base.Animate(dt);
            if (horisontal) current.X = Value * FrameWidth; else current.Y = Value * FrameHeight;
        }
        public static explicit operator Rectangle(FrameAnimation fanim)
        {
            return fanim.CurrentFrame;
        }
        public static explicit operator RectangleF(FrameAnimation fanim)
        {
            return fanim.CurrentFrame;
        }
    }
}
