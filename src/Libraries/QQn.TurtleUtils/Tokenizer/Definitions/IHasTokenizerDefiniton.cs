using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	/// <summary>
	/// 
	/// </summary>
	public interface IHasTokenDefinition
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		TokenizerDefinition GetTokenizerDefinition(TokenizerArgs args);
	}
}
