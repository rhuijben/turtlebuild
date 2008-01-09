using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace QQn.TurtleUtils.Items
{
	/// <summary>
	/// Gets a pair of two objects
	/// </summary>
	/// <typeparam name="TFirst">The type of the first.</typeparam>
	/// <typeparam name="TSecond">The type of the second.</typeparam>
	[Serializable]
	[DebuggerDisplay("Pair: First={First}, Second={Second}")]
	public sealed class Pair<TFirst,TSecond> : IComparable<Pair<TFirst, TSecond>>, IEquatable<Pair<TFirst, TSecond>>, IComparable
		where TFirst : class
		where TSecond : class
	{
		readonly TFirst _first;
		readonly TSecond _second;
		/// <summary>
		/// Initializes a new instance of the <see cref="Pair&lt;TFirst, TSecond&gt;"/> class.
		/// </summary>
		/// <param name="first">The first.</param>
		/// <param name="second">The second.</param>
		public Pair(TFirst first, TSecond second)
		{
			_first = first;
			_second = second;
		}

		/// <summary>
		/// Gets the first.
		/// </summary>
		/// <value>The first.</value>
		public TFirst First
		{
			[DebuggerStepThrough]
			get { return _first; }
		}

		/// <summary>
		/// Gets the second.
		/// </summary>
		/// <value>The second.</value>
		public TSecond Second
		{
			[DebuggerStepThrough]
			get { return _second; }
		}


		#region IComparable<Pair<TFirst,TSecond>> Members

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>.
		/// </returns>
		public int CompareTo(Pair<TFirst, TSecond> other)
		{
			if (other == null)
				return 1;

			int n = Comparer<TFirst>.Default.Compare(First, other.First);
			if (n != 0)
				return n;

			return Comparer<TSecond>.Default.Compare(Second, other.Second);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		public bool Equals(Pair<TFirst, TSecond> other)
		{
			if(other == null)
				return false;

			return EqualityComparer<TFirst>.Default.Equals(First, other.First) &&
					EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
		}

		#endregion		

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
			return Equals(obj as Pair<TFirst, TSecond>);
		}

		/// <summary>
		/// Serves as a hash function for a particular type.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode()
		{
			if (First != null)
				return First.GetHashCode();
			else if (Second != null)
				return Second.GetHashCode();

			return 0;
		}

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
			return CompareTo(obj as Pair<TFirst, TSecond>);
		}

		#endregion
	}
}
