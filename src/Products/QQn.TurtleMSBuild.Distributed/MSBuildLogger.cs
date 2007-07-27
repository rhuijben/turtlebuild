using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;

namespace QQn.TurtleMSBuild.Distributed
{
	/// <summary>
	/// Distributed variant of the MSBuildLogger in the TurtleMSBuild assembly. Needed for distributed building in .Net v3.5+
	/// </summary>
	public class MSBuildLogger : QQn.TurtleMSBuild.MSBuildLogger, IForwardingLogger, INodeLogger
	{
		public void Initialize(Microsoft.Build.Framework.IEventSource eventSource, int nodeCount)
		{
			Initialize(eventSource);			
		}

		#region IForwardingLogger Members

		IEventRedirector _redirector;
		public IEventRedirector BuildEventRedirector
		{
			get
			{
				return _redirector;
			}
			set
			{
				_redirector = value;
			}
		}

		#endregion
	}
}
