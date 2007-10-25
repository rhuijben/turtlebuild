using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// Project information container
	/// </summary>
	public class TBLogProject : ITokenizerInitialize
	{
		string _name;
		string _path;
		string _file;
		bool _completed;

		/// <summary>
		/// The name of the project (= file name of project without extension)
		/// </summary>
		[Token("name")]
		public string Name
		{
			get { return _name; }
			set { EnsureWritable(); _name = value; }
		}

		/// <summary>
		/// The full path to the directory where the project was located when the log was generated
		/// </summary>
		[Token("path")]
		public string Path
		{
			get { return _path; }
			set { EnsureWritable(); _path = value; }
		}

		/// <summary>
		/// The full name of the project file (relative from the project path)
		/// </summary>
		[Token("file")]
		public string File
		{
			get { return _file; }
			set { EnsureWritable();  _file = value; }
		}

		void EnsureWritable()
		{
			if (_completed)
				throw new InvalidOperationException();
		}

		#region ITokenizerInitialize Members

		void ITokenizerInitialize.OnBeginInitialize(TokenizerEventArgs e)
		{
			//throw new Exception("The method or operation is not implemented.");
		}

		void ITokenizerInitialize.OnEndInitialize(TokenizerEventArgs e)
		{
			_completed = true;
		}

		#endregion
	}
}
