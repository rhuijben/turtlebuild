using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using System.Diagnostics;


namespace QQn.TurtlePackage
{
	public class PackContainer : PackItem
	{
		string _containerDir;

		public PackContainer()
		{
		}

		PackFileCollection _files;

		[Token("containerDir"), DefaultValue(null)]
		public string ContainerDir
		{
			get { return _containerDir; }
			set { _containerDir = string.IsNullOrEmpty(value) ? null : NormalizeDirectory(value); }
		}
		
		[TokenGroup("Item")]
		public virtual PackFileCollection Files
		{
			get { return _files ?? (_files = new PackFileCollection(this)); }
		}

	}
}
