using System;
using System.IO;

namespace Blueberry.Audio
{
    public enum AudioFormat:byte
    {
        OGG = 1,
        WAV = 2, // not implemented
        Unknown = 0
    }

    /// <summary>
    /// A container for audio.  Represents a single piece of audio that can
    /// be repeatedly played.
    /// </summary>
    public class AudioClip : IDisposable
    {
        //VorbisFile rawClip;

        internal Stream underlyingStream;
        public AudioFormat Format {get; private set;}
        public AudioChannel StaticChanel { get; private set; }
        /// <summary>
        /// Constructs an audio clip from the given file.
        /// </summary>
        /// <param name="fileName">The file which to read from.</param>
        public AudioClip(string fileName)
        {
            AudioFormat f = AudioFormat.Unknown;
            if(fileName.EndsWith("ogg") || fileName.EndsWith("OGG"))
                f = AudioFormat.OGG;
            if(fileName.EndsWith("wav") || fileName.EndsWith("WAV"))
                f = AudioFormat.WAV;
            Init(File.OpenRead(fileName),f); 
        }

        /// <summary>
        /// Reads an audio clip from the given stream.
        /// </summary>
        /// <param name="inputStream">The stream to read from.</param>
        public AudioClip(Stream inputStream, AudioFormat format)
        {
            Init(inputStream, format);
        }

        private void Init(Stream stream, AudioFormat format)
        {
            if(format == AudioFormat.Unknown)
                throw new Exception("Audio format unknown");
            Format = format;
            underlyingStream = stream;
            StaticChanel = new AudioChannel(AudioManager.Instance.BuffersPerChannel, AudioManager.Instance.BytesPerBuffer);
            StaticChanel.Init(this);
            AudioManager.Instance.AddClip(this);
            if(Format == AudioFormat.OGG)
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
            IAudioReader reader = AudioHelper.GetReader(underlyingStream, Format);
            
            int totalBytes = 0;
            short[] buffer = new short[4096];

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

        #region IDisposable implementation
        public void Dispose()
        {
            underlyingStream.Close();
            underlyingStream.Dispose();
        }
        #endregion
    }
}
