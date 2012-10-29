using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Blueberry.Input
{
    public enum UserIndex : byte
    {
        One = (byte)0,
        Two = (byte)1,
        Three = (byte)2,
        Four = (byte)3,
        Any = (byte)255,
    }

    [Flags]
    public enum GamepadButtonFlags : short
    {
        DPadUp = (short)1,
        DPadDown = (short)2,
        DPadLeft = (short)4,
        DPadRight = (short)8,
        Start = (short)16,
        Back = (short)32,
        LeftThumb = (short)64,
        RightThumb = (short)128,
        LeftShoulder = (short)256,
        RightShoulder = (short)512,
        A = (short)4096,
        B = (short)8192,
        X = (short)16384,
        Y = (short)-32768,
        None = (short)0,
    }
}

namespace Blueberry.XInput
{
    
    [Flags]
    internal enum KeyStrokeFlags : short
    {
        KeyDown = (short)1,
        KeyUp = (short)2,
        Repeat = (short)4,
        None = (short)0,
    }
    [Flags]
    internal enum GamepadKeyCode : short
    {
        A = (short)22528,
        B = (short)22529,
        X = (short)22530,
        Y = (short)22531,
        RightShoulder = (short)22532,
        LeftShoulder = (short)22533,
        LeftTrigger = (short)22534,
        RightTrigger = (short)22535,
        DPadUp = (short)22544,
        DPadDown = (short)22545,
        DPadLeft = (short)22546,
        DPadRight = (short)22547,
        Start = (short)22548,
        Back = (short)22549,
        LeftThumbPress = (short)22550,
        RightThumbPress = (short)22551,
        LeftThumbUp = (short)22560,
        LeftThumbDown = (short)22561,
        LeftThumbRight = (short)22562,
        LeftThumbLeft = (short)22563,
        RightThumbUpLeft = (short)22564,
        LeftThumbUpright = (short)22565,
        LeftThumbDownright = (short)22566,
        RightThumbDownLeft = (short)22567,
        RightThumbUp = (short)22576,
        RightThumbDown = (short)22577,
        RightThumbRight = (short)22578,
        RightThumbLeft = (short)22579,
        RightThumbUpleft = (short)22580,
        RightThumbUpRight = (short)22581,
        RightThumbDownRight = (short)22582,
        RightThumbDownleft = (short)22583,
        None = (short)0,
    }

    internal enum DeviceType : byte
    {
        Gamepad = (byte)1,
    }
    internal enum DeviceSubType : byte
    {
        Gamepad = (byte)1,
        Wheel = (byte)2,
        ArcadeStick = (byte)3,
        FlightSick = (byte)4,
        DancePad = (byte)5,
        Guitar = (byte)6,
        DrumKit = (byte)8,
    }
    internal enum DeviceQueryType
    {
        Any,
        Gamepad,
    }
    [Flags]
    internal enum CapabilityFlags : short
    {
        VoiceSupported = (short)4,
        None = (short)0,
    }
    internal enum BatteryType : byte
    {
        Disconnected = (byte)0,
        Wired = (byte)1,
        Alkaline = (byte)2,
        Nimh = (byte)3,
        Unknown = (byte)255,
    }
    internal enum BatteryLevel : byte
    {
        Empty,
        Low,
        Medium,
        Full,
    }
    internal enum BatteryDeviceType
    {
        Gamepad,
        Headset,
    }
}
