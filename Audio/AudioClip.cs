using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NVorbis;

namespace Blueberry.Audio
{
    /// <summary>
    /// A container for audio.  Represents a single piece of audio that can
    /// be repeatedly played.
    /// </summary>
    public class AudioClip
    {
        //VorbisFile rawClip;

        internal Stream underlyingStream;

        public AudioChannel StaticChanel { get; private set; }
        /// <summary>
        /// Constructs an audio clip from the given file.
        /// </summary>
        /// <param name="fileName">The file which to read from.</param>
        public AudioClip(string fileName)
        {
            underlyingStream = File.OpenRead(fileName);
            StaticChanel = new AudioChannel(AudioManager.Instance.BuffersPerChannel, AudioManager.Instance.BytesPerBuffer);
            StaticChanel.Init(this);
            //StaticChanel.Prepare();
            AudioManager.Instance.AddClip(this);
            //rawClip = new VorbisFile(fileName);

            Cache(AudioManager.Instance.BytesPerBuffer * AudioManager.Instance.BuffersPerChannel);
        }

        /// <summary>
        /// Reads an audio clip from the given stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from.</param>
        public AudioClip(Stream inputStream)
        {
            underlyingStream = inputStream;
            StaticChanel = new AudioChannel(AudioManager.Instance.BuffersPerChannel, AudioManager.Instance.BytesPerBuffer);
            StaticChanel.Init(this);
            //StaticChanel.Prepare();
            AudioManager.Instance.AddClip(this);
            //rawClip = new VorbisFile(inputStream);
            Cache(AudioManager.Instance.BytesPerBuffer * AudioManager.Instance.BuffersPerChannel);
        }

        /// <summary>
        /// Caches the given number of bytes by reading them in and discarding
        /// them.  This is useful so that when the sound if first played,
        /// there's not a delay.
        /// </summary>
        /// <param name="bytes">Then number of PCM bytes to read.</param>
        
        protected void Cache(int bytes)
        {
        	underlyingStream.Seek(0, SeekOrigin.Begin);
            VorbisReader reader = new VorbisReader(underlyingStream, false);
            
            int totalBytes = 0;
            float[] buffer = new float[4096];

            while (totalBytes < bytes)
            {
            	int bytesRead = reader.ReadSamples(buffer, 0, buffer.Length);

                if (bytesRead <= 0)
                    break;
                totalBytes += bytesRead;
            }
            reader.Dispose();
        }
        
        /// <summary>
        /// Plays the audio clip.
        /// </summary>
        public AudioRemoteControll PlayDynamic()
        {
            return AudioManager.Instance.PlayClip(this);
        }
        public void Play()
        {
            lock (AudioManager.Instance.workWithListMutex)
            {
                StaticChanel.Play();
            }
        }
        public void Pause()
        {
            lock (AudioManager.Instance.workWithListMutex)
            {
                StaticChanel.Pause();
            }
        }
        public void Stop()
        {
            lock (AudioManager.Instance.workWithListMutex)
            {
                StaticChanel.Stop();
            }
        }
        public void Resume()
        {
            lock (AudioManager.Instance.workWithListMutex)
            {
                StaticChanel.Resume();
            }
        }

    }
}
