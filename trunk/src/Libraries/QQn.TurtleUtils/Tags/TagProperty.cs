using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;
using System.Diagnostics;
using System.Collections.ObjectModel;
using QQn.TurtleUtils.Items;
using System.ComponentModel;
using QQn.TurtleUtils.Tags.ExpressionParser;
using System.Text.RegularExpressions;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	[DebuggerDisplay("Name={Name} Value={Value} Lazy={LazyEvaluated}")]
	public class TagProperty : ICollectionItem<TagProperty>
	{
		string _name;
		string _value;
		bool _lazyEvaluated;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		public TagProperty()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TagProperty(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			Name = name;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagProperty"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		public TagProperty(string name, string value)
			: this(name)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException("value");

			_value = value;
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
		[XmlAttribute("value"), Token("value")]
		public virtual string Value
		{
			get { return _value ?? ""; }
			set { _value = value; }
		}

		/// <summary>
		/// Gets a value indicating whether this property is lazy evaluated
		/// </summary>
		/// <value><c>true</c> if the property is lazy evaluated; otherwise, <c>false</c>.</value>
		[XmlAttribute("lazy"), Token("lazy"), DefaultValue(false)]
		public virtual bool LazyEvaluated
		{
			get { return _lazyEvaluated; }
			set 
			{
				_lazyEvaluated = value;
			}
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return Value;
		}

		/// <summary>
		/// Performs an explicit conversion from <see cref="QQn.TurtleUtils.Tags.TagProperty"/> to <see cref="System.Collections.Generic.KeyValuePair{TKey,TValue}"/>.
		/// </summary>
		/// <param name="property">The property.</param>
		/// <returns>The result of the conversion.</returns>
		public static explicit operator KeyValuePair<string, string>(TagProperty property)
		{
			return new KeyValuePair<string, string>(property.Name, property.Value);
		}

		#region ICollectionItem<TagProperty> Members
		Collection<TagProperty> _collection;
		Collection<TagProperty> ICollectionItem<TagProperty>.Collection
		{
			get { return _collection; }
			set { _collection = value; }
		}
		#endregion

		/// <summary>
		/// Gets the environment.
		/// </summary>
		/// <value>The environment.</value>
		public TagEnvironment Environment
		{
			get
			{
				TagPropertyCollection collection = _collection as TagPropertyCollection;
				if (collection == null)
					return null;

				return collection.Environment;
			}
		}
	
		/// <summary>
		/// Returns the value applied via <see cref="TagPropertyCollection.ExpandProperties(String)"/>
		/// </summary>
		/// <returns></returns>
		public virtual string ExpandedValue()
		{
 			return Value;
		}
	}

	/// <summary>
	/// Keyed collection of <see cref="TagProperty"/> instances
	/// </summary>
	public class TagPropertyCollection : KeyedItemCollection<string, TagProperty>
	{
		TagEnvironment _environment;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagPropertyCollection"/> class.
		/// </summary>
		public TagPropertyCollection()
			: this(null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TagPropertyCollection"/> class.
		/// </summary>
		/// <param name="environment">The environment.</param>
		internal TagPropertyCollection(TagEnvironment environment)
			: base(StringComparer.OrdinalIgnoreCase)
		{
			_environment = environment;
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

		/// <summary>
		/// Extracts the name from the specified element.
		/// </summary>
		/// <param name="item">The element from which to extract the name.</param>
		/// <returns>The name for the specified element.</returns>
		protected override string GetKeyForItem(TagProperty item)
		{
			return item.Name;
		}

		/// <summary>
		/// Adds the specified property
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public TagProperty Add(string name, string value)
		{
			TagProperty property = new TagProperty(name, value);
			Add(property);

			return property;
		}

		/// <summary>
		/// Sets the specified property to the specified value
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		public void Set(string key, string value)
		{
			if (Contains(key))
				this[key].Value = value;
			else
				this.Add(new TagProperty(key, value));
		}

		/// <summary>
		/// Loads the environment variables of the current process in the collection
		/// </summary>
		/// <remarks>Skips properties duplicate properties and properties which already exist</remarks>
		public void LoadEnvironmentVariables()
		{
			System.Collections.IDictionary vars = System.Environment.GetEnvironmentVariables();

			foreach (string key in vars.Keys)
			{
				if (!Contains(key))
					Add(key, (string)vars[key]);
			}
		}

		/// <summary>
		/// Expands the properties.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="emptyUndefined">if set to <c>true</c> use "" for undefined properties, otherwise throw an argumentexception is thrown.</param>
		/// <returns></returns>
		public string ExpandProperties(string value, bool emptyUndefined)
		{
			if (value == null)
				throw new ArgumentNullException("value");
			else if (value.Length == 0)
				return "";

			return TagExpander.PropertyRegex.Replace(value,
				emptyUndefined ? new MatchEvaluator(ApplyPropertyEmpty) : new MatchEvaluator(ApplyPropertyException));
		}

		/// <summary>
		/// Expands the properties, using "" for undefined properties.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public string ExpandProperties(string value)
		{
			return ExpandProperties(value, true);
		}


		string ApplyPropertyEmpty(Match match)
		{
			string key = match.Groups[TagExpander.RegexGroupProperty].Value;

			if (Contains(key))
				return this[key].ExpandedValue();
			else
				return "";
		}

		string ApplyPropertyException(Match match)
		{
			string key = match.Groups[TagExpander.RegexGroupProperty].Value;

			if (Contains(key))
				return this[key].ExpandedValue();
			else
				throw new ArgumentException(string.Format("Invalid property reference: '{0}'", key), "key");
		}
	}
}
