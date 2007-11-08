using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	internal sealed class ParserState
	{
		readonly string _expression;
		readonly ParserArgs _args;
		int _pos;
		int _lastToken;

		public ParserState(string expression, ParserArgs args)
		{
			if (string.IsNullOrEmpty(expression))
				throw new ArgumentNullException("expression");
			else if (args == null)
				throw new ArgumentNullException("args");

			_expression = expression;
			_args = args.Clone();			
		}

		/// <summary>
		/// Gets the next character and advances the position
		/// </summary>
		/// <returns></returns>
		internal char NextChar()
		{
			if (_pos < _expression.Length)
				return _expression[_pos++];
			else
				return '\0';
		}

		internal void Next()
		{
			if (_pos < _expression.Length)
				_pos++;			
		}

		/// <summary>
		/// Gets a value indicating whether the parser is at the end of the expression
		/// </summary>
		/// <value><c>true</c> if at end; otherwise, <c>false</c>.</value>
		public bool AtEnd
		{
			get { return _pos >= _expression.Length; }
		}

		/// <summary>
		/// Gets the peek char.
		/// </summary>
		/// <value>The peek char.</value>
		public char PeekChar
		{
			get
			{
				return (_pos < _expression.Length) ? _expression[_pos] : '\0';
			}
		}

		/// <summary>
		/// Skips all whitespace at the current position
		/// </summary>
		public void SkipWhitespace()
		{
			bool setLast = (_pos == _lastToken);

			while (_pos < _expression.Length && char.IsWhiteSpace(_expression, _pos))
				_pos++;

			if (setLast)
				_lastToken = _pos;
		}

		/// <summary>
		/// Skips the literal chars.
		/// </summary>
		public void SkipLiteralChars()
		{
			while (_pos < _expression.Length && (char.IsLetterOrDigit(_expression, _pos) || _expression[_pos] == '_'))
				_pos++;			
		}

		bool AtHexDigit()
		{
			if (char.IsDigit(_expression, _pos))
				return true;

			char c = _expression[_pos];

			if (((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F')))
				return true;

			return false;
		}


		/// <summary>
		/// Skips the hex digits.
		/// </summary>
		public void SkipHexDigits()
		{
			while(_pos < _expression.Length && AtHexDigit())
				_pos++;			
		}

		internal void SkipDigits()
		{
			while (_pos < _expression.Length && char.IsDigit(_expression, _pos))
				_pos++;			
		}

		int _cttPos;
		string _cttVal;
		/// <summary>
		/// Gets the current token text.
		/// </summary>
		/// <value>The current token text.</value>
		public string CurrentTokenText
		{
			get
			{
				if (_cttPos == _pos)
					return _cttVal;

				_cttVal = _expression.Substring(_lastToken, _pos - _lastToken);
				_cttPos = _pos;
				return _cttVal;
			}
		}

		internal bool LiteralEquals(string tokenText)
		{
			return 0 == string.Compare(_expression, _lastToken, tokenText, 0, _pos - _lastToken, StringComparison.OrdinalIgnoreCase);
		}


		/// <summary>
		/// Creates the token.
		/// </summary>
		/// <param name="tokenType">Type of the token.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		internal TagToken CreateToken(TagTokenType tokenType, string value)
		{
			int lt = _lastToken;
			_lastToken = _pos;
			return new TagToken(tokenType, lt, _pos - lt, value);
		}

		/// <summary>
		/// Creates the token.
		/// </summary>
		/// <param name="p">The p.</param>
		/// <returns></returns>
		internal TagToken CreateToken(char p)
		{
			int lt = _lastToken;
			_lastToken = _pos;

			return new TagToken((TagTokenType)p, lt, 1, null);
		}

		/// <summary>
		/// Gets the parser args.
		/// </summary>
		/// <value>The args.</value>
		public ParserArgs Args
		{
			get { return _args; }
		}

		/// <summary>
		/// Gets the expression.
		/// </summary>
		/// <value>The expression.</value>
		public string Expression
		{
			get { return _expression; }
		}

		/// <summary>
		/// Gets the current position.
		/// </summary>
		/// <value>The position.</value>
		public int Position
		{
			get { return _pos; }
		}

		/// <summary>
		/// Gets the token start position.
		/// </summary>
		/// <value>The token start position.</value>
		public int TokenStartPosition
		{
			get { return _lastToken; }
		}

		IEnumerator<TagToken> _tokenEnumerator;
		TagToken _peekToken;
		TagToken _currentToken;
		internal TagToken NextToken()
		{
			if (_tokenEnumerator == null)
				_tokenEnumerator = Lexer.WalkTokens(this).GetEnumerator();

			if (_peekToken != null)
			{
				_currentToken = _peekToken;
				_peekToken = null;
			}
			else
			{
				if (_tokenEnumerator.MoveNext())
					_currentToken = _tokenEnumerator.Current;
				else
					_currentToken = null;
			}

			return _currentToken;
		}

		internal TagToken CurrentToken
		{
			get { return _currentToken; }
		}

		internal TagToken PeekToken()
		{
			if (_tokenEnumerator == null)
				_tokenEnumerator = Lexer.WalkTokens(this).GetEnumerator();

			if (_peekToken == null)
			{
				if (_tokenEnumerator.MoveNext())
					_peekToken = _tokenEnumerator.Current;
			}

			return _peekToken;
		}

		internal TagTokenType PeekTokenType()
		{
			TagToken peekToken = PeekToken();

			if(peekToken != null)
				return peekToken.TokenType;
			else
				return TagTokenType.AtEof;
		}
	}
}
