using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer
{
	/// <summary>
	/// 
	/// </summary>
	public class TokenizerArgs
	{
		static readonly char[] _defaultCommandLineChars = new char[] { '-', '/' };

		IList<char> _commandLineChars;
		bool _cacheDefinition;
		bool _caseSensitive;
		bool _noDirectArgs;
		string _errorMessage;
		char _argumentValueSeparator;
		bool _noPlusMinArguments;
		bool _noNamedBetweenPlaced;
		bool _allowResponseFile;

		/// <summary>
		/// 
		/// </summary>
		public TokenizerArgs()
		{
		}
		
		/// <summary>
		/// 
		/// </summary>
		public bool CacheDefinition
		{
			get { return _cacheDefinition; }
			set { _cacheDefinition = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public IList<char> CommandLineChars
		{
			get { return _commandLineChars ?? _defaultCommandLineChars; }
			set 
			{
				if (value == null)
					_commandLineChars = null;
				else
					_commandLineChars = new List<char>(value).AsReadOnly(); 
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool CaseSensitive
		{
			get { return _caseSensitive; }
			set { _caseSensitive = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool AllowDirectArgs
		{
			get { return !_noDirectArgs; }
			set { _noDirectArgs = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public char ArgumentValueSeparator
		{
			get { return _argumentValueSeparator; }
			set { _argumentValueSeparator = value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool PlusMinSuffixArguments
		{
			get { return !_noPlusMinArguments; }
			set { _noPlusMinArguments = !value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool AllowNamedBetweenPlaced
		{
			get { return !_noNamedBetweenPlaced; }
			set { _noNamedBetweenPlaced = !value; }
		}

		/// <summary>
		/// 
		/// </summary>
		public bool AllowResponseFile
		{
			get { return _allowResponseFile; }
			set { _allowResponseFile = value; }
		}
	}
}
