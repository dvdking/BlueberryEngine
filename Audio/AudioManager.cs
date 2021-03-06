﻿using System;
using System.Collections.Generic;
using System.Threading;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio;


namespace Blueberry.Audio
{
    /// <summary>
    /// A manager for audio clips and audio channels.  Puts audio clips into
    /// empty channels when they're played.
    /// </summary>
    public class AudioManager : IDisposable
    {
        public int ChannelCount { get; private set; }
        public int BuffersPerChannel { get; private set; }
        public int BytesPerBuffer { get; private set; }

        public AudioChannel[] Channels { get; private set; }

        private List<AudioClip> StaticClips { get; set; }

        public Thread UpdateThread { get; private set; }

        public bool RunUpdates { get; set; }

        private static AudioManager instance = null;

        internal object workWithListMutex = new object();

        internal readonly EffectsExtension Efx;
        internal readonly XRamExtension XRam;

        /// <summary>
        /// The sole instance of the audio manager.
        /// </summary>
        public static AudioManager Instance
        {
            get
            {
                if(instance == null)
                    instance = new AudioManager();

                return instance;
            }

            private set
            {
                instance = value;
            }
        }
		internal static bool HasInstance {get{return instance != null;}}
		
        /// <summary>
        /// Disposes of all resources used by the audio manager.
        /// </summary>
        ~AudioManager()
        {
            Dispose();
        }

        /// <summary>
        /// Constructs a default audio manager with 16 channels.
        /// </summary>
        public AudioManager():this(16,4*8,4096, true)
        { 
        }

        /// <summary>
        /// Initializes the audio manager.
        /// </summary>
        /// <param name="channels">The number of channels to use.</param>
        /// <param name="buffersPerChannel">The number of buffers each channel will contain.</param>
        /// <param name="bytesPerBuffer">The number of bytes in each buffer.</param>
        /// <param name="launchThread">If true, a separate thread will be launched to handle updating the sound manager.  
        ///                            Otherwise, a thread will not be launched and manual calls to Update() will be required.</param>
        public AudioManager(int channels, int buffersPerChannel, int bytesPerBuffer, bool launchThread)
        {
            if (instance != null) throw new Exception("Can't create more than one instances of AudioManager");

            if(AudioContext.CurrentContext == null)
                new AudioContext();
            Efx = new EffectsExtension();
            XRam = new XRamExtension();

            Init(channels, buffersPerChannel, bytesPerBuffer, launchThread);
        }

        /// <summary>
        /// Initializes the audio manager.
        /// </summary>
        /// <param name="channels">The number of channels to use.</param>
        /// <param name="buffersPerChannel">The number of buffers each channel will contain.</param>
        /// <param name="bytesPerBuffer">The number of bytes in each buffer.</param>
        private void Init(int channels, int buffersPerChannel, int bytesPerBuffer, bool launchThread)
        {
            BytesPerBuffer = bytesPerBuffer;
            BuffersPerChannel = buffersPerChannel;
            ChannelCount = channels;

            RunUpdates = launchThread;
            ChannelCount = channels;
            Channels = new AudioChannel[channels];
            StaticClips = new List<AudioClip>();
            Instance = this;

            for (int i = 0; i < channels; i++)
                Channels[i] = new AudioChannel(buffersPerChannel, bytesPerBuffer);

            if(launchThread)
            {
                UpdateThread = new Thread(RunUpdateLoop);
                UpdateThread.Name = "Audio thread";
                UpdateThread.IsBackground = true;
                UpdateThread.Start();
            }
            else
            {
                UpdateThread = null;
            }
        }

        /// <summary>
        /// Plays the audio clip on the first free channel.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        public AudioRemoteControll PlayClip(AudioClip clip)
        {
            // TODO: If all channels are busy, the clip will be ignored.  There must be a more elegant way.
            lock (workWithListMutex)
            {
                foreach (AudioChannel channel in Channels)
                {

                    if (channel.IsFree)
                    {
                        channel.Init(clip);
                        channel.Play();
                        return channel.CreateRemote();
                    }
                }
                return null; // All channels are busy
            }
        }

        /// <summary>
        /// Performs a single update by updating all channels.
        /// </summary>
        public void Update()
        {
            lock (workWithListMutex)
            {

                foreach (AudioChannel channel in Channels)
                {
                    channel.Update();
                }

                foreach (var item in StaticClips)
                {
                    item.StaticChanel.Update();
                }

            }
        }

        internal void AddClip(AudioClip clip)
        {
            lock (workWithListMutex)
            {
                StaticClips.Add(clip);
            }
        }

        /// <summary>
        /// Continuously updates the audio manager.  This method will not return
        /// unless it's interrupted, so it's best to run it in a separate 
        /// thread.
        /// </summary>
        internal void RunUpdateLoop()
        {
            while (RunUpdates)
            {
                Update();
                // TODO: Is 1ms long enough to still have good performance outside
                // of the audio?
                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Dispose of the audio manager and frees its audio memory.
        /// </summary>
        public void Dispose()
        {
            RunUpdates = false;
            UpdateThread.Join();

            foreach (AudioChannel channel in Channels)
                channel.Dispose();
            
            foreach (var item in StaticClips)
                item.StaticChanel.Dispose();
            instance = null;
        }
    }
}
