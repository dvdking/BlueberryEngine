namespace Blueberry.XInput
{
    internal class ErrorCodeHelper
    {
        /// <summary>
        /// Converts a win32 error code to a Result.
        /// 
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <returns>
        /// A HRESULT code
        /// </returns>
        public static Result ToResult(int errorCode)
        {
            return new Result(errorCode <= 0 ? (uint)errorCode : (uint)(errorCode & (int)ushort.MaxValue | -2147024896));
        }
    }
}
