using System;

namespace Blueberry.Audio
{
    public class AudioRemoteControll
    {
        private AudioChannel channel;
        public bool Connected
        {
            get{ return channel != null;}
        }
        public float Volume
        {
            get{ return  channel != null ? channel.Volume : 0;}
            set
            { 
                if (channel != null)
                    channel.Volume = value;
            }
        }
        public float LowPassHFGain
        {
            get{ return  channel != null ? channel.LowPassHFGain : 0;}
            set
            { 
                if (channel != null)
                    channel.LowPassHFGain = value;
            }
        }
        public bool IsLooped
        {
            get{return channel != null ? channel.IsLooped : false;}
            set{if(channel != null) channel.IsLooped = value;}
        }
        public bool Paused { get { return channel != null ? channel.Paused : false; } }
        public bool Playing { get { return channel != null ? channel.Playing : false;} }

        internal AudioRemoteControll(AudioChannel chanell)
        {
            this.channel = chanell;
        }
        internal void SoftBreak()
        {
            channel = null;
        }
        public void Break()
        {
            if(channel == null) return;
            lock (AudioManager.Instance.workWithListMutex)
            {
                channel.Stop();
                channel = null;
            }
        }
        public void Pause()
        {
            if(channel == null) return;
            lock (AudioManager.Instance.workWithListMutex)
            {
                channel.Pause();
            }
        }
        public void Resume()
        {
            if(channel == null) return;
            lock (AudioManager.Instance.workWithListMutex)
            {
                channel.Resume();
            }
        }
    }
}

