using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Items;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using System.IO;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tags.ExpressionParser;
using System.Text.RegularExpressions;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagItem : ICollectionItem<TagItem>, ITagItem
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

			_include = include;
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

		#region ITagItem Members

		/// <summary>
		/// Gets or sets the item specification.
		/// </summary>
		/// <value>The item specification</value>
		string ITagItem.ItemSpec
		{
			get { return this.Include; }
			set { this.Include = value; }
		}

		/// <summary>
		/// Gets the number of metadata entries associated with the item.
		/// </summary>
		/// <value>The number of metadata entries associated with the item..</value>
		int ITagItem.MetadataCount
		{
			get { return Keys.Count; }
		}

		/// <summary>
		/// Gets the name of the TagItemKeys
		/// </summary>
		/// <value></value>
		ICollection<string> ITagItem.KeyNames
		{
			get { return Keys.AllKeys; }
		}

		/// <summary>
		/// Gets or sets the <see cref="System.String"/> with the specified key name.
		/// </summary>
		/// <value></value>
		string ITagItem.this[string keyName]
		{
			get { return Keys[keyName].Value; }
			set { Keys.Set(keyName, value); }
		}

		/// <summary>
		/// Removes the key.
		/// </summary>
		/// <param name="keyName">Name of the key.</param>
		void ITagItem.RemoveKey(string keyName)
		{
			Keys.Remove(keyName);
		}

		#endregion

		/// <summary>
		/// Expands the value.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public string ExpandedValue(TagContext context, string definition)
		{
			if(context == null)
				throw new ArgumentNullException("context");
			if (definition == null)
				return Include;

			return TagExpander.KeyOrPropertyRegex.Replace(definition, delegate(Match match)
			{
				Group g;

				if (null != (g = match.Groups[TagExpander.RegexGroupKey]) && g.Success)
				{
					Group gg = match.Groups[TagExpander.RegexGroupItemPrefix];

					if (gg != null && gg.Success)
						throw new NotImplementedException();

					if (Keys.Contains(g.Value))
						return Keys[g.Value].ExpandedValue();
					else
						return "";
				}
				else if (null != (g = match.Groups[TagExpander.RegexGroupProperty]))
				{
					if (context.Properties.Contains(g.Value))
						return context.Properties[g.Value].ExpandedValue();
					else
						return "";
				}
				else
					return "";
			});
		}

		/// <summary>
		/// Expandes the key.
		/// </summary>
		/// <param name="keyName">Name of the key.</param>
		/// <returns></returns>
		public string ExpandedKey(string keyName)
		{
			if (string.IsNullOrEmpty(keyName))
				return Include;

			if (Keys.Contains(keyName))
				return Keys[keyName].ExpandedValue();

			return null;		
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
		/// Adds the file.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="include">The include.</param>
		/// <param name="baseDirectory">The base directory.</param>
		/// <returns></returns>
		public TagItem AddFile(string name, string include, string baseDirectory)
		{
			baseDirectory = Path.GetFullPath(baseDirectory);
			include = QQnPath.EnsureRelativePath(baseDirectory, include);

			TagItem item = new TagItem(name, include);
			item.Keys.Add("FileOrigin", baseDirectory);

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
