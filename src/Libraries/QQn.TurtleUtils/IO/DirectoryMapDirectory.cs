using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// 
	/// </summary>
	public class DirectoryMapDirectory : DirectoryMapItem
	{
		bool _created;
		/// <summary>
		/// Initializes a new instance of the <see cref="DirectoryMapFile"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public DirectoryMapDirectory(string name)
			: base(name)
		{
		}

		public DirectoryMapDirectory()
			: base()
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="DirectoryMapItem"/> is created.
		/// </summary>
		/// <value><c>true</c> if created; otherwise, <c>false</c>.</value>
		[Token("created"), DefaultValue(false)]
		public bool Created
		{
			get { return _created; }
			set 
			{
				EnsureWritable();
				_created = value; 
			}
		}
	}
}
