using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Items;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagItem : ICollectionItem<TagItem>
	{
		string _name;
		string _include;
		bool _notExpanded;
		TagItemKeyCollection _keys;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		public TagItem()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TagItem(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");
			
			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="include">The source.</param>
		public TagItem(string name, string include)
			: this(name)
		{
			if (string.IsNullOrEmpty(include))
				throw new ArgumentNullException("include");

			Include = include;
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[XmlAttribute("name"), Token("name")]
		public string Name
		{
			get { return _name; }
			set 
			{
				if (!string.IsNullOrEmpty(_name))
					throw new InvalidOperationException();
				else if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException("value");

				_name = value;
			}
		}

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>The value.</value>
		[Token("include")]
		public virtual string Include
		{
			get { return _include ?? ""; }
			set { _include = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this property is lazy evaluated
		/// </summary>
		/// <value><c>true</c> if the property is lazy evaluated; otherwise, <c>false</c>.</value>
		[Token("expanded"), DefaultValue(true)]
		public virtual bool FullyExpanded
		{
			get { return !_notExpanded; }
			set { _notExpanded = value; }
		}

		/// <summary>
		/// Gets the keys.
		/// </summary>
		/// <value>The keys.</value>
		public TagItemKeyCollection Keys
		{
			get { return _keys ?? (_keys = new TagItemKeyCollection(this)); }
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Include;
		}

		Collection<TagItem> _collection;
		Collection<TagItem> ICollectionItem<TagItem>.Collection
		{
			get { return _collection; }
			set { _collection = value; }
		}

		/// <summary>
		/// Gets the environment.
		/// </summary>
		/// <value>The environment.</value>
		public TagEnvironment Environment
		{
			get
			{
				TagItemCollection collection = _collection as TagItemCollection;
				if (collection == null)
					return null;

				return collection.Environment;
			}
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <param name="deep">if set to <c>true</c> clone keys, otherwise copy them.</param>
		/// <returns></returns>
		public virtual TagItem Clone(bool deep)
		{
			TagItem ti = new TagItem(Name, Include);
			if (_keys != null)
			{
				foreach (TagItemKey key in Keys)
				{
					ti.Keys.Add(deep ? key.Clone() : key);
				}
			}
			return ti;
		}
	}

	/// <summary>
	/// Collection of <see cref="TagItem"/> instances
	/// </summary>
	public class TagItemCollection : Collection<TagItem>
	{
		TagEnvironment _environment;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemCollection"/> class.
		/// </summary>
		public TagItemCollection()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagItemCollection"/> class.
		/// </summary>
		/// <param name="environment">The environment.</param>
		public TagItemCollection(TagEnvironment environment)
		{
			_environment = environment;
		}

		/// <summary>
		/// Adds the specified name.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="include">The include.</param>
		public TagItem Add(string name, string include)
		{
			TagItem item = new TagItem(name, include);
			Add(item);
			return item;
		}

		/// <summary>
		/// Gets or sets the environment.
		/// </summary>
		/// <value>The environment.</value>
		public TagEnvironment Environment
		{
			get { return _environment; }
			internal set { _environment = value; }
		}
	}
}
