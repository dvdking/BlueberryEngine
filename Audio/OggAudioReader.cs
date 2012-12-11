using System;
using System.IO;
using Blueberry.Audio;

namespace Blueberry
{
	#if OGG
    public class OggAudioReader:NVorbis.VorbisReader, IAudioReader
    {
    	long lastPos;
    	Stream stream;
    	
        public OggAudioReader(Stream stream, bool closeStreamOnDispose):base(stream, closeStreamOnDispose)
        {
        	this.stream = stream;
        	tempBuffer = new float[AudioManager.Instance.BytesPerBuffer];
        	lastPos = stream.Position;
        }

        #region IAudioReader implementation

        float[] tempBuffer;
        public int ReadSamples(short[] buffer, int offset, int length)
        {
        	stream.Position = lastPos;
        	
        	int c = base.ReadSamples(tempBuffer, offset, length);
            for (int i = offset; i < buffer.Length; i++)
            {
                var temp = (int)(32767f * tempBuffer[i]);
                if (temp > short.MaxValue) temp = short.MaxValue;
                else if (temp < short.MinValue) temp = short.MinValue;
                buffer[i] = (short)temp;
            }
            lastPos = stream.Position;
            return c;
        }
        #endregion
    	
		public long Position {
        	get{return stream.Position;}
        	set{stream.Position = value; lastPos = value; if(value == 0) DecodedTime = TimeSpan.Zero; }
		}
    }
    #endif
}

