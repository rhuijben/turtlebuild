using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Collections;
using QQn.TurtleUtils.Tokenizer;
using QQn.TurtleLogger;

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
				return; // Already building

			if (e.Properties != null)
			{
				// e.Properties = null in current beta version of MSBuild 3.5 when building parallel

				building.Add(e.ProjectFile, new BuildProject(e.ProjectFile, e.TargetNames, e.Properties, e.Items, _settings));
			}
		}

		void ProjectBuildFinished(object sender, ProjectFinishedEventArgs e)
		{
			if (!building.ContainsKey(e.ProjectFile))
				return; // Can't finish if not building; probably reference project

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
		BuildParameters _settings = new BuildParameters();

		public string Parameters
		{
			get { return _parameters; }
			set 
			{
				BuildParameters settings;
				if (!Tokenizer.TryParseConnectionString(value, out settings))
					throw new ArgumentException("Invalid setting string");
				_parameters = value;
				_settings = settings;
			}
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
