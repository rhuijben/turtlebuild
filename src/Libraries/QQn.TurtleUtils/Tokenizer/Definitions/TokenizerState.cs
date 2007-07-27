using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.ComponentModel;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// Contains the parser state while tokanizing
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TokenizerState<T> : Hashtable, IDisposable
		where T : class, new()
	{
		readonly T _instance;
		readonly TokenizerDefinition _definition;
		readonly TokenizerArgs _args;
		ISupportInitialize _initialize;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerState&lt;T&gt;"/> class.
		/// </summary>
		/// <param name="instance">The instance.</param>
		/// <param name="definition">The definition.</param>
		/// <param name="args">The args.</param>
		/// <remarks>Calls <see cref="ISupportInitialize.BeginInit"/> if the instance implements <see cref="ISupportInitialize"/></remarks>
		public TokenizerState(T instance, TokenizerDefinition definition, TokenizerArgs args)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
			else if (definition == null)
				throw new ArgumentNullException("definition");
			else if (args == null)
				throw new ArgumentNullException("args");

			_instance = instance;
			_definition = definition;
			_args = args;
			_initialize = instance as ISupportInitialize;

			if (_initialize != null)
				_initialize.BeginInit();
		}

		/// <summary>
		/// Gets the instance.
		/// </summary>
		/// <value>The instance.</value>
		public T Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// Gets the definition.
		/// </summary>
		/// <value>The definition.</value>
		public TokenizerDefinition Definition
		{
			get { return _definition; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is complete.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is complete; otherwise, <c>false</c>.
		/// </value>
		public bool IsComplete
		{
			get
			{
				// TODO: Verify required arguments

				return true;
			}
		}

		/// <summary>
		/// Gets the tokenizer args.
		/// </summary>
		/// <value>The tokenizer args.</value>
		public TokenizerArgs TokenizerArgs
		{
			get { return _args; }
		}

		#region IDisposable Members

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		/// <remarks>Calls <see cref="ISupportInitialize.EndInit"/> if the instance implements <see cref="ISupportInitialize"/></remarks>
		public void Dispose()
		{
			ISupportInitialize init = _initialize;
			if (init != null)
			{
				_initialize = null;

				init.EndInit();
			}
		}

		#endregion
	}
}
