namespace Blueberry.XInput
{
    /// <summary>
    /// Common error code from XInput
    /// </summary>
    internal sealed class ResultCode
    {
        /// <summary>
        /// Device is not connected
        /// </summary>
        public static readonly Result NotConnected = ErrorCodeHelper.ToResult(1167);

        static ResultCode()
        {
        }
    }
}
