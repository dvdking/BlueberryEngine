using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry
{
    public interface IAnimation : IDisposable
    {

        void Animate(float dt);
        void Play(bool restart = false);
        void Pause();
        void Stop();
        void Resume();
        bool Playing { get; }
        bool Stoped { get; }
        bool Paused { get; }
    }
}
