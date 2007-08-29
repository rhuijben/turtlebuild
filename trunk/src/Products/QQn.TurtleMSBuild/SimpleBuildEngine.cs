using System;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using System.Diagnostics;

namespace QQn.TurtleMSBuild
{
	class SimpleBuildEngine : IBuildEngine
	{
		#region IBuildEngine Members

		public bool BuildProjectFile(string projectFileName, string[] targetNames, System.Collections.IDictionary globalProperties, System.Collections.IDictionary targetOutputs)
		{
			return false;
		}

		public int ColumnNumberOfTaskNode
		{
			get { return 0; }
		}

		public bool ContinueOnError
		{
			get { return false; }
		}

		public int LineNumberOfTaskNode
		{
			get { return 0; }
		}

		public void LogCustomEvent(CustomBuildEventArgs e)
		{
			Debug.WriteLine(e.Message);
		}

		public void LogErrorEvent(BuildErrorEventArgs e)
		{
			Debug.WriteLine(e.Message);
		}

		public void LogMessageEvent(BuildMessageEventArgs e)
		{
			Debug.WriteLine(e.Message);
		}

		public void LogWarningEvent(BuildWarningEventArgs e)
		{
			Debug.WriteLine(e.Message);
		}

		public string ProjectFileOfTaskNode
		{
			get { return ""; }
		}

		#endregion
	}
}
