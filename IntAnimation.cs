using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public class IntAnimation : Animation<int>
    {
        protected double temp; // время, прошедшее с последнего

        public IntAnimation(int from, int to, double period, bool loop)
        {
            From = from;
            To = to;
            Period = period;
            Loop = loop;
            Value = From;
        }
        public IntAnimation()
        {
            From = 0;
            To = 0;
            Period = 0;
            Loop = false;
            Value = 0;
        }
        public void Reset()
        {
            Value = From;
            temp = 0;
        }

        public static explicit operator int(IntAnimation anim)
        {
            return anim.Value;
        }

        public override void Animate(float dtime)
        {
            temp += dtime;
            int v = (int)(temp / Period);
            if (v > 0)
            {
                if (From < To)
                    Value += v;
                else
                    Value -= v;

                temp = temp % Period;
            }
            if ((From < To && Value > To) || (From > To && Value < To))
            {
                if (Loop) Value = From;
                else { Value = To; Stop(); }
            }
        }
        public override void Play(bool restart = false)
        {
            base.Play(restart);
            if (restart)
                temp = 0;
        }
        public override void Stop()
        {
            base.Stop();
            temp = 0;
        }
    }
}