using System.Runtime.InteropServices;
using Blueberry.Input;

namespace Blueberry.XInput
{
    /// <summary>
    /// Specifies keystroke data returned byXInputGetKeystroke.
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct Keystroke
    {
        public GamepadKeyCode VirtualKey;
        public char Unicode;
        public KeyStrokeFlags Flags;
        public UserIndex UserIndex;
        public byte HidCode;
    }
}
