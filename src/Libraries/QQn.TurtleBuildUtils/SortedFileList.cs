using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using QQn.TurtleUtils.IO;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.SortedFileList")]
[module: SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Scope = "type", Target = "QQn.TurtleBuildUtils.SortedFileList`1")]

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// Collection of files
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class SortedFileList<T> : SortedList<string, T>, IEnumerable<T>
	{
		string _baseDirectory;

		/// <summary>
		/// Initializes a new instance of the <see cref="SortedFileList&lt;T&gt;"/> class.
		/// </summary>
		public SortedFileList()
			: base(StringComparer.OrdinalIgnoreCase)
		{
		}

		/// <summary>
		/// Returns an enumerator that iterates through values inthe collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		public new IEnumerator<T> GetEnumerator()
		{
			return Values.GetEnumerator();
		}

		/// <summary>
		/// Gets or sets the base path.
		/// </summary>
		/// <value>The base path.</value>
		public string BaseDirectory
		{
			get { return _baseDirectory; }
			set
			{
				if (Count > 0)
					throw new InvalidOperationException();

				_baseDirectory = value;
			}
		}

		/// <summary>
		/// Adds an element with the specified key and value into the <see cref="T:System.Collections.Generic.SortedList`2"/>.
		/// </summary>
		/// <param name="key">The key of the element to add.</param>
		/// <param name="value">The value of the element to add. The value can be null for reference types.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		/// <exception cref="T:System.ArgumentException">An element with the same key already exists in the <see cref="T:System.Collections.Generic.SortedList`2"/>.</exception>
		public new void Add(string key, T value)
		{
			base.Add(EnsureRelative(key), value);
		}

		/// <summary>
		/// Ensures the specified key is relative
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected virtual string EnsureRelative(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			if (string.IsNullOrEmpty(BaseDirectory))
				return Path.GetFullPath(key);
			else
				return QQnPath.EnsureRelativePath(BaseDirectory, key);
		}

		/// <summary>
		/// Gets the item with the specified key.
		/// </summary>
		/// <value></value>
		public new T this[string key]
		{
			get { return base[EnsureRelative(key)]; }
			set { base[EnsureRelative(key)] = value; }
		}

		/// <summary>
		/// Gets the T at the specified index.
		/// </summary>
		/// <value></value>
		public T this[int index]
		{
			get { return base[Keys[index]]; }
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">The key whose value to get.</param>
		/// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value"/> parameter. This parameter is passed uninitialized.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.SortedList`2"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		public new bool TryGetValue(string key, out T value)
		{
			return base.TryGetValue(EnsureRelative(key), out value);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="T:System.Collections.Generic.SortedList`2"/>.
		/// </summary>
		/// <param name="key">The key of the element to remove.</param>
		/// <returns>
		/// true if the element is successfully removed; otherwise, false.  This method also returns false if <paramref name="key"/> was not found in the original <see cref="T:System.Collections.Generic.SortedList`2"/>.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		public new bool Remove(string key)
		{
			return base.Remove(EnsureRelative(key));
		}

		/// <summary>
		/// Determines whether the <see cref="T:System.Collections.Generic.SortedList`2"/> contains a specific key.
		/// </summary>
		/// <param name="key">The key to locate in the <see cref="T:System.Collections.Generic.SortedList`2"/>.</param>
		/// <returns>
		/// true if the <see cref="T:System.Collections.Generic.SortedList`2"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		public new bool ContainsKey(string key)
		{
			return base.ContainsKey(EnsureRelative(key));
		}
	}
	/// <summary>
	/// 
	/// </summary>
	public class SortedFileList : SortedFileList<string>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SortedFileList"/> class.
		/// </summary>
		public SortedFileList()
		{
		}

		/// <summary>
		/// Adds the specified filename.
		/// </summary>
		/// <param name="filename">The filename.</param>
		public virtual void Add(string filename)
		{
			string relative = EnsureRelative(filename);
			base.Add(relative, relative);
		}

		/// <summary>
		/// Adds the specified file if it was not already added
		/// </summary>
		/// <param name="filename"></param>
		public virtual void AddUnique(string filename)
		{
			string relative = EnsureRelative(filename);

			if (!ContainsKey(relative))
				Add(relative, relative);
		}

		/// <summary>
		/// Determines whether [contains] [the specified filename].
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns>
		/// 	<c>true</c> if the list contains the specified filename; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string filename)
		{
			return ContainsKey(filename);
		}
	}
}
