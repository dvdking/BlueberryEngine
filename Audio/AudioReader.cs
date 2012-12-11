using System;
using System.IO;

namespace Blueberry
{
    public interface IAudioReader:IDisposable
    {
        int Channels{ get; }
        int SampleRate { get; }
        long Position {get; set; }
        int ReadSamples(short[] buffer, int offset, int length);
    }
}

