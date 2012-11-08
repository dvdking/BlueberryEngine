using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public class FloatAnimation : Animation<float>
    {
        float left, right; // upper and lower bounds
        float range;

        public FloatAnimation(float from, float to, float period, LoopMode loopMode, Interpolator interpolator):
            base(interpolator)
        {
            From = from;
            To = to;
            left = Math.Min(from, to);
            right = Math.Max(from, to);
            // now we know, in what direction animation moves and first interpolation orientation
            interpolatorOrientation = direction = From < To;

            range = right - left;

            Period = period;
            Loop = loopMode;
            Value = From;
        }

        public FloatAnimation(float from, float to, float period, LoopMode loopMode)
            :this(from,to,period,loopMode, v=>v)
        {
        }
        public FloatAnimation(float from, float to, float period)
            :this(from,to,period, LoopMode.Loop, v=>v)
        {
        }

        public FloatAnimation():this(0,1,0,Blueberry.LoopMode.None, v=>v)
        {
        }

        public void Reset()
        {
            Value = From;
            timer = 0;
        }

        public static explicit operator float(FloatAnimation anim)
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
                    Value = right - InterpolationFunction(1 - interval) * range;
                } else
                    Value = left + InterpolationFunction(interval) * range;

            } else
            {
                if (Loop == LoopMode.LoopWithReversing && interpolatorOrientation != direction)
                {
                    Value = left + InterpolationFunction(1 - interval) * range;
                } else
                    Value = right - InterpolationFunction(interval) * range;
            }
            base.RaiseAnimateEvent();
        }
    }
}