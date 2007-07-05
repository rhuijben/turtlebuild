using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public class MultiStreamCreateArgs
	{
		//bool _streamLayout;
		int _maxSubStreams;
		VerificationMode _verification;
		
		/// <summary>
		/// 
		/// </summary>
		public MultiStreamCreateArgs()
		{
			MaximumNumberOfStreams = 32;
		}

		/// <summary>
		/// Gets or sets the maximum number of substreams
		/// </summary>
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
		/// 
		/// </summary>
		public VerificationMode Verification
		{
			get { return _verification; }
			set { _verification = value; }
		}
	}
}
