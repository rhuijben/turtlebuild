using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// Target information container
	/// </summary>
	public class TBLogTarget : ITokenizerInitialize
	{
		TBLogConfiguration _configuration;
		string _src;
		string _targetName;
		string _targetExt;
		string _keySrc;
		string _keyContainer;
		string _processorArchitecture;
		string _targetPlatform;
		string _debugSrc;
		string _debugReference;
		bool _completed;

		/// <summary>
		/// A src reference to the primary target file
		/// </summary>
		[Token("src")]
		public string Src
		{
			get { return _src; }
			set { EnsureWritable(); _src = value; }
		}

		/// <summary>
		/// Gets the full path to the primary target file
		/// </summary>
		public string FullSrc
		{
			get { return (Configuration != null && !string.IsNullOrEmpty(Src)) ? QQnPath.Combine(Configuration.BasePath, Src) : null; }
		}

		/// <summary>
		/// The name of the target (assembly) file (file name without extension)
		/// </summary>
		[Token("name")]
		public string TargetName
		{
			get { return _targetName; }
			set { EnsureWritable(); _targetName = value; }
		}

		/// <summary>
		/// The extension of the target (assembly) file
		/// </summary>
		[Token("ext")]
		public string Extension
		{
			get { return _targetExt; }
			set { EnsureWritable(); _targetExt = value; }
		}

		/// <summary>
		/// The strong name keyfile used to sign the target(s)
		/// </summary>
		[Token("keySrc")]
		public string KeySrc
		{
			get { return _keySrc; }
			set { EnsureWritable(); _keySrc = value; }
		}

		/// <summary>
		/// The strong name keycontainer used to sign the target(s)
		/// </summary>
		[Token("keyContainer")]
		public string KeyContainer
		{
			get { return _keyContainer; }
			set { EnsureWritable(); _keyContainer = value; }
		}

		/// <summary>
		/// Gets or sets the processor architecture.
		/// </summary>
		/// <value>The processor architecture.</value>
		[Token("processorArchitecture")]
		public string ProcessorArchitecture
		{
			get { return _processorArchitecture; }
			set { EnsureWritable(); _processorArchitecture = value; }
		}

		/// <summary>
		/// Gets or sets the target platform.
		/// </summary>
		/// <value>The target platform.</value>
		[Token("platform")]
		public string TargetPlatform
		{
			get { return _targetPlatform; }
			set { EnsureWritable(); _targetPlatform = value; }
		}

		/// <summary>
		/// Gets the location of the debug informationfile
		/// </summary>
		[Token("debugSrc")]
		public string DebugSrc
		{
			get { return _debugSrc; }
			set { EnsureWritable(); _debugSrc = value; }
		}

		/// <summary>
		/// Gets the unique id used by the Microsoft debuggers to validate the debug informationfile. 
		/// The format used is the identifier used by Microsoft SymbolServer
		/// </summary>
		[Token("debugId")]
		public string DebugId
		{
			get { return _debugReference; }
			set { EnsureWritable(); _debugReference = value; }
		}

		/// <summary>
		/// Gets or sets the configuration.
		/// </summary>
		/// <value>The configuration.</value>
		public TBLogConfiguration Configuration
		{
			get { return _configuration; }
			internal set { _configuration = value; }
		}

		#region ITokenizerInitialize Members

		void EnsureWritable()
		{
			if (_completed)
				throw new InvalidOperationException();
		}

		void ITokenizerInitialize.OnBeginInitialize(TokenizerEventArgs e)
		{
		}

		void ITokenizerInitialize.OnEndInitialize(TokenizerEventArgs e)
		{
			_completed = true;
		}

		#endregion
	}
}
