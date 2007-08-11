using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Build.Framework;
using System.Xml;

namespace QQn.TurtleMSBuild
{
	abstract class ReferenceItem
	{
		public abstract void WriteReference(XmlWriter xw, bool forReadability);
	}

	class ReferenceList : List<AssemblyReference>, IComparer<AssemblyReference>
	{
		internal void WriteReferences(XmlWriter xw, bool forReadability)
		{
			this.Sort(this);

			AssemblyReference last = null;
			string lastName = null;
			foreach (AssemblyReference reference in this)
			{
				bool skipLast;
				string name = reference.AssemblyName;

				if (last != null)
					skipLast = name.StartsWith(lastName) && (name.Length == lastName.Length || name[lastName.Length] == ',');
				else
					skipLast = true;
				
				if(!skipLast)
					last.WriteReference(xw, forReadability);

				last = reference;
				lastName = name;
			}
			if(last != null)
				last.WriteReference(xw, forReadability);
			
		}

		#region IComparer<AssemblyReference> Members

		public int Compare(AssemblyReference x, AssemblyReference y)
		{
			if(x == null)
				return -1;
			else if(y == null)
				return 1;

			return string.Compare(x.AssemblyName, y.AssemblyName);
		}

		#endregion
	}

	class AssemblyReference : ReferenceItem
	{
		readonly string _assemblyName;
		string _src;

		public AssemblyReference(string assemblyName, ITaskItem item, Project project)
		{
			if (assemblyName == null)
				throw new ArgumentNullException("assemblyName");

			_assemblyName = assemblyName;
			if (item != null && project != null)
				_src = project.MakeRelativePath(item.ItemSpec);
		}

		public void WriteAttributes(XmlWriter xw)
		{
			xw.WriteAttributeString("assemblyName", _assemblyName);
			if(_src != null)
				xw.WriteAttributeString("src", _src);
		}

		public override void WriteReference(XmlWriter xw, bool forReadability)
		{
			xw.WriteStartElement("Assembly");
			WriteAttributes(xw);
			xw.WriteEndElement();
		}

		public string AssemblyName
		{
			get { return _assemblyName; }
		}
	}
}
