using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public class FloatAnimation : Animation<float>
    {
        float left, right; // upper and lower bounds
        int sign; // shows animation direction (incermental or decremental)
        float range;

        public FloatAnimation(float from, float to, float period, LoopMode loopMode)
        {
            From = from;
            To = to;
            left = Math.Min(from, to);
            right = Math.Max(from, to);
            if (From < To) sign = 1;
            else sign = -1;
            range = right - left;

            Period = period;
            Loop = loopMode;
            Value = From;
        }
        public FloatAnimation()
        {
            From = 0;
            To = 10;
            Period = 0;
            Loop = LoopMode.None;
            Value = 0;
        }
        public void Reset()
        {
            Value = From;
            timer = 0;
            if (From < To) sign = 1;
            else sign = -1;
        }

        public static explicit operator float(FloatAnimation anim)
        {
            return anim.Value;
        }

        public override void Animate(double dtime)
        {
            timer += dtime;
            float interval = (float)(timer / Period);
            if (interval > 1)
            {
                if (Loop == LoopMode.Loop)
                    Value = From + (sign > 0 ? (interval - 1) * range : (1 - interval) * range);
                else if (Loop == LoopMode.LoopWithReversing)
                {
                    Value = sign > 0 ? right - (interval - 1) * range : left + (interval - 1) * range;
                    sign = -sign;
                }
                else
                {
                    Value = To;
                    Stop();
                }
                timer = 0;
            }
            else
            {
                if (sign > 0)
                    Value = (left + interval * range);
                else
                    Value = (right - interval * range);
            }
        }
        public override void Play(bool restart = false)
        {
            base.Play(restart);
            if (restart)
                timer = 0;
        }
        public override void Stop()
        {
            base.Stop();
            timer = 0;
        }
    }
}