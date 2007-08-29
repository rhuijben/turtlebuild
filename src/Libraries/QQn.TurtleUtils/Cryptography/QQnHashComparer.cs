using System;
using System.Collections.Generic;
using System.Collections;

namespace QQn.TurtleUtils.Cryptography
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class QQnHashComparer : IComparer, IEqualityComparer, IComparer<string>, IEqualityComparer<string>
	{
		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// Value Condition Less than zero <paramref name="x"/> is less than <paramref name="y"/>. Zero <paramref name="x"/> equals <paramref name="y"/>. Greater than zero <paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">Neither <paramref name="x"/> nor <paramref name="y"/> implements the <see cref="T:System.IComparable"/> interface.-or- <paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other. </exception>
		int IComparer.Compare(object x, object y)
		{
			return Compare((string)x, (string)y);
		}

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
		int IEqualityComparer.GetHashCode(object obj)
		{
			return GetHashCode((string)obj);
		}

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="x"/> and <paramref name="y"/> are of different types and neither one can handle comparisons with the other.</exception>
		bool IEqualityComparer.Equals(object x, object y)
		{
			return Equals((string)x, (string)y);
		}

		#region IComparer<string> Members

		/// <summary>
		/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
		/// </summary>
		/// <param name="x">The first object to compare.</param>
		/// <param name="y">The second object to compare.</param>
		/// <returns>
		/// Value Condition Less than zero<paramref name="x"/> is less than <paramref name="y"/>.Zero<paramref name="x"/> equals <paramref name="y"/>.Greater than zero<paramref name="x"/> is greater than <paramref name="y"/>.
		/// </returns>
		public int Compare(string x, string y)
		{
			string xHash = QQnCryptoHelpers.NormalizeHashValue(x, false, false);
			string yHash = QQnCryptoHelpers.NormalizeHashValue(y, false, false);

			return StringComparer.InvariantCultureIgnoreCase.Compare(xHash, yHash);
		}

		#endregion

		#region IEqualityComparer<string> Members

		/// <summary>
		/// Determines whether the specified objects are equal.
		/// </summary>
		/// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
		/// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
		/// <returns>
		/// true if the specified objects are equal; otherwise, false.
		/// </returns>
		public bool Equals(string x, string y)
		{
			string xHash = QQnCryptoHelpers.NormalizeHashValue(x, false, false);
			string yHash = QQnCryptoHelpers.NormalizeHashValue(y, false, false);

			return StringComparer.InvariantCultureIgnoreCase.Equals(xHash, yHash);
		}

		/// <summary>
		/// Returns a hash code for the specified object.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
		/// <returns>A hash code for the specified object.</returns>
		/// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
		public int GetHashCode(string obj)
		{
			string xHash = QQnCryptoHelpers.NormalizeHashValue(obj, false, false);

			return StringComparer.InvariantCultureIgnoreCase.GetHashCode(xHash);
		}

		#endregion
	}
}
