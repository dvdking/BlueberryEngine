using System;
using System.Globalization;

namespace Blueberry.XInput
{
    /// <summary>
    /// The base class for errors that occur in XInput.
    /// 
    /// </summary>
    [Serializable]
    internal class XInputException : Exception
    {
        private ResultDescriptor descriptor;

        /// <summary>
        /// Gets the <see cref="T:Blueberry.XInput.Result">Result code</see> for the exception. This value indicates
        ///               the specific type of failure that occured within XInput.
        /// 
        /// </summary>
        public Result ResultCode
        {
            get
            {
                return this.descriptor.Result;
            }
        }

        /// <summary>
        /// Gets the <see cref="T:Blueberry.XInput.Result">Result code</see> for the exception. This value indicates
        ///               the specific type of failure that occured within XInput.
        /// 
        /// </summary>
        public ResultDescriptor Descriptor
        {
            get
            {
                return this.descriptor;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        public XInputException()
            : base("A XInput exception occurred.")
        {
            this.descriptor = ResultDescriptor.Find(Result.Fail);
            this.HResult = (int)Result.Fail;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="result">The result code that caused this exception.</param>
        public XInputException(Result result)
            : this(ResultDescriptor.Find(result))
        {
            this.HResult = (int)result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="descriptor">The result descriptor.</param>
        public XInputException(ResultDescriptor descriptor)
            : base(descriptor.ToString())
        {
            this.descriptor = descriptor;
            this.HResult = (int)descriptor.Result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="result">The error result code.</param><param name="message">The message describing the exception.</param>
        public XInputException(Result result, string message)
            : base(message)
        {
            this.descriptor = ResultDescriptor.Find(result);
            this.HResult = (int)result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="result">The error result code.</param><param name="message">The message describing the exception.</param><param name="args">formatting arguments</param>
        public XInputException(Result result, string message, params object[] args)
            : base(string.Format((IFormatProvider)CultureInfo.InvariantCulture, message, args))
        {
            this.descriptor = ResultDescriptor.Find(result);
            this.HResult = (int)result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="message">The message describing the exception.</param><param name="args">formatting arguments</param>
        public XInputException(string message, params object[] args)
            : this(Result.Fail, message, args)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.XInputException"/> class.
        /// 
        /// </summary>
        /// <param name="message">The message describing the exception.</param><param name="innerException">The exception that caused this exception.</param><param name="args">formatting arguments</param>
        public XInputException(string message, Exception innerException, params object[] args)
            : base(string.Format((IFormatProvider)CultureInfo.InvariantCulture, message, args), innerException)
        {
            this.descriptor = ResultDescriptor.Find(Result.Fail);
            this.HResult = (int)Result.Fail;
        }
    }
}
