using System.Runtime.InteropServices;
using Blueberry.Input;

namespace Blueberry.XInput
{
    /// <summary>
    /// Describes the current state of the Xbox 360 Controller.
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct Gamepad
    {
        /// <summary>
        /// Constant TriggerThreshold.
        /// </summary>
        public const byte TriggerThreshold = (byte)30;
        /// <summary>
        /// Constant LeftThumbDeadZone.
        /// </summary>
        public const short LeftThumbDeadZone = (short)7849;
        /// <summary>
        /// Constant RightThumbDeadZone.
        /// </summary>
        public const short RightThumbDeadZone = (short)8689;
        /// <summary>
        /// <dd>Bitmask of the device digital buttons, as follows. A set bit indicates that the corresponding button is pressed.
        ///             <pre>#define <see cref="F:SharpDX.XInput.GamepadButtonFlags.DPadUp"/>          0x00000001
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.DPadDown"/>        0x00000002
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.DPadLeft"/>        0x00000004
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.DPadRight"/>       0x00000008
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.Start"/>            0x00000010
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.Back"/>             0x00000020
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.LeftThumb"/>       0x00000040
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.RightThumb"/>      0x00000080
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.LeftShoulder"/>    0x0100
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.RightShoulder"/>   0x0200
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.A"/>                0x1000
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.B"/>                0x2000
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.X"/>                0x4000
        ///             #define <see cref="F:SharpDX.XInput.GamepadButtonFlags.Y"/>                0x8000</pre><p>Bits that are set but not defined above are reserved, and their state is undefined.</p></dd>
        /// </summary>
        public GamepadButtonFlags Buttons;
        /// <summary>
        /// <dd>The current value of the left trigger analog control. The value is between 0 and 255. </dd>
        /// </summary>
        public byte LeftTrigger;
        /// <summary>
        /// <dd>The current value of the right trigger analog control. The value is between 0 and 255. </dd>
        /// </summary>
        public byte RightTrigger;
        /// <summary>
        /// <dd>Left thumbstick x-axis value. Each of the thumbstick axis members is a signed value between -32768 and 32767 describing the position of the thumbstick. A value of 0 is centered. Negative values signify down or to the left. Positive values signify up or to the right. The constants <see cref="F:SharpDX.XInput.Gamepad.LeftThumbDeadZone"/> or <see cref="F:SharpDX.XInput.Gamepad.RightThumbDeadZone"/> can be used as a positive and negative value to filter a thumbstick input. </dd>
        /// </summary>
        public short LeftThumbX;
        /// <summary>
        /// <dd>Left thumbstick y-axis value. The value is between -32768 and 32767. </dd>
        /// </summary>
        public short LeftThumbY;
        /// <summary>
        /// <dd>Right thumbstick x-axis value. The value is between -32768 and 32767. </dd>
        /// </summary>
        public short RightThumbX;
        /// <summary>
        /// <dd>Right thumbstick y-axis value. The value is between -32768 and 32767. </dd>
        /// </summary>
        public short RightThumbY;

        public override string ToString()
        {
            return string.Format("Buttons: {0}, LeftTrigger: {1}, RightTrigger: {2}, LeftThumbX: {3}, LeftThumbY: {4}, RightThumbX: {5}, RightThumbY: {6}", (object)this.Buttons, (object)this.LeftTrigger, (object)this.RightTrigger, (object)this.LeftThumbX, (object)this.LeftThumbY, (object)this.RightThumbX, (object)this.RightThumbY);
        }
    }
}
