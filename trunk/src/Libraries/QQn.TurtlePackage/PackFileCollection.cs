using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;

namespace QQn.TurtlePackage
{
	public class PackFileCollection : PackCollection<PackFile>
	{
		public PackFileCollection(PackContainer parent)
			: base(parent)
		{
		}
	}
}
