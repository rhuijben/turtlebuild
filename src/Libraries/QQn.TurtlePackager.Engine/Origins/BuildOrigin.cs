using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleBuildUtils.Files.TBLog;

namespace QQn.TurtlePackager.Origins
{
	public class BuildOrigin : Origin
	{
		readonly TBLogFile _log;

		public BuildOrigin(TBLogFile logFile)
		{
			if (logFile == null)
				throw new ArgumentNullException("logFile");

			_log = logFile;
		}

		public TBLogFile LogFile
		{
			get { return _log; }
		}
	}
}
