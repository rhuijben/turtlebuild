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
		/// Runs the batch, checking conditions
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public IEnumerable<TagBatchInstance<TKey>> RunBatch<TKey>(TagBatchDefinition<TKey> definition)
			where TKey : class
		{
			return RunBatch(definition, true);
		}

		/// <summary>
		/// Runs the batch, optionally checking conditions
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <param name="checkConditions">if set to <c>true</c> [check conditions].</param>
		/// <returns></returns>
		public IEnumerable<TagBatchInstance<TKey>> RunBatch<TKey>(TagBatchDefinition<TKey> definition, bool checkConditions)
			where TKey: class
		{
			if(definition == null)
				throw new ArgumentNullException("definition");

			// We delegate to an implementation fuction to allow validating the parameters before the first for each step

			definition.Prepare();
			// TODO: Check if we have a single instance result or a result list
			//definition.

			IList<string> itemsUsed = definition.ItemsUsed;

			return RunBatchInternal(definition, checkConditions);
		}

		/// <summary>
		/// Creates a batch instance over a single item
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <param name="checkConditions">if set to <c>true</c> [check conditions].</param>
		/// <returns></returns>
		private IEnumerable<TagBatchInstance<TKey>> RunBatchInternal<TKey>(TagBatchDefinition<TKey> definition, bool checkConditions)
			where TKey : class
		{
			AutoKeyedCollection<string, TagItemCollection> restLists = new AutoKeyedCollection<string,TagItemCollection>(StringComparer.OrdinalIgnoreCase);
			AutoKeyedCollection<string, TagItemCollection> currentLists = new AutoKeyedCollection<string, TagItemCollection>(StringComparer.OrdinalIgnoreCase);

			int nLeft = 0;
			foreach (string itemName in definition.ItemsUsed)
			{
				TagItemCollection tt = Items.GetAllByName(itemName);
				restLists.Add(tt);
				currentLists.Add(tt.Clone(false));
				nLeft += tt.Count;
			}

			// Create a list of valid items
			IList<Pair<string, string>> constraints = definition.Constraints;			
			TagBatchInstance<TKey> instance = new TagBatchInstance<TKey>(this, definition);
			do
			{
				string[] constraintValues = new string[constraints.Count];

				for (int iList = 0; iList < restLists.Count; iList++)
				{
					TagItemCollection rest = restLists[iList];
					TagItemCollection current = currentLists[iList];
					string listName = restLists[iList].Name; 
					
					for (int iItem = 0; iItem < rest.Count; iItem++)
					{
						TagItem ti = rest[iItem];
						int n = 0;
						bool next = false;

						foreach (Pair<string, string> p in constraints)
						{
							if (!string.IsNullOrEmpty(p.First) && !StringComparer.OrdinalIgnoreCase.Equals(p.First, listName))
								continue;

							string v = ti.ExpandedKey(p.Second);
							if ((object)constraintValues[n] == null)
								constraintValues[n++] = v;
							else
								if (!StringComparer.OrdinalIgnoreCase.Equals(v, constraintValues[n++]))
								{
									next = true;
									break;
								}
						}
						if (next)
							continue;
						else
						{
							rest.RemoveAt(iItem--);
							current.Add(ti);
							nLeft--;
						}
					}
				}

				// At least one item was added to current
				instance.Fill(currentLists, constraintValues);

				if (!checkConditions || instance.ConditionResult())
					yield return instance;

				foreach (TagItemCollection current in currentLists)
				{
					current.Clear();
				}
			}
			while (nLeft > 0);
		}

		/// <summary>
		/// Runs the single item batch.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <param name="checkConditions">if set to <c>true</c> [check conditions].</param>
		/// <returns></returns>
		private IEnumerable<TagBatchInstance<TKey>> RunSingleItemBatch<TKey>(TagBatchDefinition<TKey> definition, bool checkConditions)
			where TKey : class
		{
			TagBatchInstance<TKey> instance = new TagBatchInstance<TKey>(this, definition);			

			/*instance.Fill(new TagItemCollection[0], null);

			if (checkConditions && !instance.ConditionResult())*/
				yield break;

			yield return instance;
		}

		/// <summary>
		/// Runs the matrix batch internal.
		/// </summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <param name="definition">The definition.</param>
		/// <param name="checkConditions">if set to <c>true</c> [check conditions].</param>
		/// <returns></returns>
		private IEnumerable<TagBatchInstance<TKey>> RunMatrixBatchInternal<TKey>(TagBatchDefinition<TKey> definition, bool checkConditions)
			where TKey : class
		{
			yield break;
		}
	}
}
