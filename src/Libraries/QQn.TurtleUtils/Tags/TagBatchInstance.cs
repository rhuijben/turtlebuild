using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TagBatchInstance
	{
		readonly TagBatchDefinition _definition;

		internal TagBatchInstance(TagBatchDefinition definition)
		{
			if(definition == null)
				throw new ArgumentNullException("definition");

			_definition = definition;
		}


		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value></value>
		public object this[object key]
		{
			get { return GetValue(key); }
		}

		/// <summary>
		/// Gets the value of the specified key
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected abstract object GetValue(object key);

		/// <summary>
		/// Gets the batch definition.
		/// </summary>
		/// <value>The batch definition.</value>
		public TagBatchDefinition BatchDefinition
		{
			get { return _definition; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TKey">The type of the key.</typeparam>
	public class TagBatchInstance<TKey> : TagBatchInstance
		where TKey: class
	{
		readonly TagBatchDefinition<TKey> _definition;
		internal TagBatchInstance(TagBatchDefinition<TKey> definition)
			: base(definition)
		{
			_definition = definition;
		}


		/// <summary>
		/// Gets the <see cref="System.Object"/> with the specified key.
		/// </summary>
		/// <value></value>
		public object this[TKey key]
		{
			get
			{
				return null;
			}
		}

		/// <summary>
		/// Conditions the result.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		public bool ConditionResult(TKey key)
		{
			return true;
		}

		/// <summary>
		/// Gets the value of the specified key
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns></returns>
		protected override object GetValue(object key)
		{
			return this[(TKey)key];
		}

		/// <summary>
		/// Gets the batch definition.
		/// </summary>
		/// <value>The batch definition.</value>
		public new TagBatchDefinition<TKey> BatchDefinition
		{
			get { return _definition; }
		}
	}
}
