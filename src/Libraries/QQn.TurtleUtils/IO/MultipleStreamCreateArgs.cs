using System;
using System.Collections.Generic;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Argument for creating a <see cref="MultipleStreamReader"/> or <see cref="MultipleStreamWriter"/>
	/// </summary>
	public class MultipleStreamCreateArgs
	{
		//bool _streamLayout;
		int _maxSubStreams;
		VerificationMode _verification;

		/// <summary>
		/// Initializes a new instance of the <see cref="MultipleStreamCreateArgs"/> class.
		/// </summary>
		public MultipleStreamCreateArgs()
		{
			MaximumNumberOfStreams = 32;
		}

		/// <summary>
		/// Gets or sets the maximum number of substreams
		/// </summary>
		/// <value>The maximum number of streams.</value>
		public int MaximumNumberOfStreams
		{
			get { return _maxSubStreams; }
			set 
			{
				if (value < 1)
					_maxSubStreams = 1;
				else
					_maxSubStreams = value;
			}
		}

		/// <summary>
		/// Gets or sets the verification mode.
		/// </summary>
		/// <value>The verification mode.</value>
		public VerificationMode VerificationMode
		{
			get { return _verification; }
			set { _verification = value; }
		}
	}
}
