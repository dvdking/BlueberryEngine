namespace Blueberry.XInput
{
    /// <summary>
    /// Represents the state of a controller.
    /// </summary>
    internal struct State
    {
        /// <summary>
        /// <dd>State packet number. The packet number indicates whether there have been any changes in the state of the controller. If the <strong>dwPacketNumber</strong> member is the same in sequentially returned <see cref="T:Blueberry.XInput.State"/> structures, the controller state has not changed. </dd>
        /// </summary>
        public int PacketNumber;
        /// <summary>
        /// <dd><see cref="T:Blueberry.XInput.Gamepad"/> structure containing the current state of an Xbox 360 Controller. </dd>
        /// </summary>
        public Gamepad Gamepad;
    }
}
