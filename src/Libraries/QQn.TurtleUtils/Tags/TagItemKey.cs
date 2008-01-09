using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Items;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("Key={Key} Value={Value}")]
	public class TagItemKey : ICollectionItem<TagItemKey>
	{
		string _key;
		string _value;
		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemKey"/> class.
		/// </summary>
		public TagItemKey()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemKey"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		public TagItemKey(string key)
		{
			if (String.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			Key = key;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemKey"/> class.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public TagItemKey(string key, string value)
			: this(key)
		{
			if (String.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			Value = value;
		}

		/// <summary>
		/// Gets or sets the key.
		/// </summary>
		/// <value>The key.</value>
		[XmlAttribute("key"), Token("key")]
		public string Key
		{
			get { return _key; }
			set
			{
				if (!string.IsNullOrEmpty(_key))
					throw new InvalidOperationException();
				else if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");

				_key = value;
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[XmlAttribute("value"), Token("value")]
		public string Value
		{
			get { return _value ?? ""; }
			set { _value = value; }
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <value>The item.</value>
		public TagItem Item
		{
			get
			{
				TagItemKeyCollection collection = _collection as TagItemKeyCollection;

				if (collection == null)
					return null;

				return collection.Item;
			}
		}

		/// <summary>
		/// Gets the environment.
		/// </summary>
		/// <value>The environment.</value>
		public TagEnvironment Environment
		{
			get
			{
				TagItem item = Item;

				if (item == null)
					return null;

				return item.Environment;
			}
		}

		#region ICollectionItem<TagItemKey> Members

		Collection<TagItemKey> _collection;
		Collection<TagItemKey> ICollectionItem<TagItemKey>.Collection
		{
			get { return _collection; }
			set { _collection = value; }
		}

		#endregion

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns></returns>
		public TagItemKey Clone()
		{
			return new TagItemKey(Key, Value);
		}

		internal string ExpandedValue()
		{
			return Value;
		}
	}

	/// <summary>
	/// Keyed collection of <see cref="TagItemKey"/> instances
	/// </summary>
	public class TagItemKeyCollection : KeyedItemCollection<string, TagItemKey>, ICollection<KeyValuePair<string, string>>
	{
		TagItem _item;
		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemKeyCollection"/> class.
		/// </summary>
		public TagItemKeyCollection()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemKeyCollection"/> class.
		/// </summary>
		/// <param name="item">The item.</param>
		public TagItemKeyCollection(TagItem item)
			: base(StringComparer.OrdinalIgnoreCase)
		{
			_item = item;
		}

		/// <summary>
		/// Extracts the key from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the key.</param>
		/// <returns>The key for the specified element.</returns>
		protected override string GetKeyForItem(TagItemKey item)
		{
			return item.Key;
		}

		/// <summary>
		/// Gets the item.
		/// </summary>
		/// <value>The item.</value>
		public TagItem Item
		{
			get { return _item; }
		}

		/// <summary>
		/// Adds the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public TagItemKey Add(string key, string value)
		{
			TagItemKey tik = new TagItemKey(key, value);
			Add(tik);
			return tik;
		}

		/// <summary>
		/// Sets the specified key.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public void Set(string key, string value)
		{
			if (Contains(key))
				this[key].Value = value;
			else
				Add(key, value);
		}

		#region ICollection<KeyValuePair<string,string>> Members

		void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
		{
			Add(item.Key, item.Value);
		}

		bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
		{
			return Contains(item.Key);
		}

		void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
		{
			for (int i = 0; i < Count; i++)
			{
				array[i] = new KeyValuePair<string, string>(this[i].Key, this[i].Value);
			}
		}

		bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
		{
			return Remove(item.Key);
		}

		#endregion

		#region IEnumerable<KeyValuePair<string,string>> Members

		IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
		{
			foreach (TagItemKey key in this)
			{
				yield return new KeyValuePair<string, string>(key.Key, key.Value);
			}
		}

		#endregion

		#region ICollection<KeyValuePair<string,string>> Members


		bool ICollection<KeyValuePair<string, string>>.IsReadOnly
		{
			get { return false; }
		}

		#endregion
	}
}
