using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class TBLogContainer
	{
		TBLogConfiguration _configuration;
		TBLogFile _file;
		#region IHasFullPath Members

		/// <summary>
		/// Gets the full path.
		/// </summary>
		/// <value>The full path.</value>
		protected internal string BasePath
		{
			get
			{
				if (Configuration != null)
					return Configuration.BasePath;
				else
					return LogFile.BasePath;
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public TBLogConfiguration Configuration
		{
			get { return _configuration; }
			internal set { _configuration = value; }
		}

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public TBLogFile LogFile
		{
			get { return _file ?? _configuration.LogFile; }
			internal set { _file = value; }
		}
	}
}
