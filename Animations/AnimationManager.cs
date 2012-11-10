using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Blueberry.Animations
{
    class AnimationManager
    {
        private static AnimationManager instance = null;
        public Thread UpdateThread { get; private set; }

        public bool RunUpdates { get; set; }
        internal List<IAnimation> animations;

        internal object updateMutex = new object();

        public static AnimationManager Manager
        {
            get
            {
                if (instance == null)
                    instance = new AnimationManager();

                return instance;
            }
            private set
            {
                instance = value;
            }
        }

        public AnimationManager(bool launchUpdateThread = true)
        {
            if (instance != null) throw new Exception("Can't create more than one instances of AnimationManager");
            Init(launchUpdateThread);
        }

        private void Init(bool launchThread)
        {
            animations = new List<IAnimation>();
            RunUpdates = launchThread;

            instance = this;

            if (launchThread)
            {
                stopwatch = new Stopwatch();
                UpdateThread = new Thread(RunUpdateLoop);
                UpdateThread.Name = "Animation thread";
                UpdateThread.IsBackground = true;
                UpdateThread.Start();
            }
            else
            {
                UpdateThread = null;
            }
        }


        public void Update(double dt)
        {
            lock (updateMutex)
            {
                for (int i = 0; i < animations.Count; i++)
                {
                    animations[i].Animate(dt);
                }
            }
        }
        Stopwatch stopwatch;
        double time;
        internal void RunUpdateLoop()
        {
            while (RunUpdates)
            {
                time = stopwatch.Elapsed.TotalSeconds;
                stopwatch.Restart();
                Update(time);
                Thread.Sleep(5);
            }
        }

        public void Dispose()
        {
            RunUpdates = false;
            UpdateThread.Join();

            foreach (IAnimation anim in animations)
            {
                anim.Dispose();
            }
        }
    }
}
