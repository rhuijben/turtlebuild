using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags
{
	/// <summary>
	/// 
	/// </summary>
	public class TagContext
	{
		readonly TagEnvironment _environment;

		/// <summary>
		/// Initializes a new instance of the <see cref="TagContext"/> class.
		/// </summary>
		/// <param name="environment">The environment.</param>
		public TagContext(TagEnvironment environment)
		{
			if (environment == null)
				throw new ArgumentNullException("environment");

			_environment = environment;
		}

		internal TagContext()
		{
			_environment = (TagEnvironment)this;
		}

		/// <summary>
		/// Evaluates the condition.
		/// </summary>
		/// <param name="condition">The condition.</param>
		/// <returns></returns>
		public bool EvaluateCondition(string condition)
		{
			return true;
		}
	}
}
