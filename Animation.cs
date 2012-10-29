using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public abstract class Animation<T> : IAnimation
    {
        public virtual T From { get; protected set; }
        public virtual T To { get; protected set; }
        public virtual T Value { get; protected set; }
        public bool Loop { get; set; }
        public bool Playing { get; private set; }
        public bool Paused { get; private set; }
        public bool Stoped { get; private set; }
        public virtual double Period { get; protected set; }

        public Animation()
        {
            Stoped = true;
            //AnimationManager.Manager.Animations.Add(this);
        }

        public virtual void Animate(float dt)
        {
           
        }

        public virtual void Play(bool restart = false)
        {
            if (restart)
            {
                Value = From;
                Playing = true;
                Paused = false;
                Stoped = false;
                AnimationManager.Manager.Animations.Add(this);
            }
            else
            {
                if (Paused)
                    Resume();
                else if (Stoped)
                {
                    Value = From;
                    Playing = true;
                    Paused = false;
                    Stoped = false;
                    AnimationManager.Manager.Animations.Add(this);
                }
            }
        }

        public virtual void Pause()
        {
            if (Playing)
            {
                Playing = false;
                Paused = true;
                Stoped = false;
                AnimationManager.Manager.Animations.Remove(this);
            }
        }

        public virtual void Stop()
        {
            if (!Stoped)
            {
                Stoped = true;
                Playing = false;
                Paused = false;
                AnimationManager.Manager.Animations.Remove(this);
            }
        }

        public virtual void Resume()
        {
            if (Paused)
            {
                Paused = false;
                Stoped = false;
                Playing = true;

                AnimationManager.Manager.Animations.Add(this);
            }

        }

        public virtual void Dispose()
        {
        }
    }
}
