using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	enum ExValueType
	{
		Boolean,
		String,
		Int,
		Double,
	}

	/// <summary>
	/// Expression value encapsulator
	/// </summary>
	sealed class ExValue : IEquatable<ExValue>, IComparable<ExValue>, IComparable, IFormattable
	{
		readonly ExValueType _type;
		readonly string _str;
		readonly bool _bl;
		readonly int _number;
		readonly double _dbl;
		readonly object _value;

		/// <summary>
		/// Initializes a new instance of the <see cref="ExValue"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExValue(string value)
		{
			_type = ExValueType.String;
			_value = _str = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExValue"/> class.
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		public ExValue(bool value)
		{
			_type = ExValueType.Boolean;
			_value = _bl = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExValue"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExValue(int value)
		{
			_type = ExValueType.Int;
			_value = _number = value;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ExValue"/> class.
		/// </summary>
		/// <param name="value">The value.</param>
		public ExValue(double value)
		{
			_type = ExValueType.Double;
			_value = _dbl = value;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Value.ToString();
		}

		/// <summary>
		/// Gets the raw value
		/// </summary>
		/// <value>The value.</value>
		public object Value
		{
			get { return _value; }
		}

		#region IFormattable Members

		/// <summary>
		/// Formats the value of the current instance using the specified format.
		/// </summary>
		/// <param name="format">The <see cref="T:System.String"/> specifying the format to use.-or- null to use the default format defined for the type of the <see cref="T:System.IFormattable"/> implementation.</param>
		/// <param name="formatProvider">The <see cref="T:System.IFormatProvider"/> to use to format the value.-or- null to obtain the numeric format information from the current locale setting of the operating system.</param>
		/// <returns>
		/// A <see cref="T:System.String"/> containing the value of the current instance in the specified format.
		/// </returns>
		public string ToString(string format, IFormatProvider formatProvider)
		{
			IFormattable f = Value as IFormattable;

			if (f != null)
				return f.ToString(format, formatProvider);
			else
				return Value.ToString();
		}

		#endregion

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		/// <summary>
		/// Gets the type of the ex value.
		/// </summary>
		/// <value>The type of the ex value.</value>
		public ExValueType ExValueType
		{
			get { return _type; }
		}

		#region IEquatable<ExValue> Members

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(ExValue other)
		{
			if ((object)other == null)
				return false;

			if (other.ExValueType != ExValueType)
				return false;

			switch (ExValueType)
			{
				case ExValueType.Boolean:
					return _bl == other._bl;
				case ExValueType.String:
					return StringComparer.OrdinalIgnoreCase.Equals(_str, other._str);
				case ExValueType.Double:
					return _dbl == other._dbl;
				case ExValueType.Int:
					return _number == other._number;
				default:
					throw new InvalidOperationException();
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
		public override bool Equals(object obj)
		{
			return Equals((ExValue)obj);
		}

		#endregion

		/// <summary>
		/// Converts the value to a boolean
		/// </summary>
		/// <returns></returns>
		internal bool ToBool()
		{
			switch (ExValueType)
			{
				case ExValueType.Boolean:
					return _bl;
				case ExValueType.Int:
					return _number != 0;
				case ExValueType.Double:
					return _dbl != 0.0;
				default:
					throw new InvalidOperationException();
			}
		}

		#region IComparable<ExValue> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(ExValue other)
		{
			if (other == null)
				throw new ArgumentNullException("other");
			else if (other.ExValueType != ExValueType)
				throw new InvalidOperationException();

			switch (ExValueType)
			{
				case ExValueType.Boolean:
					return _bl.CompareTo(other._bl);
				case ExValueType.String:
					return StringComparer.OrdinalIgnoreCase.Compare(_str, other._str);
				case ExValueType.Double:
					return _dbl.CompareTo(other._dbl);
				case ExValueType.Int:
					return _number.CompareTo(other._number);
				default:
					throw new InvalidOperationException();
			}
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Compares the current instance with another object of the same type.
		/// </summary>
		/// <param name="obj">An object to compare with this instance.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj"/>. Zero This instance is equal to <paramref name="obj"/>. Greater than zero This instance is greater than <paramref name="obj"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="obj"/> is not the same type as this instance. </exception>
		public int CompareTo(object obj)
		{
			return CompareTo(obj as ExValue);
		}

		#endregion
	}
}
