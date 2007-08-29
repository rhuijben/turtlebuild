using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace QQn.TurtleUtils.Tokens.Converters
{
	/// <summary>
	/// TypeConverter for <see cref="System.Version"/>
	/// </summary>
	public sealed class UtcDateTimeConverter : TypeConverter
	{
		/// <summary>
		/// Converts the timestamp to a UTC string, usable for roundtripping via this converter
		/// </summary>
		/// <param name="timestamp">The timestamp.</param>
		/// <returns></returns>
		public static string ToString(DateTime timestamp)
		{
			return timestamp.ToUniversalTime().ToString("u");
		}

		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string name = value as string;
			if (name != null)
			{
				return DateTime.Parse(name, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
		}


		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType.IsAssignableFrom(typeof(string)) || base.CanConvertTo(context, destinationType);
		}


		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType.IsAssignableFrom(typeof(string)))
				return ((DateTime)value).ToUniversalTime().ToString("u");

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}

	/// <summary>
	/// TypeConverter for <see cref="System.Version"/>
	/// </summary>
	public sealed class TicksDateTimeConverter : TypeConverter
	{
		/// <summary>
		/// Converts the given object to the type of this converter, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo"/> to use as the current culture.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		{
			string name = value as string;
			if (name != null)
			{
				DateTimeKind kind = DateTimeKind.Unspecified;
				switch (name[0])
				{
					case 'Z':
					case 'U':
						kind = DateTimeKind.Utc;
						name = name.Substring(1);
						break;
					case 'L':
						kind = DateTimeKind.Local;
						name = name.Substring(1);
						break;
				}

				long ticks;
				if (long.TryParse(name, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ticks))
					return new DateTime(ticks, kind);
				else
					return DateTime.MinValue;
			}

			return base.ConvertFrom(context, culture, value);
		}

		/// <summary>
		/// Returns whether this converter can convert an object of the given type to the type of this converter, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="sourceType">A <see cref="T:System.Type"/> that represents the type you want to convert from.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return (sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType);
		}


		/// <summary>
		/// Returns whether this converter can convert the object to the specified type, using the specified context.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="destinationType">A <see cref="T:System.Type"/> that represents the type you want to convert to.</param>
		/// <returns>
		/// true if this converter can perform the conversion; otherwise, false.
		/// </returns>
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return destinationType.IsAssignableFrom(typeof(string)) || base.CanConvertTo(context, destinationType);
		}


		/// <summary>
		/// Converts the given value object to the specified type, using the specified context and culture information.
		/// </summary>
		/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext"/> that provides a format context.</param>
		/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo"/>. If null is passed, the current culture is assumed.</param>
		/// <param name="value">The <see cref="T:System.Object"/> to convert.</param>
		/// <param name="destinationType">The <see cref="T:System.Type"/> to convert the <paramref name="value"/> parameter to.</param>
		/// <returns>
		/// An <see cref="T:System.Object"/> that represents the converted value.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">The <paramref name="destinationType"/> parameter is null. </exception>
		/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed. </exception>
		public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType.IsAssignableFrom(typeof(string)))
			{
				DateTime dateTime = (DateTime)value;

				switch (dateTime.Kind)
				{
					case DateTimeKind.Utc:
						return 'Z' + dateTime.Ticks.ToString("x", CultureInfo.InvariantCulture);
					case DateTimeKind.Local:
						return 'L' + dateTime.Ticks.ToString("x", CultureInfo.InvariantCulture);
					default:
						return dateTime.Ticks.ToString("x", CultureInfo.InvariantCulture);
				}
			}

			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
