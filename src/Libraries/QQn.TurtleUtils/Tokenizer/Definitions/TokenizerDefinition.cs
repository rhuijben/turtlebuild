using System;
using System.Collections.Generic;
using System.Text;

namespace QQn.TurtleUtils.Tokenizer.Definitions
{
	public abstract class TokenizerDefinition : TokenItemBase
	{
		internal readonly Dictionary<string, TokenMember> _tokens = new Dictionary<string, TokenMember>();
		readonly Dictionary<string, TokenItem> _csNames = new Dictionary<string, TokenItem>(StringComparer.InvariantCultureIgnoreCase);
		readonly Dictionary<string, TokenItem> _ciNames = new Dictionary<string, TokenItem>(StringComparer.InvariantCultureIgnoreCase);
		private readonly List<TokenItem> _placedItems = new List<TokenItem>();

		TokenItem _rest;

		protected void AddToken(TokenMember token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			_tokens.Add(token.Name, token);
		}

		protected void AddPlaced(int position, TokenItem token)
		{
			if (token == null)
				throw new ArgumentNullException("token");
			if (position < 0)
				throw new ArgumentOutOfRangeException("position", position, "Position must be greater than or equal to zero");
			else if (position == int.MaxValue)
				throw new InvalidOperationException("Can't set rest arguments via AddPlaces");

			while (_placedItems.Count < position + 1)
				_placedItems.Add(null);

			if (_placedItems[position] != null)
				throw new ArgumentException("Can't place two members at the same position");

			_placedItems[position] = token;
		}

		protected void SetRest(TokenItem token)
		{
			if (token == null)
				throw new ArgumentNullException("token");

			if (_rest != null)
				throw new InvalidOperationException("Can only set one rest argument");

			_rest = token;
		}

		public void Validate()
		{
			foreach (TokenItem t in _placedItems)
			{
				if (t == null)
					throw new InvalidOperationException("Not all placed locations are filled");
			}
		}

		public bool HasPlacedArguments
		{
			get { return _placedItems.Count > 0 || _rest != null; }
		}

		public virtual bool TryGetToken(string name, bool caseSensitive, out TokenItem token)
		{
			if(_csNames.Count == 0)
			{
				foreach(TokenMember tm in _tokens.Values)
				{
					foreach (TokenItem tk in tm.Tokens)
					{
						if (tk.Aliases != null)
						{
							foreach (string alias in tk.Aliases)
							{
								_csNames.Add(alias, tk);
							}
						}
					}
				}
			}
			if (!caseSensitive)
			{
				if (_ciNames.Count != _csNames.Count)
				{
					_ciNames.Clear();

					foreach (KeyValuePair<string, TokenItem> tk in _csNames)
						_ciNames.Add(tk.Key, tk.Value);
				}
			}

			return (caseSensitive ? _csNames : _ciNames).TryGetValue(name, out token);
		}

		internal IList<TokenItem> PlacedItems
		{
			get { return _placedItems; }
		}

		public TokenItem RestToken
		{
			get { return _rest; }
		}

	}
}
