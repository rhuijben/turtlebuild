using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagEnvironment : TagContext
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
		public TagPropertyCollection Properties
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
			return RunBatchInternal(definition);
		
		}

		private IEnumerable<TagBatchInstance<TKey>> RunBatchInternal<TKey>(TagBatchDefinition<TKey> definition)
			where TKey : class
		{
			// TODO: Create results
			yield break;
		}
	}
}
