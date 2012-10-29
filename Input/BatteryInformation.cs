
using System.Runtime.InteropServices;

namespace Blueberry.XInput
{
    /// <summary>
    /// Contains information on battery type and charge state.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct BatteryInformation
    {
        public static readonly BatteryInformation Empty = new BatteryInformation() { BatteryLevel = BatteryLevel.Empty, BatteryType = BatteryType.Disconnected };
        /// <summary>
        /// <dd>The type of battery.  <em>BatteryType</em> will be one of the following values. <table><tr><th>Value</th><th>Description</th></tr><tr><td><see cref="F:SharpDX.XInput.BatteryType.Disconnected"/></td><td>The device is not connected.?</td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryType.Wired"/></td><td>The device is a wired device and does not have a battery.?</td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryType.Alkaline"/></td><td>The device has an alkaline battery.?</td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryType.Nimh"/></td><td>The device has a nickel metal hydride battery.?</td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryType.Unknown"/></td><td>The device has an unknown  battery type.?</td></tr></table></dd>
        /// </summary>
        public BatteryType BatteryType;
        /// <summary>
        /// <dd>The charge state of the battery.  This value is only valid for wireless devices with a known battery type.   <em>BatteryLevel</em> will be one of the following values. <table><tr><th>Value</th></tr><tr><td><see cref="F:SharpDX.XInput.BatteryLevel.Empty"/></td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryLevel.Low"/></td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryLevel.Medium"/></td></tr><tr><td><see cref="F:SharpDX.XInput.BatteryLevel.Full"/></td></tr></table></dd>
        /// </summary>
        public BatteryLevel BatteryLevel;
    }
}
