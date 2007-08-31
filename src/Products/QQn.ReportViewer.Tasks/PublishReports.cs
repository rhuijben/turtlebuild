using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace QQn.ReportViewer.Tasks
{
	public class PublishReports : Microsoft.Build.Utilities.Task
	{
		ITaskItem[] _reports;
		String _reportServer;
		String _targetFolder;
		bool _overwrite;
		bool _dontUseDefaultCredentials;

		[Required]
		public ITaskItem[] Reports
		{
			get { return _reports; }
			set { _reports = value; }
		}

		[Required]
		public String ReportServerUrl
		{
			get { return _reportServer; }
			set { _reportServer = value; }
		}

		public String TargetFolder
		{
			get { return _targetFolder; }
			set { _targetFolder = value; }
		}

		public bool Overwrite
		{
			get { return _overwrite; }
			set { _overwrite = value; }
		}

		public bool UseDefaultCredentials
		{
			get { return !_dontUseDefaultCredentials; }
			set { _dontUseDefaultCredentials = value; }
		}

		public override bool Execute()
		{
#if DEBUG
#endif
			return true;
		}
	}
}
