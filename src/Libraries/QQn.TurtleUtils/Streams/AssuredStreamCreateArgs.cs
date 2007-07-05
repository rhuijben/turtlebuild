using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Cryptography;

namespace QQn.TurtleUtils.Streams
{
	/// <summary>
	/// 
	/// </summary>
	public class AssuredStreamCreateArgs
	{
		bool _nullGuid;
		StrongNameKey _strongName;
		string _fileType;

		/// <summary>
		/// 
		/// </summary>
		public AssuredStreamCreateArgs()
		{
		}

		internal AssuredStreamCreateArgs(StrongNameKey strongName, string fileType)
		{
			StrongName = strongName;
			FileType = fileType;
		}

		internal void VerifyArgs(string paramName)
		{
			if (string.IsNullOrEmpty(FileType))
				throw new ArgumentException("FileType must be set on SignedStreamCreateArgs", paramName);
		}

		/// <summary>
		/// 
		/// </summary>
		public string FileType
		{
			get { return _fileType; }
			set { _fileType = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool NullGuid
		{
			get { return _nullGuid; }
			set { _nullGuid = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public StrongNameKey StrongName
		{
			get { return _strongName; }
			set { _strongName = value; }
		}
	}
}
