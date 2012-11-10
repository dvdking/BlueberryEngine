using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.Animations
{
    public interface IAnimation : IDisposable
    {
        void Animate(double dt);
        void Play(bool restart = false);
        void Pause();
        void Stop();
        void Resume();
        PlaybackState State { get; }
    }
}
