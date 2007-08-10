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
	/// <summary>
	/// MSbuild logger which logs MSBuild results to a .tbProj file
	/// </summary>
	public class MSBuildLogger : ILogger
	{
		readonly SortedList<string, BuildProject> completed = new SortedList<string, BuildProject>();
		readonly SortedList<string, BuildProject> building = new SortedList<string, BuildProject>();
		string _parameters;
		BuildParameters _settings = new BuildParameters();
		int _nodeId;

		/// <summary>
		/// Initializes a new instance of the <see cref="MSBuildLogger"/> class.
		/// </summary>
		public MSBuildLogger()
		{
			
		}

		/// <summary>
		/// Gets the parsed build parameters.
		/// </summary>
		/// <value>The build parameters.</value>
		protected BuildParameters BuildParameters
		{
			get { return _settings ?? (_settings = new BuildParameters()); }
		}

		#region ILogger Members
		/// <summary>
		/// Subscribes loggers to specific events. This method is called when the logger is registered with the build engine, before any events are raised.
		/// </summary>
		/// <param name="eventSource">The events available to loggers.</param>
		public void Initialize(IEventSource eventSource)
		{
			if (eventSource == null)
				throw new ArgumentNullException("eventSource");

			eventSource.ProjectStarted += new ProjectStartedEventHandler(ProjectBuildStarted);
			eventSource.ProjectFinished += new ProjectFinishedEventHandler(ProjectBuildFinished);
			eventSource.TaskFinished += new TaskFinishedEventHandler(ProjectTaskFinished);
		}

		void ProjectTaskFinished(object sender, TaskFinishedEventArgs e)
		{
			if (e.TaskName == "VCBuild")
			{
				BuildProject project;
				if (building.TryGetValue(e.ProjectFile, out project))
				{
					project.UsedVCBuild = true;
				}
			}			
		}

		/// <summary>
		/// Handles the <see cref="IEventSource.ProjectStarted"/> event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Microsoft.Build.Framework.ProjectStartedEventArgs"/> instance containing the event data.</param>
		void ProjectBuildStarted(object sender, ProjectStartedEventArgs e)
		{
			if (building.ContainsKey(e.ProjectFile))
				return; // Already building

			if (e.Properties != null)
			{
				// e.Properties = null if the project is build parallel (.Net v3.5+); Use the distributed logger instead
				building.Add(e.ProjectFile, new BuildProject(e.ProjectFile, e.TargetNames, e.Properties, e.Items, _settings));
			}
		}

		/// <summary>
		/// Handles the <see cref="IEventSource.ProjectFinished"/> event
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="Microsoft.Build.Framework.ProjectFinishedEventArgs"/> instance containing the event data.</param>
		void ProjectBuildFinished(object sender, ProjectFinishedEventArgs e)
		{
			if (!building.ContainsKey(e.ProjectFile))
				return; // Can't finish if not building; probably reference project

			BuildProject bp = building[e.ProjectFile];
			building.Remove(e.ProjectFile);

			bool isBuild = false;

			foreach (string target in bp.BuildTargetName.Split(';'))
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

		/// <summary>
		/// Gets or sets the user-defined parameters of the logger.
		/// </summary>
		/// <value></value>
		/// <returns>The logger parameters.</returns>
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


		/// <summary>
		/// Releases the resources allocated to the logger at the time of initialization or during the build. This method is called when the logger is unregistered from the engine, after all events are raised. A host of MSBuild typically unregisters loggers immediately before quitting.
		/// </summary>
		public void Shutdown()
		{
			//throw new Exception("The method or operation is not implemented.");
		}

		LoggerVerbosity _verbosity;

		/// <summary>
		/// Gets or sets the level of detail to show in the event log.
		/// </summary>
		/// <value></value>
		/// <returns>One of the enumeration values. The default is <see cref="F:Microsoft.Build.Framework.LoggerVerbosity.Normal"></see>.</returns>
		public LoggerVerbosity Verbosity
		{
			get { return _verbosity; }
			set { _verbosity = value; }
		}

		/// <summary>
		/// Gets or sets the node id.
		/// </summary>
		/// <value>The node id.</value>
		public int NodeId
		{
			get { return _nodeId; }
			set { _nodeId = value; }
		}
	}
}
