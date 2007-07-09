using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
//using Microsoft.Build.BuildEngine;
using System.IO;
using Microsoft.Build.Framework;
using QQn.TurtleLogger;

namespace QQn.TurtleMSBuild
{
	class BuildProject
	{
		readonly string _projectFile;
		readonly string _targetNames;
		readonly IEnumerable _properties;
		readonly IEnumerable _items;
		readonly BuildParameters _parameters;

		public BuildProject(string projectFile, string targetNames, IEnumerable properties, IEnumerable items, BuildParameters parameters)
		{
			if (string.IsNullOrEmpty(projectFile))
				throw new ArgumentNullException("projectFile");
			else if (targetNames == null)
				throw new ArgumentNullException("targetNames");
			else if (properties == null)
				throw new ArgumentNullException("properties");
			else if (items == null)
				throw new ArgumentNullException("items");
			else if (parameters == null)
				throw new ArgumentNullException("parameters");

			_projectFile = Path.GetFullPath(projectFile);
			_targetNames = targetNames;
			_properties = properties;
			_items = items;
			_parameters = parameters;
		}

		public string ProjectFile
		{
			get { return _projectFile; }
		}

		public string TargetName
		{
			get { return _targetNames; }
		}

		internal IEnumerable PropertiesEnumerable
		{
			get { return _properties; }
		}

		internal IEnumerable ItemsEnumerable
		{
			get { return _items; }
		}

		SortedList<string, string> _buildProperties;

		List<ProjectItem> _buildItems = new List<ProjectItem>();

		public void Refresh()
		{
			SortedList<string, string> buildProperties = new SortedList<string, string>();
			foreach (DictionaryEntry o in PropertiesEnumerable)
			{
				buildProperties[(string)o.Key] = (string)o.Value;
			}

			List<ProjectItem> buildItems = new List<ProjectItem>();
			foreach (DictionaryEntry o in ItemsEnumerable)
			{
				buildItems.Add(new ProjectItem(this, (string)o.Key, (ITaskItem)o.Value));
			}

			_buildProperties = buildProperties;
			_buildItems = buildItems;

			// Clear properties
			_outDir = null;
			_configuration = null;
		}

		public SortedList<string, string> BuildProperties
		{
			get { return _buildProperties; }
		}

		public List<ProjectItem> BuildItems
		{
			get { return _buildItems; }
		}

		string _projectPath;
		public string ProjectPath
		{
			get { return _projectPath ?? (_projectPath = Path.GetDirectoryName(ProjectFile)); }
		}

		string _projectName;
		public string ProjectName
		{
			get { return _projectName ?? (_projectName = Path.GetFileNameWithoutExtension(ProjectFile)); }
		}

		string _outDir;
		public string OutDir
		{
			get { return _outDir ?? (_outDir = GetProperty("OutDir")); }
		}

		string _intermediateOutputPath;
		public string IntermediateOutputPath
		{
			get { return _intermediateOutputPath ?? (_intermediateOutputPath = GetProperty("IntermediateOutputPath")); }
		}
		

		string _configuration;
		public string Configuration
		{
			get { return _configuration ?? (_configuration = BuildProperties["Configuration"]); }
		}

		public string GetProperty(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			if (BuildProperties == null)
				return null;

			string r;

			if (BuildProperties.TryGetValue(key, out r))
				return r;
			else
				return null;
		}

		public BuildParameters Parameters
		{
			get { return _parameters; }
		}

		internal string MakeRelativePath(string include)
		{

			Uri includeUri = new Uri(Path.GetFullPath(include).Replace(Path.DirectorySeparatorChar, '/'));
			Uri projectUri = new Uri(ProjectFile.Replace(Path.DirectorySeparatorChar, '/'));
			Uri relUri = projectUri.MakeRelativeUri(includeUri);

			return relUri.ToString().Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		}
	}
}
