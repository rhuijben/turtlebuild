using System;
using System.Collections.Generic;
using System.Reflection;
using QQn.TurtleUtils.Cryptography;
using QQn.TurtleUtils.Items;
using QQn.TurtleUtils.Tags.BatchItems;

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

		/// <summary>
		/// Gets the ITaskItem type when it has been used in at least one definition; otherwise <c>null</c>
		/// </summary>
		/// <value>The I task item.</value>
		internal static Type ITaskItemTypeUnsafe
		{
			get
			{
				return _iTaskItem;
			}
		}

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

		bool _prepared;

		internal bool Prepared
		{
			get { return _prepared; }
			set { _prepared = value; }
		}

		readonly SortedList<string, string> _itemsUsed = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);
		SortedList<Pair<string, string>, string> _constraints = new SortedList<Pair<string,string>,string>();

		internal virtual void ClearPreparation()
		{
			_itemsUsed.Clear();
			_constraints.Clear();
		}

		internal abstract void Prepare();


		internal void AddUsedItem(string itemName)
		{
			if(!_itemsUsed.ContainsKey(itemName))
				_itemsUsed.Add(itemName, itemName);
		}

		internal void AddConstraint(string prefix, string tag)
		{
			Pair<string, string> pair = new Pair<string, string>(prefix, tag);

			if (_constraints == null)
				_constraints = new SortedList<Pair<string, string>, string>();

			if (!_constraints.ContainsKey(pair))
				_constraints.Add(pair, tag);			
		}

		/// <summary>
		/// Gets the single item used in a batch
		/// </summary>
		/// <value>The default item. <c>null</c> if there are no items, or multiple items are used</value>
		public string DefaultItemName
		{
			get { return ((_itemsUsed != null) && _itemsUsed.Count == 1) ? _itemsUsed.Keys[0] : null; }
		}

		/// <summary>
		/// Gets the items used.
		/// </summary>
		/// <value>The items used.</value>
		internal IList<string> ItemsUsed
		{
			get { return _itemsUsed.Keys; }
		}

		internal IList<Pair<string, string>> Constraints
		{
			get { return _constraints.Keys; }
		}

		/// <summary>
		/// Gets the <see cref="QQn.TurtleUtils.Tags.BatchItems.TagBatchItem"/> with the specified key.
		/// </summary>
		/// <value></value>
		internal abstract TagBatchItem this[object key]
		{
			get;
		}

		/// <summary>
		/// Gets the number of items.
		/// </summary>
		/// <value>The item count.</value>
		public abstract int ItemCount
		{
			get;
		}

		internal static object CreateTaskItem(string p, string value)
		{
			throw new NotImplementedException();
		}

		internal abstract ICollection<TagBatchItem> AllValues
		{
			get;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class TagBatchDefinition<TKey> : TagBatchDefinition, IEnumerable<TKey>
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
				Type elementType = resultType.GetElementType();
				if (IsTagItemType(elementType))
					_items.Add(key, new TagMultiBatchItem(definition, elementType));
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
		/// Gets all values.
		/// </summary>
		/// <value>All values.</value>
		internal override ICollection<TagBatchItem> AllValues
		{
			get { return _items.Values; }
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

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
		/// </returns>
		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
		{
			return AllKeys.GetEnumerator();
		}

		internal override void Prepare()
		{
			foreach (TagBatchItem item in _items.Values)
				item.PrePrepare(this);

			if (!Prepared)
			{
				ClearPreparation();

				int offset = 0;
				foreach (TagBatchItem item in _items.Values)
					item.Prepare(this, offset++);

				foreach (TagBatchItem item in _items.Values)
					item.PostPrepare(this);
			}
		}

		internal override TagBatchItem this[object key]
		{
			get { return this[(TKey)key]; }
		}

		/// <summary>
		/// Gets the <see cref="QQn.TurtleUtils.Tags.BatchItems.TagBatchItem"/> with the specified key.
		/// </summary>
		/// <value></value>
		internal TagBatchItem this[TKey key]
		{
			get { return _items[key]; }
		}

		/// <summary>
		/// Gets the number of items.
		/// </summary>
		/// <value>The item count.</value>
		public override int ItemCount
		{
			get { return _items.Count; }
		}
	}
}
