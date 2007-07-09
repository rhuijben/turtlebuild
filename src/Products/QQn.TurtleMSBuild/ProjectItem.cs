using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.Collections;
using System.IO;

namespace QQn.TurtleMSBuild
{
	class ProjectItem
	{
		readonly IDictionary _metaData;
		readonly BuildProject _project;
		readonly string _name;
		readonly string _include;

		public ProjectItem(BuildProject project, string itemName, ITaskItem taskItem)
		{
			if (project == null)
				throw new ArgumentNullException("project");
			else if (string.IsNullOrEmpty(itemName))
				throw new ArgumentNullException("itemName");
			else if (taskItem == null)
				throw new ArgumentNullException("taskItem");

			_project = project;
			_name = itemName;
			_metaData = taskItem.CloneCustomMetadata();
			_include = taskItem.ItemSpec;
		}

		public string Name
		{
			get { return _name; }
		}

		public string Include
		{
			get { return _include; }
		}

		public bool TryTransformMetaData(string metaDataName, out string value)
		{
			int n;
			switch (metaDataName)
			{
				case "FullPath":
					value = Path.Combine(_project.ProjectPath, Include);
					return true;
				case "RootDir":
					value = Path.GetPathRoot(Path.Combine(_project.ProjectPath, Include));
					return true;
				case "Filename":
					value = Path.GetFileName(Include);
					return true;
				case "Extension":
					value = Path.GetExtension(Include);
					return true;
				case "RelativeDir":
					n = Include.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
					value = (n >= 0) ? Include.Substring(0, n+1) : "";
					return true;
				case "Directory":
					value = Path.Combine(_project.ProjectPath, Include);
					n = value.LastIndexOfAny(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
					value = (n >= 0) ? Include.Substring(0, n + 1) : "";
					value = value.Substring(Path.GetPathRoot(value).Length);
					return true;
				case "RecursiveDir":
					value = "";
					return true;
				case "Identity":
					value = Include;
					return true;
				case "ModifiedTime":
					value = new DirectoryInfo(Path.Combine(_project.ProjectPath, Include)).LastWriteTime.ToString("u");
					return true;
				case "CreatedTime":
					value = new DirectoryInfo(Path.Combine(_project.ProjectPath, Include)).CreationTime.ToString("u");
					return true;
				case "AccessedTime":
					value = new DirectoryInfo(Path.Combine(_project.ProjectPath, Include)).LastAccessTime.ToString("u");
					return true;
				default:
					value = null;
					return false;
			}
		}

		public bool HasMetadata(string metaDataName)
		{
			if (string.IsNullOrEmpty(metaDataName))
				throw new ArgumentNullException("metaDataName");

			string result;

			return _metaData.Contains(metaDataName) || TryTransformMetaData(metaDataName, out result);
		}

		public string GetMetadata(string metaDataName)
		{
			if (string.IsNullOrEmpty(metaDataName))
				throw new ArgumentNullException("metaDataName");

			string value;

			if (TryTransformMetaData(metaDataName, out value))
				return value;

			value = (string)_metaData[metaDataName];

			if (value == null)
				throw new ArgumentOutOfRangeException("metaDataName", metaDataName, "Meta data not available");

			return value;
		}
	}
}
