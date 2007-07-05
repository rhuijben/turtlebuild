using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class TokenizerState<T> : Hashtable
		where T : class, new()
	{
		readonly T _instance;
		readonly TokenizerDefinition _definition;
		readonly TokenizerArgs _args;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="definition"></param>
		/// <param name="args"></param>
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
		}

		/// <summary>
		/// 
		/// </summary>
		public T Instance
		{
			get { return _instance; }
		}

		/// <summary>
		/// 
		/// </summary>
		public TokenizerDefinition Definition
		{
			get { return _definition; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsComplete
		{
			get
			{
				// TODO: Verify required arguments

				return true;
			}
		}
	}
}
