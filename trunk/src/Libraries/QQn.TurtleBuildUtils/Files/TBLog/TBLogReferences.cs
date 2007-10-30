using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogReferences : TBLogContainer
	{
		readonly TBLogItemCollection<TBLogProjectReference> _projectReferences;

		/// <summary>
		/// Initializes a new instance of the <see cref="TBLogReferences"/> class.
		/// </summary>
		public TBLogReferences()
		{
			_projectReferences = new TBReferenceCollection<TBLogProjectReference>(this);
		}

		/// <summary>
		/// Gets the referenced projects.
		/// </summary>
		/// <value>The projects.</value>
		[TokenGroup("Project")]
		public TBLogItemCollection<TBLogProjectReference> Projects
		{
			get { return _projectReferences; }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class TBLogProjectReference : TBLogItem
	{
		string _name;

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[Token("name")]
		public string Name
		{
			get { return _name; }
			set { _name = value; }
		}

	}
}
