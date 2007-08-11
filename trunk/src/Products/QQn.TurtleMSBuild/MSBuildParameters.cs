using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleMSBuild
{
	/// <summary>
	/// Build parameter container; Used by the tokenizer in TurtleUtils to parse the
	/// value of the <see cref="Microsoft.Build.Framework.ILogger.Parameters"/> property
	/// </summary>
	public class TurtleParameters
	{
		/// <summary>
		/// The directory to write the tbLog's to
		/// </summary>
		[Token("OutputDir")]
		public string OutputDir;

		/// <summary>
		/// Indent the files for readability
		/// </summary>
		[Token("Indent")]
		public bool Indent;

		/// <summary>
		/// Defines extra file extensions for the scripts result
		/// </summary>
		[Token("ScriptExtension")]
		public string[] ScriptExtensions;

		/// <summary>
		/// Defines extra items searched for scripts
		/// </summary>
		[Token("ScriptItem")]
		public string[] ScriptItems;

		/// <summary>
		/// Defines extra items searched for shared files
		/// </summary>
		[Token("SharedItem")]
		public string[] SharedItems;

		/// <summary>
		/// Defines extra items searched for copied files
		/// </summary>
		[Token("CopyItem")]
		public string[] CopyItems;

		/// <summary>
		/// Defines extra items searched for local files
		/// </summary>
		[Token("LocalItem")]
		public string[] LocalItems;

		/// <summary>
		/// Defines extra items searched for content files
		/// </summary>
		[Token("ContentItem")]
		public string[] ContentItems;

		/// <summary>
		/// 
		/// </summary>
		[Token("BuildManifest")]
		public string BuildManifest;
	}
}
