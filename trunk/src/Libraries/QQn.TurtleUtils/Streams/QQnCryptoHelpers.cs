using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public static class QQnCryptoHelpers
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="bytes"></param>
		/// <returns></returns>
		public static string HashString(byte[] bytes)
		{
			if (bytes == null)
				throw new ArgumentNullException("bytes");
			StringBuilder sb = new StringBuilder(bytes.Length * 2);

			foreach (byte b in bytes)
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:x2}", b);

			return sb.ToString();
		}
	}
}
