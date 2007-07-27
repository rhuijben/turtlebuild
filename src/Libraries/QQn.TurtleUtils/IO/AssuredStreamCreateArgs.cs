using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Argument container for creating <see cref="AssuredStream"/> objects
	/// </summary>
	public class AssuredStreamCreateArgs
	{
		bool _nullGuid;
		StrongNameKey _strongName;
		string _fileType;

		/// <summary>
		/// Initializes a new instance of the <see cref="AssuredStreamCreateArgs"/> class.
		/// </summary>
		public AssuredStreamCreateArgs()
		{
		}

		internal AssuredStreamCreateArgs(StrongNameKey strongName, string fileType)
		{
			StrongNameKey = strongName;
			FileType = fileType;
		}

		internal void VerifyArgs(string paramName)
		{
			if (string.IsNullOrEmpty(FileType))
				throw new ArgumentException("FileType must be set on SignedStreamCreateArgs", paramName);
		}

		/// <summary>
		/// Gets or sets the type of the file.
		/// </summary>
		/// <value>The type of the file.</value>
		public string FileType
		{
			get { return _fileType; }
			set { _fileType = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the file should have <see cref="Guid.Empty"/> as guid value
		/// </summary>
		/// <value><c>true</c> if <see cref="Guid.Empty"/> should be used; otherwise, <c>false</c>.</value>
		public bool NullGuid
		{
			get { return _nullGuid; }
			set { _nullGuid = value; }
		}

		/// <summary>
		/// Gets or sets the strong name key.
		/// </summary>
		/// <value>The strong name key.</value>
		public StrongNameKey StrongNameKey
		{
			get { return _strongName; }
			set { _strongName = value; }
		}
	}
}
