using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TagBatchDefinition : System.Collections.IEnumerable
	{
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		public abstract System.Collections.IEnumerator GetEnumerator();


		static Type _iTaskItem;

		internal static bool IsTagItemType(Type type)
		{
			if (type == null)
				throw new ArgumentNullException("type");

			if (type == typeof(string) || type == typeof(ITagItem))
				return true;

			if (type != _iTaskItem && (type.Name == "ITaskItem" && type.Namespace == "Microsoft.Build.Framework"))
			{
				if (_iTaskItem == null)
				{
					// Never load the assembly ourselves, but allow nevertheless
					AssemblyName name = new AssemblyName(type.Assembly.FullName);

					if (QQnCryptoHelpers.HashString(name.GetPublicKeyToken()) == "b03f5f7f11d50a3a")
						_iTaskItem = type;
				}
			}

			return type == _iTaskItem;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class TagBatchDefinition<TKey> : TagBatchDefinition
		where TKey : class
	{
		Dictionary<TKey, TagBatchItem> _items = new Dictionary<TKey, TagBatchItem>();		

		/// <summary>
		/// Adds the specified definitionw
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="definition">The definition.</param>
		/// <param name="resultType">Type of the result.</param>
		public void Add(TKey key, string definition, Type resultType)
		{
			if (key == null)
				throw new ArgumentNullException("key");
			else if(string.IsNullOrEmpty(definition))
				throw new ArgumentNullException("definition");

			if (resultType.IsArray)
			{
				if (IsTagItemType(resultType))
					_items.Add(key, new TagMultiBatchItem(definition, resultType));
				else
					throw new ArgumentException("Invalid result type", "resultType");
			}
			else
			{
				if (IsTagItemType(resultType))
					_items.Add(key, new TagSingleBatchItem(definition, resultType));
				else
					throw new ArgumentException("Invalid result type", "resultType");
			}
		}

		/// <summary>
		/// Adds the specified condition.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="definition">The definition.</param>
		public void AddCondition(TKey key, string definition)
		{
			if (key == null)
				throw new ArgumentNullException("key");
			else if (string.IsNullOrEmpty(definition))
				throw new ArgumentNullException("definition");

			_items.Add(key, new TagConditionItem(definition));
		}

		/// <summary>
		/// Gets a collection containing all defined keys.
		/// </summary>
		/// <value>All keys.</value>
		public ICollection<TKey> AllKeys
		{
			get { return _items.Keys; }
		}


		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
		/// </returns>
		public override System.Collections.IEnumerator GetEnumerator()
		{
			return AllKeys.GetEnumerator();
		}

		internal void Prepare()
		{

		}
	}
}
