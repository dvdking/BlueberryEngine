
using System;
using System.IO;

namespace Blueberry.Audio
{
	/// <summary>
	/// Description of WavAudioReader.
	/// </summary>
	public class WavAudioReader:IAudioReader
	{
		
		public int Channels { get; private set; }
		
		public int SampleRate { get; private set; }
		
		private bool closeStreamOnDispose;
		private int _headerSize;
		private int _bitsPerSample;
		private BinaryReader _reader;
		private long lastPos;
		
		public WavAudioReader(Stream stream, bool closeStreamOnDispose)
		{
			if (stream == null)
                throw new ArgumentNullException("stream");
			this.closeStreamOnDispose = closeStreamOnDispose;
			_reader = new BinaryReader(stream);
			ReadHeader();
		}
		private void ReadHeader()
		{
            // RIFF header
            string signature = new string(_reader.ReadChars(4));
            if (signature != "RIFF")
                throw new NotSupportedException("Specified stream is not a wave file.");

            int riff_chunck_size = _reader.ReadInt32();
            string format = new string(_reader.ReadChars(4));
            if (format != "WAVE")
                throw new NotSupportedException("Specified stream is not a wave file.");

            // WAVE header
            string format_signature = new string(_reader.ReadChars(4));
            if (format_signature != "fmt ")
                throw new NotSupportedException("Specified wave file is not supported.");

            int format_chunk_size = _reader.ReadInt32();
            int audio_format = _reader.ReadInt16();
            int num_channels = _reader.ReadInt16();
            int sample_rate = _reader.ReadInt32();
            int byte_rate = _reader.ReadInt32();
            int block_align = _reader.ReadInt16();
            _bitsPerSample = _reader.ReadInt16();

            
            string next_signature = new string(_reader.ReadChars(4));
            int signature_size = _reader.ReadInt32();
            string signature_data = "";
            try
            {
	            while(next_signature != "data")
	            {
	            	signature_data = new string(_reader.ReadChars(signature_size));
	            	next_signature = new string(_reader.ReadChars(4));
	            	signature_size = _reader.ReadInt32();
	            }
            }
            catch(EndOfStreamException)
            {
            	throw new NotSupportedException("Specified wave file is not supported.");
            }

            int data_chunk_size = signature_size;

            Channels = num_channels;
            SampleRate = sample_rate;
            _headerSize = (int)_reader.BaseStream.Position;
            lastPos = _reader.BaseStream.Position;
		}
		
		public int ReadSamples(short[] buffer, int offset, int length)
		{
			_reader.BaseStream.Position = lastPos;
			int i = 0;
			try
			{
				for (i = 0; i < length; i++) 
				{
					buffer[offset + i] = BitConverter.ToInt16(_reader.ReadBytes(2),0);
				}
			}
			catch(EndOfStreamException)
			{}
			catch(ArgumentOutOfRangeException)
			{}
			lastPos = _reader.BaseStream.Position;
			return i;
		}
		
		public void Dispose()
		{
			
			if(closeStreamOnDispose)
			{
				_reader.Dispose();
			}
		}
		
		public long Position {
			get { return _reader.BaseStream.Position - _headerSize; }
			set 
			{
				_reader.BaseStream.Position = value + _headerSize;
				lastPos = value;
			
			}
		}
	}
}
