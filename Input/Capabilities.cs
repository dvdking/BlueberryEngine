using System.Runtime.InteropServices;

namespace Blueberry.XInput
{
    /// <summary>
    /// Describes the capabilities of a connected controller.  TheXInputGetCapabilitiesfunction returnsXINPUT_CAPABILITIES.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct Capabilities
    {
        public static readonly Capabilities Empty = new Capabilities();
        public DeviceType Type;
        public DeviceSubType SubType;
        public CapabilityFlags Flags;
        public Gamepad Gamepad;
        public Vibration Vibration;
    }
}
