namespace Blueberry
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /// <summary>Represents a closed range of floating point values.</summary>
    [Serializable]
    public struct Range : IEquatable<Range>, IFormattable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Range"/> structure.
        /// </summary>
        /// <param name="minimum">The minimum value in the closed range.</param>
        /// <param name="maximum">The maximum value in the closed range.</param>
        public Range(float minimum, float maximum)
        {
            this.Minimum = minimum;
            this.Maximum = maximum;
        }

        /// <summary>
        /// Gets or sets the inclusive minimum value in the range.
        /// </summary>
        public float Minimum;

        /// <summary>
        /// Gets or sets the inclusive maximum value in the range.
        /// </summary>
        public float Maximum;

        /// <summary>
        /// Gets the size of the range.
        /// </summary>
        public float Size
        {
            get { return Math.Abs(this.Maximum - this.Minimum); }
        }

        /// <summary>
        /// Creates a new range by parsing an ISO 31-11 string representation of a closed interval.
        /// </summary>
        /// <param name="value">Input stirng value.</param>
        /// <returns>A new range value.</returns>
        /// <exception cref="FormatException">Thrown if the input String is not in a valid ISO 31-11 closed interval format.</exception>
        /// <remarks>Example of a well formed ISO 31-11 closed interval: <i>"[0,1]"</i>. Open intervals are not supported.</remarks>
        static public Range Parse(String value)
        {
            return Range.Parse(value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a new range by parsing an ISO 31-11 string representation of a closed interval.
        /// </summary>
        /// <param name="value">Input stirng value.</param>
        /// <param name="format">The format provider.</param>
        /// <remarks>Example of a well formed ISO 31-11 closed interval: <i>"[0,1]"</i>. Open intervals are not supported.</remarks>
        static public Range Parse(String value, IFormatProvider format)
        {
            if (value == null || value == "")
                throw new ArgumentNullException("value", "value can't be null or empty");

            if (!value.StartsWith("[") || !value.EndsWith("]"))
                goto badformat;

            NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(format);

            char[] groupSeperator = numberFormat.NumberGroupSeparator.ToCharArray();

            String[] endpoints = value.Trim(new char[] { '[', ']' }).Split(groupSeperator);

            if (endpoints.Length != 2)
                goto badformat;

            return new Range
            {
                Minimum = Single.Parse(endpoints[0], NumberStyles.Float, numberFormat),
                Maximum = Single.Parse(endpoints[1], NumberStyles.Float, numberFormat)
            };

        badformat:
            throw new FormatException("Value is not in ISO 31-11 format for a closed interval.");
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean Equals(Object obj)
        {
            if (obj != null)
                if (obj is Range)
                    return this.Equals((Range)obj);

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Range"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="Range"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Range"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public Boolean Equals(Range value)
        {
            return this.Minimum.Equals(value.Minimum) &&
                   this.Maximum.Equals(value.Maximum);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override Int32 GetHashCode()
        {
            return this.Minimum.GetHashCode() ^ this.Maximum.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override String ToString()
        {
            return this.ToString("G", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public String ToString(IFormatProvider formatProvider)
        {
            return this.ToString("G", formatProvider);
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public String ToString(String format, IFormatProvider formatProvider)
        {
            NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(formatProvider);

            String minimum = this.Minimum.ToString(format, numberFormat);
            String maximum = this.Maximum.ToString(format, numberFormat);
            String seperator = numberFormat.NumberGroupSeparator;

            return String.Format(formatProvider, "[{0}{1}{2}]", minimum, seperator, maximum);
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="x">The lvalue.</param>
        /// <param name="y">The rvalue.</param>
        /// <returns>
        /// 	<c>true</c> if the lvalue <see cref="Range"/> is equal to the rvalue; otherwise, <c>false</c>.
        /// </returns>
        static public Boolean operator ==(Range x, Range y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="x">The lvalue.</param>
        /// <param name="y">The rvalue.</param>
        /// <returns>
        /// 	<c>true</c> if the lvalue <see cref="Range"/> is not equal to the rvalue; otherwise, <c>false</c>.
        /// </returns>
        static public Boolean operator !=(Range x, Range y)
        {
            return !x.Equals(y);
        }

        /// <summary>
        /// Implicit cast operator from Single to Range.
        /// </summary>
        /// <param name="value">The floating point value.</param>
        /// <returns>A new range Object with minimum and maximum values set to <paramref name="value"/></returns>
        static public implicit operator Range(float value)
        {
            return new Range
            {
                Minimum = value,
                Maximum = value
            };
        }
    }
}