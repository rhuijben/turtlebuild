using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.Collections;

namespace QQn.TurtleMSBuild
{
	class ProjectItem
	{
		readonly IDictionary _metaData;
		readonly string _name;
		readonly string _include;

		public ProjectItem(string itemName, ITaskItem taskItem)
		{
			if (string.IsNullOrEmpty(itemName))
				throw new ArgumentNullException("itemName");
			else if (taskItem == null)
				throw new ArgumentNullException("taskItem");

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

		public bool HasMetadata(string metaDataName)
		{
			if (string.IsNullOrEmpty(metaDataName))
				throw new ArgumentNullException("metaDataName");

			return _metaData.Contains(metaDataName);
		}

		public string GetMetadata(string metaDataName)
		{
			if (string.IsNullOrEmpty(metaDataName))
				throw new ArgumentNullException("metaDataName");

			string v = (string)_metaData[metaDataName];

			if (v == null)
				throw new ArgumentOutOfRangeException("metaDataName", metaDataName, "Meta data not available");

			return v;
		}
	}
}
