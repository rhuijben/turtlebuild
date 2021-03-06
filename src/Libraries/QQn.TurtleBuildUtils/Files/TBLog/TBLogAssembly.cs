using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogAssembly
	{
		string _assemblyName;

		/// <summary>
		/// The full assembly name of the produced assembly
		/// </summary>
		[Token("assemblyName")]
		public string AssemblyName
		{
			get { return _assemblyName; }
			set { _assemblyName = value; }
		}
	}
}
