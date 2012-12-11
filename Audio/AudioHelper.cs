using System;
using System.IO;

namespace Blueberry.Audio
{
    internal static class AudioHelper
    {
        public static IAudioReader GetReader(Stream stream, AudioFormat format)
        {
            switch (format)
            {
                case AudioFormat.OGG:
#if OGG
                    return new OggAudioReader(stream, false);
#else
                    throw new Exception("OGG support disabled");
#endif
                case AudioFormat.WAV:
#if WAV
					return new WavAudioReader(stream, false);
#else
                    throw new Exception("WAV support disabled");
#endif
                case AudioFormat.Unknown:
                    throw new Exception("Can't get reader of unknown format");
            }
            throw new Exception("WTF");
        }
    }
}

