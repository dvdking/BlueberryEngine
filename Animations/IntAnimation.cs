using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.Animations
{
    public class IntAnimation : Animation<int>
    {
        int left, right; // upper and lower bounds
        int range;
        /// <summary>
        /// Constructor of IntAnimation
        /// </summary>
        /// <param name="from">Start value</param>
        /// <param name="to">End value</param>
        /// <param name="period">Duration time</param>
        /// <param name="loopMode">Specify loop behaviour</param>
        public IntAnimation(int from, int to, double period, LoopMode loopMode, Interpolator interpolator)
            :base(interpolator)
        {
            From = from;
            To = to;
            left = Math.Min(from, to);
            right = Math.Max(from, to);
            interpolatorOrientation = direction = From < To;

            range = right - left;
            Period = period;
            Loop = loopMode;
            Value = From;
        }
        public IntAnimation(int from, int to, double period, LoopMode loopMode)
            :this(from, to, period, loopMode, v=>v)
        {
        }
        public IntAnimation(int from, int to, double period)
            :this(from, to, period, LoopMode.Loop, v=>v)
        {
        }
        public IntAnimation()
            :this(0, 1, 0, LoopMode.None, v=>v)
        {
        }
        public void Reset()
        {
            Value = From;
            timer = 0;
        }

        public static explicit operator int(IntAnimation anim)
        {
            return anim.Value;
        }
        public override void Animate(double dtime)
        {
            base.Animate(dtime);

            if (direction)
            {
                if (Loop == LoopMode.LoopWithReversing && interpolatorOrientation != direction)
                {
                    Value = (int)(right - InterpolationFunction(1 - interval) * range);
                } else
                    Value = (int)(left + InterpolationFunction(interval) * range);
                
            } else
            {
                if (Loop == LoopMode.LoopWithReversing && interpolatorOrientation != direction)
                {
                    Value = (int)(left + InterpolationFunction(1 - interval) * range);
                } else
                    Value = (int)(right - InterpolationFunction(interval) * range);
            }
        }
    }
}