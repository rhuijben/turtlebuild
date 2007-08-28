using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.AttributeParsers
{
	/// <summary>
	/// 
	/// </summary>
	public class NamedAttributeArgument
	{
		readonly string _name;
		string _value;

		/// <summary>
		/// Initializes a new instance of the <see cref="NamedAttributeArgument"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public NamedAttributeArgument(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_name = name;
		}

		/// <summary>
		/// Gets or sets the name of the named argument.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets or sets the value of the named argument
		/// </summary>
		/// <value>The value.</value>
		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class AttributeDefinition
	{
		string _name;
		string _comment;
		string _namespacePrefix;
		readonly Collection<string> _parameters;
		readonly KeyedCollection<string, NamedAttributeArgument> _namedArguments;

		sealed class ArgCollection : KeyedCollection<string, NamedAttributeArgument>
		{
			public ArgCollection()
				: base(StringComparer.InvariantCultureIgnoreCase)
			{
			}

			protected override string GetKeyForItem(NamedAttributeArgument item)
			{
				return item.Name;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AttributeDefinition"/> class.
		/// </summary>
		public AttributeDefinition()
		{
			_namedArguments = new ArgCollection();
			_parameters = new Collection<string>();
		}

		/// <summary>
		/// Gets or sets the namespace prefix using a single dot as namespace separator
		/// </summary>
		/// <value>The namespace prefix.</value>
		public string NamespacePrefix
		{
			get { return _namespacePrefix; }
			set { _namespacePrefix = value; }
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		/// <summary>
		/// Gets the arguments.
		/// </summary>
		/// <value>The arguments.</value>
		public Collection<string> Arguments
		{
			get { return _parameters; }
		}

		/// <summary>
		/// Gets the named arguments.
		/// </summary>
		/// <value>The named arguments.</value>
		public KeyedCollection<string, NamedAttributeArgument> NamedArguments
		{
			get { return _namedArguments; }
		}

		/// <summary>
		/// Gets or sets the comment.
		/// </summary>
		/// <value>The comment.</value>
		public string Comment
		{
			get { return _comment; }
			set { _comment = value != null ? value.Replace('\n', ' ').Replace('\r', ' ') : null; }
		}
	}
}
