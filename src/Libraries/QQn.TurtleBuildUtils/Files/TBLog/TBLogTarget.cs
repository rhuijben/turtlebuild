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
		/// <summary>
		/// 
		/// </summary>
		[Token("src")]
		public string Src;

		/// <summary>
		/// The name of the target (assembly) file (file name without extension)
		/// </summary>
		[Token("name")]
		public string TargetName;

		/// <summary>
		/// The extension of the target (assembly) file
		/// </summary>
		[Token("ext")]
		public string TargetExt;

		/// <summary>
		/// The strong name keyfile used to sign the target(s)
		/// </summary>
		[Token("keySrc")]
		public string KeySrc;

		/// <summary>
		/// The strong name keycontainer used to sign the target(s)
		/// </summary>
		[Token("keyContainer")]
		public string KeyContainer;

		/// <summary>
		/// Gets the location of the debug informationfile
		/// </summary>
		[Token("debugSrc")]
		public string DebugSrc;

		/// <summary>
		/// Gets the unique id used by the Microsoft debuggers to validate the debug informationfile. 
		/// The format used is the identifier used by Microsoft SymbolServer
		/// </summary>
		[Token("debugId")]
		public string DebugId;
	}
}
