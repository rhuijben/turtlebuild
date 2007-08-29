using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogTarget
	{
		string _src;
		string _targetName;
		string _targetExt;
		string _keySrc;
		string _keyContainer;
		string _debugSrc;
		string _debugReference;

		/// <summary>
		/// A src reference to the primary target file
		/// </summary>
		[Token("src")]
		public string Src
		{
			get { return _src; }
			set { _src = value; }
		}

		/// <summary>
		/// The name of the target (assembly) file (file name without extension)
		/// </summary>
		[Token("name")]
		public string TargetName
		{
			get { return _targetName; }
			set { _targetName = value; }
		}

		/// <summary>
		/// The extension of the target (assembly) file
		/// </summary>
		[Token("ext")]
		public string Extension
		{
			get { return _targetExt; }
			set { _targetExt = value; }
		}

		/// <summary>
		/// The strong name keyfile used to sign the target(s)
		/// </summary>
		[Token("keySrc")]
		public string KeySrc
		{
			get { return _keySrc; }
			set { _keySrc = value; }
		}

		/// <summary>
		/// The strong name keycontainer used to sign the target(s)
		/// </summary>
		[Token("keyContainer")]
		public string KeyContainer
		{
			get { return _keyContainer; }
			set { _keyContainer = value; }
		}

		/// <summary>
		/// Gets the location of the debug informationfile
		/// </summary>
		[Token("debugSrc")]
		public string DebugSrc
		{
			get { return _debugSrc; }
			set { _debugSrc = value; }
		}

		/// <summary>
		/// Gets the unique id used by the Microsoft debuggers to validate the debug informationfile. 
		/// The format used is the identifier used by Microsoft SymbolServer
		/// </summary>
		[Token("debugId")]
		public string DebugId
		{
			get { return _debugReference; }
			set { _debugReference = value; }
		}
	}
}
