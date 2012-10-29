using System.Runtime.InteropServices;

namespace Blueberry.XInput
{
    /// <summary>
    /// Specifies motor speed levels for the vibration function of a controller.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct Vibration
    {
        /// <summary>
        /// <dd>Speed of the left motor. Valid values are in the range 0 to 65,535. Zero signifies no motor use; 65,535 signifies 100 percent motor use. </dd>
        /// </summary>
        public ushort LeftMotorSpeed;
        /// <summary>
        /// <dd>Speed of the right motor. Valid values are in the range 0 to 65,535. Zero signifies no motor use; 65,535 signifies 100 percent motor use. </dd>
        /// </summary>
        public ushort RightMotorSpeed;
    }
}
