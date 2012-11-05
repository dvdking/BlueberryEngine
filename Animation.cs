using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public enum LoopMode : byte
    {
        None = 0,
        Loop = 1,
        LoopWithReversing = 2
    }
    public enum PlaybackState
    {
        Play,
        Pause,
        Stop
    }
    public abstract class Animation<T> : IAnimation
    {
        public virtual T From { get; protected set; }
        public virtual T To { get; protected set; }
        public virtual T Value { get; protected set; }
        public LoopMode Loop { get; set; }
        public PlaybackState State { get; private set; }
        public virtual double Period { get; protected set; }

        protected double timer; // main timer

        public Animation()
        {
            State = PlaybackState.Stop;
        }

        public virtual void Animate(double dt)
        {
           
        }

        public virtual void Play(bool restart = false)
        {
            lock (AnimationManager.Manager.updateMutex)
            {
                if (restart)
                {
                    Value = From;
                    State = PlaybackState.Play;
                    AnimationManager.Manager.animations.Add(this);
                }
                else
                {
                    if (State == PlaybackState.Pause)
                        Resume();
                    else if (State == PlaybackState.Stop)
                    {
                        Value = From;
                        State = PlaybackState.Play;
                        AnimationManager.Manager.animations.Add(this);
                    }
                }
            }
        }

        public virtual void Pause()
        {
            lock (AnimationManager.Manager.updateMutex)
            {
                if (State == PlaybackState.Play)
                {
                    State = PlaybackState.Pause;
                    AnimationManager.Manager.animations.Remove(this);
                }
            }
        }

        public virtual void Stop()
        {
            lock (AnimationManager.Manager.updateMutex)
            {
                if (State != PlaybackState.Stop)
                {
                    State = PlaybackState.Stop;
                    AnimationManager.Manager.animations.Remove(this);
                }
            }
        }

        public virtual void Resume()
        {
            lock (AnimationManager.Manager.updateMutex)
            {
                if (State == PlaybackState.Pause)
                {
                    State = PlaybackState.Play;
                    AnimationManager.Manager.animations.Add(this);
                }
            }

        }

        public virtual void Dispose()
        {
            lock (AnimationManager.Manager.updateMutex)
            {
                AnimationManager.Manager.animations.Remove(this);
            }
        }
    }
}
