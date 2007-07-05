using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Cryptography
{
	public class SignedStreamCreateArgs
	{
		bool _nullGuid;
		AssemblyStrongNameKey _strongName;
		string _fileType;

		public SignedStreamCreateArgs()
		{
		}

		internal SignedStreamCreateArgs(AssemblyStrongNameKey strongName, string fileType)
		{
			StrongName = strongName;
			FileType = fileType;
		}

		internal void VerifyArgs(string paramName)
		{
			if (string.IsNullOrEmpty(FileType))
				throw new ArgumentException("FileType must be set on SignedStreamCreateArgs", paramName);
		}

		public string FileType
		{
			get { return _fileType; }
			set { _fileType = value; }
		}

		public bool NullGuid
		{
			get { return _nullGuid; }
			set { _nullGuid = value; }
		}

		public AssemblyStrongNameKey StrongName
		{
			get { return _strongName; }
			set { _strongName = value; }
		}
	}
}
