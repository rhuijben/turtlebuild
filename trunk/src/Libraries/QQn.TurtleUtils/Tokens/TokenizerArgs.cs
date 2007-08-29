using System;
using System.Collections.Generic;
using System.Globalization;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	public class TokenizerArgs
	{
		static readonly char[] _defaultCommandLineChars = new char[] { '-', '/' };

		IList<char> _commandLineChars;
		bool _caseSensitive;
		bool _noDirectArgs;
		string _errorMessage;
		char _argumentValueSeparator;
		bool _noPlusMinArguments;
		bool _namedBetweenPlaced;
		bool _allowResponseFile;
		bool _skipUnknownNamedItems;
		char _directValueSeparator;
		CultureInfo _cultureInfo;

		/// <summary>
		/// Initializes a new instance of the <see cref="TokenizerArgs"/> class.
		/// </summary>
		public TokenizerArgs()
		{
			_directValueSeparator = '=';
		}

		/// <summary>
		/// Clones this instance.
		/// </summary>
		/// <returns></returns>
		public virtual TokenizerArgs Clone(object context)
		{
			TokenizerArgs clone = (TokenizerArgs)MemberwiseClone();

			clone._context = context;

			return clone;
		}

		/// <summary>
		/// Gets or sets the command line chars.
		/// </summary>
		/// <value>The command line chars.</value>
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
		/// Gets or sets a value indicating whether [case sensitive].
		/// </summary>
		/// <value><c>true</c> if [case sensitive]; otherwise, <c>false</c>.</value>
		public bool CaseSensitive
		{
			get { return _caseSensitive; }
			set { _caseSensitive = value; }
		}

		/// <summary>
		/// Gets or sets the error message.
		/// </summary>
		/// <value>The error message.</value>
		public string ErrorMessage
		{
			get { return _errorMessage; }
			set { _errorMessage = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [allow direct args].
		/// </summary>
		/// <value><c>true</c> if [allow direct args]; otherwise, <c>false</c>.</value>
		public bool AllowDirectArgs
		{
			get { return !_noDirectArgs; }
			set { _noDirectArgs = value; }
		}

		/// <summary>
		/// Gets or sets the argument value separator.
		/// </summary>
		/// <value>The argument value separator.</value>
		public char ArgumentValueSeparator
		{
			get { return _argumentValueSeparator; }
			set { _argumentValueSeparator = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether plus min suffix arguments are allowed.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if plus min suffix arguments are allowed; otherwise, <c>false</c>.
		/// </value>
		public bool PlusMinSuffixArguments
		{
			get { return !_noPlusMinArguments; }
			set { _noPlusMinArguments = !value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [allow named between placed].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [allow named between placed]; otherwise, <c>false</c>.
		/// </value>
		public bool AllowNamedBetweenPlaced
		{
			get { return _namedBetweenPlaced; }
			set { _namedBetweenPlaced = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether response files are allowed
		/// </summary>
		/// <value>
		/// 	<c>true</c> if response files are allowed; otherwise, <c>false</c>.
		/// </value>
		public bool AllowResponseFile
		{
			get { return _allowResponseFile; }
			set { _allowResponseFile = value; }
		}

		/// <summary>
		/// Gets or sets the direct value separator.
		/// </summary>
		/// <value>The direct value separator.</value>
		public char DirectValueSeparator
		{
			get { return _directValueSeparator; }
			set { _directValueSeparator = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to skip unknown named items (Valid on xml and Connection strings)
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [skip unknown named items]; otherwise, <c>false</c>.
		/// </value>
		public bool SkipUnknownNamedItems
		{
			get { return _skipUnknownNamedItems; }
			set { _skipUnknownNamedItems = value; }
		}

		/// <summary>
		/// Gets or sets the culture info.
		/// </summary>
		/// <value>The culture info.</value>
		public CultureInfo CultureInfo
		{
			get { return _cultureInfo ?? (_cultureInfo = CultureInfo.CurrentCulture); }
			set { _cultureInfo = value; }
		}

		object _context;
		/// <summary>
		/// Gets or sets the context.
		/// </summary>
		/// <value>The context.</value>
		internal object Context
		{
			get { return _context; }
		}
	}
}
