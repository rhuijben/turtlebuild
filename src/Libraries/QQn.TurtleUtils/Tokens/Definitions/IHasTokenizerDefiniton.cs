using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.Tokens.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public interface IHasTokenDefinition
	{
		/// <summary>
		/// Gets the tokenizer definition.
		/// </summary>
		/// <param name="args">The args.</param>
		/// <returns></returns>
		TokenizerDefinition GetTokenizerDefinition(TokenizerArgs args);
	}
}
