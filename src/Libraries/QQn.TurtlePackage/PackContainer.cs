using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using System.Diagnostics;


namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class PackContainer : PackItem
	{
		string _containerDir;

		/// <summary>
		/// Initializes a new instance of the <see cref="PackContainer"/> class.
		/// </summary>
		public PackContainer()
		{
		}

		PackFileCollection _files;

		/// <summary>
		/// Gets or sets the container dir.
		/// </summary>
		/// <value>The container dir.</value>
		[Token("containerDir"), DefaultValue(null)]
		public string ContainerDir
		{
			get { return _containerDir; }
			set { _containerDir = string.IsNullOrEmpty(value) ? null : NormalizeDirectory(value); }
		}

		/// <summary>
		/// Gets the files.
		/// </summary>
		/// <value>The files.</value>
		[TokenGroup("Item")]
		public virtual PackFileCollection Files
		{
			get { return _files ?? (_files = new PackFileCollection(this)); }
		}

	}
}
