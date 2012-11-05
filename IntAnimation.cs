using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public class IntAnimation : Animation<int>
    {
        int left, right; // upper and lower bounds
        int sign; // shows animation direction (incermental or decremental)
        /// <summary>
        /// Constructor of IntAnimation
        /// </summary>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="period">Duration time</param>
        /// <param name="loopMode">Specify loop behaviour</param>
        public IntAnimation(int from, int to, double period, LoopMode loopMode)
        {
            From = from;
            To = to;
            left = Math.Min(from, to);
            right = Math.Max(from, to);
            if (From < To) sign = 1;
            else sign = -1;

            Period = period;
            Loop = loopMode;
            Value = From;
        }
        public IntAnimation()
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

        public static explicit operator int(IntAnimation anim)
        {
            return anim.Value;
        }
        // TODO: need to fix it all
        public override void Animate(double dtime)
        {
            timer += dtime;
            int val = (int)(timer / Period);
            if (val > 0)
            {
                Value += val * sign;
                timer = timer % Period;
            }
            if (Value > right || Value < left)
            {
                if (Loop == LoopMode.Loop) Value = From; // this is not right, see FloatAnimation
                else if (Loop == LoopMode.LoopWithReversing)
                {
                    if (sign > 0)
                        Value = right;
                    else
                        Value = left;
                    sign = -sign;
                }
                else { Value = To; Stop(); }
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