using System;
using OpenTK.Audio.OpenAL;
using System.IO;

namespace Blueberry.Audio
{
    /// <summary>
    /// An object for buffering and playing data on a single channel. 
    /// </summary>
    public class AudioChannel : IDisposable
    {
        public short[] SampleBuffer { get; private set; }
        private int[] alBufferIds;
        public int[] Buffers
        {
            get{ return alBufferIds;}
        }
        public int BufferCount { get; private set; }
        public int BufferSize { get; private set; }
        private int alSourceId;
        public int Source { get { return alSourceId; } }
        private int alFilterId;
        public IAudioReader Reader { get; private set; }
        public AudioClip CurrentClip { get; private set; }

        public ALFormat CurrentFormat { get; private set; }
        public int CurrentRate { get; private set; }

        private bool eof;

        public bool IsFree
        {
            get
            {
                return Reader == null || Stoped;
            }
        }

        public bool Paused { get { ALSourceState st = AL.GetSourceState(Source); return st == ALSourceState.Paused; } }
        public bool Stoped { get { ALSourceState st = AL.GetSourceState(Source); return st == ALSourceState.Stopped; } }
        public bool Playing { get { ALSourceState st = AL.GetSourceState(Source); return st == ALSourceState.Playing; } }

        float lowPassHfGain;
        public float LowPassHFGain
        {
            get { return lowPassHfGain; }
            set
            {
                if (AudioManager.Instance.Efx.IsInitialized)
                {
                    lowPassHfGain = MathUtils.Clamp(value, 0, 1);
                    AudioManager.Instance.Efx.Filter(alFilterId, EfxFilterf.LowpassGainHF, lowPassHfGain);
                    AudioManager.Instance.Efx.BindFilterToSource(Source, alFilterId);
                    CheckErrors();
                }
            }
        }
        
        float volume;
        public float Volume
        {
            get { return volume; }
            set
            {
                volume = MathUtils.Clamp(value, 0, 1);
                AL.Source(alSourceId, ALSourcef.Gain, volume);
                CheckErrors();
            }
        }
        
        public bool IsLooped { get; set; }


        /// <summary>
        /// Constructs an empty channel, ready to play sound.
        /// </summary>
        /// <param name="bufferCount">The number of audio buffers to size.</param>
        /// <param name="bufferSize">The size, in bytes, of each audio buffer.</param>
        internal AudioChannel(int bufferCount, int bufferSize)
        {
            alBufferIds = new int[bufferCount];
            BufferCount = bufferCount;
            BufferSize = bufferSize;
            Reader = null;
            CurrentClip = null;

            SampleBuffer = new short[bufferSize];

            // Make the source
            alSourceId = AL.GenSource();
            
            // Make the buffers
            alBufferIds = AL.GenBuffers(bufferCount);

            if (AudioManager.Instance.XRam.IsInitialized)
            {
                AudioManager.Instance.XRam.SetBufferMode(BufferCount, ref alBufferIds[0], XRamExtension.XRamStorage.Hardware);
                CheckErrors();
            }
            
            Volume = 1;
            
            if (AudioManager.Instance.Efx.IsInitialized)
            {
                alFilterId = AudioManager.Instance.Efx.GenFilter();
                AudioManager.Instance.Efx.Filter(alFilterId, EfxFilteri.FilterType, (int)EfxFilterType.Lowpass);
                AudioManager.Instance.Efx.Filter(alFilterId, EfxFilterf.LowpassGain, 1);
                LowPassHFGain = 1;
            }
            IsLooped = false;
        }

        /// <summary>Deconstructs the channel, freeing its hardware resources.</summary>
        ~AudioChannel()
        {
            Dispose(); 
        }

        /// <summary>
        /// Disposes the channel, freeing its hardware resources.
        /// </summary>
        public void Dispose()
        {
            AL.SourceStop(Source);
            if(Buffers != null)
                AL.DeleteBuffers(Buffers);
            AL.DeleteSource(Source);
            alBufferIds = null;
            CloseReader();
        }
        private AudioRemoteControll currentRemote;
        internal AudioRemoteControll CreateRemote()
        {
            if(currentRemote != null)
                currentRemote.SoftBreak();
            currentRemote = new AudioRemoteControll(this);
            return currentRemote;
        }
        public void Init(AudioClip clip)
        {
            ALSourceState state = AL.GetSourceState(Source);
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
            {
                Stop();
            }
            if (clip != CurrentClip)
            {
                CloseReader();
                CurrentClip = clip;
                OpenReader();

                CurrentFormat = Reader.Channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;
                CurrentRate = Reader.SampleRate;
            }
            Volume = 1;
            LowPassHFGain = 1;
            IsLooped = false;
        }

        private object updateMutex = new object();
        private object readerMutex = new object();

        public static void CheckErrors()
        {
            ALError error;
            if ((error = AL.GetError()) != ALError.NoError)
                throw new InvalidOperationException(AL.GetErrorString(error));

        }
        void Empty()
        {
            AL.SourceStop(Source);
            int queued;
            AL.GetSource(Source, ALGetSourcei.BuffersQueued, out queued);
            try
            {
                while (queued > 0)
                {
                    int[] buffer = new int[1];
                    AL.SourceUnqueueBuffers(Source, 1, buffer);
                    CheckErrors();
                    queued--;
                }
            }
            catch (InvalidOperationException)
            {
                // This is a bug in the OpenAL implementation
                // Salvage what we can
                int processed;
                AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out processed);
                var salvaged = new int[processed];
                if (processed > 0)
                {
                    AL.SourceUnqueueBuffers(Source, processed, salvaged);
                    CheckErrors();
                }

                // Try turning it off again?
                AL.SourceStop(Source);
                CheckErrors();

                Empty();
            }
        }
        /// <summary>
        /// Begins playing the given clip.
        /// </summary>
        public void Play()
        {
            var state = AL.GetSourceState(Source);

            switch (state)
            {
                case ALSourceState.Playing:
                    return;
                case ALSourceState.Paused:
                    Resume();
                    return;
                case ALSourceState.Stopped:
                    Empty();
                    break;
            }

            Prepare();

            // Start playing the clip
            AL.SourcePlay(Source);
        }

        internal void OpenReader()
        {
            lock (readerMutex)
            {
                CurrentClip.underlyingStream.Seek(0, SeekOrigin.Begin);
                Reader = AudioHelper.GetReader(CurrentClip.underlyingStream, CurrentClip.Format);
            }
        }

        internal void Prepare()
        {
            ALSourceState state = AL.GetSourceState(Source);
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
                Stop();
            lock (readerMutex)
            {
                Reader.Position = 0; //CurrentTime = TimeSpan.Zero;
            }
            eof = false;

            DequeuUsedBuffers();
            // Buffer initial audio
            int usedBuffers = 0;
            for (int i = 0; i < BufferCount; i++)
            {
                lock (readerMutex)
                {
                    int samples = Reader.ReadSamples(SampleBuffer, 0, SampleBuffer.Length);
                    if (samples > 0)
                    {
                        // Buffer the segment
                        AL.BufferData(Buffers[i], CurrentFormat, SampleBuffer, samples * sizeof(short), CurrentRate);

                        usedBuffers++;
                    }
                    else if (samples == 0)
                    {
                        // Clip is too small to fill the initial buffer, so stop buffering.
                        break;
                    }
                    else
                    {
                        throw new System.IO.IOException("Error reading or processing audio file");
                    }
                }
            }
            AL.SourceQueueBuffers(Source, usedBuffers, Buffers);
        }

        public void Pause()
        {
            if (AL.GetSourceState(Source) != ALSourceState.Playing)
                return;
            AL.SourcePause(Source);
        }

        public void Resume()
        {
            if (AL.GetSourceState(Source) != ALSourceState.Paused)
                return;
            AL.SourcePlay(Source);
        }

        public void Stop()
        {
            var state = AL.GetSourceState(Source);
            if (state == ALSourceState.Playing || state == ALSourceState.Paused)
            {
                lock (readerMutex)
                {
                    AL.SourceStop(Source);
                    Reader.Position = 0;
                }
            }
            if(currentRemote != null) currentRemote.SoftBreak();
        }

        internal void CloseReader()
        {
            if (Reader != null)
            {
                lock (readerMutex)
                {
                    Reader.Dispose();
                    Reader = null;
                }
            }
            
        }

        /// <summary>
        /// Removes all empty buffers from the audio queue.
        /// </summary>
        protected void DequeuUsedBuffers()
        {
            int processedBuffers;
            AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out processedBuffers);

            int[] removedBuffers = new int[processedBuffers];
            AL.SourceUnqueueBuffers(Source, processedBuffers, removedBuffers);
        }

        /// <summary>
        /// Updates the channel, buffer addition audio if needed.  This method
        /// needs to be called frequently to maintain real-time performance.
        /// </summary>
        public void Update()
        {
            if (!Playing) return; 
            
            if (Reader != null)
            {
                int buffersQueued;
                AL.GetSource(Source, ALGetSourcei.BuffersQueued, out buffersQueued);

                int processedBuffers;
                AL.GetSource(Source, ALGetSourcei.BuffersProcessed, out processedBuffers);

                if (eof)
                {
                    // Clip is done being buffered
                    if (buffersQueued <= processedBuffers)
                    {
                        // Clip has finished
                        Stop();
                        DequeuUsedBuffers();

                        return;
                    }
                }
                else
                {
                    lock (readerMutex)
                    {
                        // Still some buffering to do
                        if (buffersQueued - processedBuffers > 0 && AL.GetError() == ALError.NoError)
                        {
                            // Make sure we're playing (not sure why we would've stopped)
                            if (AL.GetSourceState(Source) != ALSourceState.Playing)
                            {
                                AL.SourcePlay(Source);
                            }
                        }

                        // Detect buffer under-runs
                        bool underRun = (processedBuffers >= BufferCount);

                        // Remove processed buffers
                        while (processedBuffers > 0)
                        {
                            int removedBuffer = 0;

                            // TODO: Can remove more than one buffer here.  Can also
                            // add the buffers back to the queue.
                            AL.SourceUnqueueBuffers(Source, 1, ref removedBuffer);

                            // Just remove the buffer and don't do anything else if
                            // we're at the end of the clip.
                            if (eof)
                            {
                                processedBuffers--;
                                continue;
                            }
                        retry:
                            // Buffer the next chunk
                            int readSamples = Reader.ReadSamples(SampleBuffer, 0, SampleBuffer.Length);
                            if (readSamples > 0)
                            {
                                // TOOD: Queue multiple buffers here
                                AL.BufferData(removedBuffer, CurrentFormat, SampleBuffer, readSamples * sizeof(short),
                                    CurrentRate);
                                AL.SourceQueueBuffer(Source, removedBuffer);
                            }
                            if (readSamples < SampleBuffer.Length)
                            {
                                // Reached the end of the file
                                if(IsLooped)
                                {
                                    Reader.Position = 0;
                                    goto retry;
                                }
                                else
                                    eof = true;
                            }
                            else
                            {
                                // A file read error has occurred, stop playing
                                Stop();
                                break;
                            }

                            // Check for OpenAL errors
                            if (AL.GetError() != ALError.NoError)
                            {
                                Stop();
                                break;
                            }

                            processedBuffers--;
                        }
                    }
                }
            }
        }
    }
}
