using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using QQn.TurtleUtils.IO;

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
			: base(QQnPath.PathStringComparer)
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
				{
					List<KeyValuePair<string, T>> values = new List<KeyValuePair<string, T>>(this);
					bool hadBase = !string.IsNullOrEmpty(_baseDirectory);
					string oldBase = _baseDirectory;
					_baseDirectory = value;
					
					Clear();
					foreach (KeyValuePair<string, T> v in values)
					{
						Add(hadBase ? QQnPath.Combine(oldBase, v.Key) : v.Key, v.Value);
					}
				}

				_baseDirectory = value;
			}
		}

		/// <summary>
		/// Adds an element with the specified key and value into the <see cref="SortedFileList{T}"/>.
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
		/// true if the <see cref="SortedFileList{T}"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		public new bool TryGetValue(string key, out T value)
		{
			return base.TryGetValue(EnsureRelative(key), out value);
		}

		/// <summary>
		/// Removes the element with the specified key from the <see cref="SortedFileList{T}"/>.
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
		/// Determines whether the <see cref="SortedFileList{T}"/> contains a specific file
		/// </summary>
		/// <param name="key">The file to locate in the <see cref="SortedFileList{T}"/>.</param>
		/// <returns>
		/// true if the <see cref="SortedFileList{T}"/> contains an element with the specified key; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="key"/> is null.</exception>
		public new bool ContainsKey(string key)
		{
			return base.ContainsKey(EnsureRelative(key));
		}

		/// <summary>
		/// Determines whether the <see cref="SortedFileList{T}"/> contains the specified file
		/// </summary>
		/// <param name="filename">The filename.</param>
		/// <returns>
		/// 	<c>true</c> if the list contains the specified filename; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string filename)
		{
			return ContainsKey(filename);
		}

		/// <summary>
		/// Gets all the keys as full paths.
		/// </summary>
		/// <value>The keys as full paths.</value>
		public IEnumerable<string> KeysAsFullPaths
		{
			get
			{
				bool hasBase = !string.IsNullOrEmpty(BaseDirectory);

				foreach (string key in Keys)
				{
					if (hasBase)
						yield return QQnPath.Combine(BaseDirectory, key);
					else
						yield return key;
				}
			}
		}	
	}

	/// <summary>
	/// 
	/// </summary>
	public class SortedFileList : SortedFileList<string>, ICollection<string>
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
		/// <param name="item">The filename.</param>
		public virtual void Add(string item)
		{
			string relative = EnsureRelative(item);
			base.Add(relative, relative);
		}

		/// <summary>
		/// Adds the specified file if it was not already added
		/// </summary>
		/// <param name="filename">The filename.</param>
		public virtual void AddUnique(string filename)
		{
			string relative = EnsureRelative(filename);

			if (!ContainsKey(relative))
				Add(relative, relative);
		}

		#region ICollection<string> Members

		/// <summary>
		/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
		/// </summary>
		/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param>
		/// <param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// 	<paramref name="array"/> is null.</exception>
		/// <exception cref="T:System.ArgumentOutOfRangeException">
		/// 	<paramref name="arrayIndex"/> is less than 0.</exception>
		/// <exception cref="T:System.ArgumentException">
		/// 	<paramref name="array"/> is multidimensional.-or-<paramref name="arrayIndex"/> is equal to or greater than the length of <paramref name="array"/>.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="T"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
		public void CopyTo(string[] array, int arrayIndex)
		{
			Keys.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
		/// </summary>
		/// <value></value>
		/// <returns>true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.</returns>
		bool ICollection<string>.IsReadOnly
		{
			get { return false; }
		}

		#endregion
	}
}
