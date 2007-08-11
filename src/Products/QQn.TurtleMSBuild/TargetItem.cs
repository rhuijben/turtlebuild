using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace QQn.TurtleMSBuild
{
	enum TargetType
	{
		None=0,
		Item=0x01,
		Copy=0x02,
		SharedItem=0x11,
		SharedCopy=0x12,
	}

	sealed class TargetItem
	{
		readonly string _target;
		readonly string _include;
		readonly ProjectItem _projectItem;
		TargetType _targetType;

		public TargetItem(string targetPath, string include, TargetType targetType)
			: this(targetPath, include, targetType, null)
		{
		}
		public TargetItem(string targetPath, string include, TargetType targetType, ProjectItem projectItem)
		{
			_target = targetPath;
			_include = include;
			_targetType = targetType;
			_projectItem = projectItem;
		}

		public ProjectItem Item
		{
			get { return _projectItem; }
		}

		public string Include
		{
			get { return _include; }
		}

		public string Target
		{
			get { return _target; }
		}

		public TargetType Type
		{
			get { return _targetType; }
			set { _targetType = value; }
		}
	}

	class ProjectOutputList : SortedList<string, TargetItem>
	{
		public ProjectOutputList()
			: base(StringComparer.InvariantCultureIgnoreCase)
		{ }

		public void Add(TargetItem target)
		{
			Add(target.Target, target);
		}

		public void WriteProjectOutput(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartElement("ProjectOutput");
			
			if (forReadability)
				xw.WriteComment("Project Items");

			foreach (TargetItem ti in Values)
			{
				if (ti.Type == TargetType.Item)
					WriteProjectOutputItem(xw, ti);
			}

			if (forReadability)
				xw.WriteComment("Project Copy Items");

			foreach (TargetItem ti in Values)
			{
				if (ti.Type == TargetType.Copy)
				{
					WriteProjectOutputItem(xw, ti);
				}
			}

			if (forReadability)
				xw.WriteComment("Shared Items");

			foreach (TargetItem ti in Values)
			{
				if (ti.Type == TargetType.SharedItem)
					WriteProjectOutputItem(xw, ti);
			}

			if (forReadability)
				xw.WriteComment("Shared Copy Items");

			foreach (TargetItem ti in Values)
			{
				if (ti.Type == TargetType.SharedCopy)
					WriteProjectOutputItem(xw, ti);
			}

			xw.WriteEndElement();
		}

		private static void WriteProjectOutputItem(XmlWriter xw, TargetItem ti)
		{
			xw.WriteStartElement(ti.Type.ToString());
			xw.WriteAttributeString("src", ti.Target);
			if(ti.Include != ti.Target)
				xw.WriteAttributeString("fromSrc", ti.Include);

			xw.WriteEndElement();
		}
	}
}
