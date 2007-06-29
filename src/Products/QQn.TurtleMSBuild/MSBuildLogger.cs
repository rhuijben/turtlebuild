using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Collections;

namespace QQn.TurtleMSBuild
{
	public class MSBuildLogger : ILogger
	{
		readonly SortedList<string, BuildProject> completed = new SortedList<string, BuildProject>();
		readonly SortedList<string, BuildProject> building = new SortedList<string, BuildProject>();

		public MSBuildLogger()
		{
		}

		#region ILogger Members
		public void Initialize(IEventSource eventSource)
		{
			if (eventSource == null)
				throw new ArgumentNullException("eventSource");

			eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectBuildStarted);
			eventSource.ProjectFinished += new ProjectFinishedEventHandler(ProjectBuildFinished);
		}

		void ProjectBuildStarted(object sender, ProjectStartedEventArgs e)
		{
			if (building.ContainsKey(e.ProjectFile))
				return;
				//throw new InvalidOperationException("Project already building");

			if (e.Properties == null)
			{
				//Console.WriteLine("X-Started {0}", e.ProjectFile);
			}
			else
			{
				//Console.WriteLine("Started {0}", e.ProjectFile);

				building.Add(e.ProjectFile, new BuildProject(e.ProjectFile, e.TargetNames, e.Properties, e.Items));
			}
		}

		void ProjectBuildFinished(object sender, ProjectFinishedEventArgs e)
		{
			if (!building.ContainsKey(e.ProjectFile))
			{
				return;
				//throw new InvalidOperationException("Project not building");
			}

			BuildProject bp = building[e.ProjectFile];
			building.Remove(e.ProjectFile);

			bool isBuild = false;

			foreach (string target in bp.TargetName.Split(';'))
			{
				if (string.IsNullOrEmpty(target))
					isBuild = true;
				else if (string.Equals(target, "build", StringComparison.InvariantCultureIgnoreCase))
					isBuild = true;

				if (isBuild)
					break;
			}

			if (isBuild && !completed.ContainsKey(e.ProjectFile))
			{
				bp.Refresh();
				completed.Add(e.ProjectFile, bp);

				TurtleFilter.Execute(bp);
			}
		}

		#endregion

		string _parameters;

		public string Parameters
		{
			get { return _parameters; }
			set { _parameters = value; }
		}
		

		public void Shutdown()
		{
			//throw new Exception("The method or operation is not implemented.");
		}

		LoggerVerbosity _verbosity;

		public LoggerVerbosity Verbosity
		{
			get { return _verbosity; }
			set { _verbosity = value; }
		}		
	}
}
