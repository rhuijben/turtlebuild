using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.Collections;
using System.IO;

namespace QQn.TurtleMSBuild
{
	class ProjectItem : ITaskItem
	{
		readonly Dictionary<string, string> _metaData;
		readonly BuildProject _project;

		internal BuildProject Project
		{
			get { return _project; }
		} 

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
			_metaData = new Dictionary<string, string>();
			taskItem.CopyMetadataTo(this);
			_include = taskItem.ItemSpec.Replace("%20", "");
		}

		public string Name
		{
			get { return _name; }
		}

		/// <summary>
		/// Gets the include.
		/// </summary>
		/// <value>The include.</value>
		public string Include
		{
			get { return _include; }
		}

		/// <summary>
		/// Gets the full path.
		/// </summary>
		/// <value>The full path.</value>
		public string FullPath
		{
			get { return Path.Combine(_project.ProjectPath, Include); }
		}

		/// <summary>
		/// Gets the filename.
		/// </summary>
		/// <value>The filename.</value>
		public string Filename
		{
			get { return Path.GetFileName(Include); }
		}

		/// <summary>
		/// Gets the extension.
		/// </summary>
		/// <value>The extension.</value>
		public string Extension
		{
			get { return Extension; }
		}

		public string QNames
		{
			get
			{
				StringBuilder sb = new StringBuilder();
				foreach (string v in _metaData.Keys)
				{
					sb.Append(v);
					sb.Append("!");
				}
				return sb.ToString();
			}
		}

		public bool TryGetMetaData(string metaDataName, out string value)
		{
			if (TryTransformMetaData(metaDataName, out value))
				return true;
			else if(_metaData.TryGetValue(metaDataName, out value))
				return true;

			return false;
		}

		/// <summary>
		/// Tries to get the generated metadata item.
		/// </summary>
		/// <param name="metaDataName">Name of the meta data.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		bool TryTransformMetaData(string metaDataName, out string value)
		{
			int n;
			switch (metaDataName)
			{
				case "FullPath":
					value = FullPath;
					return true;
				case "RootDir":
					value = Path.GetPathRoot(Path.Combine(_project.ProjectPath, Include));
					return true;
				case "Filename":
					value = Filename;
					return true;
				case "Extension":
					value = Extension;
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

			return _metaData.ContainsKey(metaDataName) || TryTransformMetaData(metaDataName, out result);
		}

		public string GetMetadata(string metaDataName)
		{
			string result;

			if (!TryGetMetaData(metaDataName, out result))
				throw new ArgumentOutOfRangeException("metaDataName", metaDataName, "Meta data not available");

			return result;
		}

		#region ITaskItem Members

		IDictionary ITaskItem.CloneCustomMetadata()
		{
			throw new NotImplementedException();
		}

		void ITaskItem.CopyMetadataTo(ITaskItem destinationItem)
		{
			throw new NotImplementedException();
		}

		string ITaskItem.GetMetadata(string metadataName)
		{
			string result;
			if(TryTransformMetaData(metadataName, out result))
				return result;

			if(_metaData.TryGetValue(metadataName, out result))
				return result;

			return null;
		}

		string ITaskItem.ItemSpec
		{
			get { return _include; }
			set
			{
				if (_include == value)
					return;
				throw new InvalidOperationException();
			}
		}

		int ITaskItem.MetadataCount
		{
			get { return _metaData.Count; }
		}

		ICollection ITaskItem.MetadataNames
		{
			get { return _metaData.Keys; }
		}

		void ITaskItem.RemoveMetadata(string metadataName)
		{
			_metaData.Remove(metadataName);
		}

		void ITaskItem.SetMetadata(string metadataName, string metadataValue)
		{
			_metaData[metadataName] = metadataValue;
		}

		#endregion
	}
}
