using System;
using System.Runtime.InteropServices;

namespace Blueberry
{
    /// <summary>
    /// A boolean value stored on 4 bytes (instead of 1 in .NET).
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal struct Bool : IEquatable<Bool>
    {
        private int boolValue;

        /// <summary>
        /// Initializes a new instance of the Bool class.
        /// </summary>
        /// <param name="boolValue">if set to <c>true</c> [bool value].</param>
        public Bool(bool boolValue)
        {
            this.boolValue = boolValue ? 1 : 0;
        }

        public static implicit operator bool(Bool booleanValue)
        {
            return booleanValue.boolValue != 0;
        }

        public static implicit operator Bool(bool boolValue)
        {
            return new Bool(boolValue);
        }

        /// <summary>
        /// Implements the ==.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Bool left, Bool right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the !=.
        /// 
        /// </summary>
        /// <param name="left">The left.</param><param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Bool left, Bool right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// 
        /// </summary>
        /// <param name="other">The other.</param>
        /// <returns>
        /// true if <paramref name="other"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public bool Equals(Bool other)
        {
            return this.boolValue == other.boolValue;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals((object)null, obj) || !(obj is Bool))
                return false;
            else
                return this.Equals((Bool)obj);
        }

        public override int GetHashCode()
        {
            return this.boolValue;
        }

        public override string ToString()
        {
            return string.Format("{0}", (bool)(this.boolValue != 0 ? true : false));
        }
    }
}
