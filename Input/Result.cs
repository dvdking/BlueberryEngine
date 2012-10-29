using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Blueberry.XInput
{
    /// <summary>
    /// Result structure for COM methods.
    /// 
    /// </summary>
    [Serializable]
    internal struct Result : IEquatable<Result>
    {
        /// <summary>
        /// Result code Ok
        /// 
        /// </summary>
        public static Result Ok = new Result(0);
        /// <summary>
        /// Result code False
        /// 
        /// </summary>
        public static Result False = new Result(1);
        /// <summary>
        /// Result code Abord
        /// 
        /// </summary>
        public static Result Abord = new Result(-2147467260);
        /// <summary>
        /// Result code AccessDenied
        /// 
        /// </summary>
        public static Result AccessDenied = new Result(-2147024891);
        /// <summary>
        /// Result code Fail
        /// 
        /// </summary>
        public static Result Fail = new Result(-2147467259);
        /// <summary>
        /// Resuld code Handle
        /// 
        /// </summary>
        public static Result Handle = new Result(-2147024890);
        /// <summary>
        /// Result code invalid argument
        /// 
        /// </summary>
        public static Result InvalidArg = new Result(-2147024809);
        /// <summary>
        /// Result code no interface
        /// 
        /// </summary>
        public static Result NoInterface = new Result(-2147467262);
        /// <summary>
        /// Result code not implemented
        /// 
        /// </summary>
        public static Result NotImplemented = new Result(-2147467263);
        /// <summary>
        /// Result code out of memory
        /// 
        /// </summary>
        public static Result OutOfMemory = new Result(-2147024882);
        /// <summary>
        /// Result code Invalid pointer
        /// 
        /// </summary>
        public static Result InvalidPointer = new Result(-2147467261);
        /// <summary>
        /// Unexpected failure
        /// 
        /// </summary>
        public static Result UnexpectedFailure = new Result(-2147418113);
        private int _code;

        /// <summary>
        /// Gets the HRESULT error code.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// The HRESULT error code.
        /// </value>
        public int Code
        {
            get
            {
                return this._code;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Blueberry.XInput.Result"/> is success.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if success; otherwise, <c>false</c>.
        /// </value>
        public bool Success
        {
            get
            {
                return this.Code >= 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="T:Blueberry.XInput.Result"/> is failure.
        /// 
        /// </summary>
        /// 
        /// <value>
        /// <c>true</c> if failure; otherwise, <c>false</c>.
        /// </value>
        public bool Failure
        {
            get
            {
                return this.Code < 0;
            }
        }

        static Result()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.Result"/> struct.
        /// 
        /// </summary>
        /// <param name="code">The HRESULT error code.</param>
        public Result(int code)
        {
            this._code = code;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Blueberry.XInput.Result"/> struct.
        /// 
        /// </summary>
        /// <param name="code">The HRESULT error code.</param>
        public Result(uint code)
        {
            this._code = (int)code;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:Blueberry.XInput.Result"/> to <see cref="T:System.Int32"/>.
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator int(Result result)
        {
            return result.Code;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:Blueberry.XInput.Result"/> to <see cref="T:System.UInt32"/>.
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static explicit operator uint(Result result)
        {
            return (uint)result.Code;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:System.Int32"/> to <see cref="T:Blueberry.XInput.Result"/>.
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Result(int result)
        {
            return new Result(result);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="T:System.UInt32"/> to <see cref="T:Blueberry.XInput.Result"/>.
        /// 
        /// </summary>
        /// <param name="result">The result.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Result(uint result)
        {
            return new Result(result);
        }

        /// <summary>
        /// Implements the operator ==.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Result left, Result right)
        {
            return left.Code == right.Code;
        }

        /// <summary>
        /// Implements the operator !=.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Result left, Result right)
        {
            return left.Code != right.Code;
        }

        /// <summary>
        /// Equalses the specified other.
        /// 
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns/>
        public bool Equals(Result other)
        {
            return this.Code == other.Code;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to this instance.
        /// 
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="T:System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// 
        /// </returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Result))
                return false;
            else
                return this.Equals((Result)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// 
        /// </returns>
        public override int GetHashCode()
        {
            return this.Code;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents this instance.
        /// 
        /// </summary>
        /// 
        /// <returns>
        /// A <see cref="T:System.String"/> that represents this instance.
        /// 
        /// </returns>
        public override string ToString()
        {
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "HRESULT = 0x{0:X}", new object[1]
      {
        (object) this._code
      });
        }

        /// <summary>
        /// Checks the error.
        /// 
        /// </summary>
        public void CheckError()
        {
            if (this._code < 0)
                throw new XInputException(this);
        }

        /// <summary>
        /// Gets a <see cref="T:Blueberry.XInput.Result"/> from an <see cref="T:System.Exception"/>.
        /// 
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <returns>
        /// The associated result code
        /// </returns>
        public static Result GetResultFromException(Exception ex)
        {
            return new Result(Marshal.GetHRForException(ex));
        }
    }
}
