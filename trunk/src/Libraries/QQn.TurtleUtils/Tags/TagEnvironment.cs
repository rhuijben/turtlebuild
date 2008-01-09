using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Items;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public sealed class TagEnvironment : TagContext
	{
		readonly TagPropertyCollection _properties;
		readonly TagItemCollection _items;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagEnvironment"/> class.
		/// </summary>
		public TagEnvironment()
		{
			_properties = new TagPropertyCollection(this);
			_items = new TagItemCollection(this);
		}

		/// <summary>
		/// Gets the properties collection
		/// </summary>
		/// <value>The properties collection.</value>
		[TokenGroup("property")]
		public override TagPropertyCollection Properties
		{
			get { return _properties; }
		}

		/// <summary>
		/// Gets the items collection
		/// </summary>
		/// <value>The items collection.</value>
		[TokenGroup("item")]
		public TagItemCollection Items
		{
			get { return _items; }
		}

		/// <summary>
		/// Runs the batch.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public IEnumerable<TagBatchInstance<TKey>> RunBatch<TKey>(TagBatchDefinition<TKey> definition)
			where TKey: class
		{
			if(definition == null)
				throw new ArgumentNullException("definition");

			// We delegate to an implementation fuction to allow validating the parameters before the first for each step

			definition.Prepare();
			// TODO: Check if we have a single instance result or a result list
			//definition.

			IList<string> itemsUsed = definition.ItemsUsed;

			if (itemsUsed.Count == 1) // Most common case
				return RunBatchInternal(definition);
			else if (itemsUsed.Count == 0)
				return RunSingleItemBatch(definition);
			else
				return RunMatrixBatchInternal(definition);
		}		

		/// <summary>
		/// Creates a batch instance over a single item
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <param name="definition"></param>
		/// <returns></returns>
		private IEnumerable<TagBatchInstance<TKey>> RunBatchInternal<TKey>(TagBatchDefinition<TKey> definition)
			where TKey : class
		{
			string tagName = definition.DefaultItemName;

			if(string.IsNullOrEmpty(tagName))
				throw new InvalidOperationException(); // Must be set

			// Create a list of valid items
			TagItemCollection rest = new TagItemCollection(this);
			TagItemCollection current = new TagItemCollection(this);
			foreach (TagItem item in Items)
			{
				if (string.Equals(item.Name, tagName, StringComparison.OrdinalIgnoreCase))
					rest.Add(item);
			}

			IList<Pair<string, string>> constraints = definition.Constraints;
			string[] constraintValues = new string[constraints.Count];
			ICollection<TagItem>[] instances = new ICollection<TagItem>[1];
			while(rest.Count > 0)
			{
				TagBatchInstance<TKey> instance = new TagBatchInstance<TKey>(this, definition);
				bool fill = true;
				for (int i = 0; i < rest.Count; i++)
				{
					TagItem ti = rest[i];
					int n = 0;
					bool next = false;
					foreach (Pair<string, string> p in constraints)
					{
						string v = ti.ExpandedKey(p.Second);
						if (fill)
							constraintValues[n++] = v;
						else
							if (!string.Equals(v, constraintValues[n++], StringComparison.OrdinalIgnoreCase))
							{
								next = true;
								break;
							}
					}
					if(next)
						continue;
					else
					{
						rest.RemoveAt(i);
						current.Add(ti);
						i--;
					}

					fill = false;
				}

				// At least one item was added to current
				instance.Fill(new TagItemCollection[] { current });
				yield return instance;

				current.Clear();
			}
			while (rest.Count > 0);
		}

		private IEnumerable<TagBatchInstance<TKey>> RunSingleItemBatch<TKey>(TagBatchDefinition<TKey> definition)
			where TKey : class
		{
			TagBatchInstance<TKey> instance = new TagBatchInstance<TKey>(this, definition);

			instance.Fill(null);

			yield return instance;
		}

		private IEnumerable<TagBatchInstance<TKey>> RunMatrixBatchInternal<TKey>(TagBatchDefinition<TKey> definition)
			where TKey : class
		{
			yield break;
		}
	}
}
