using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace QQn.TurtleUtils.Tags.ExpressionParser
{
	abstract class BinaryExpression : TagExpression
	{
		TagExpression _lhs; // Only editted by and and or-reordering
		TagExpression _rhs;
		bool _editable;

		protected BinaryExpression(TagToken token, TagExpression lhs, TagExpression rhs)
			: base(token)
		{
			if (lhs == null)
				throw new ArgumentNullException("lhs");
			else if (rhs == null)
				throw new ArgumentNullException("rhs");

			_lhs = lhs;
			_rhs = rhs;
		}

		/// <summary>
		/// Gets all sub expressions.
		/// </summary>
		/// <value>The sub expressions.</value>
		protected internal override IEnumerable<TagExpression> SubExpressions
		{
			get { return new TagExpression[] { _lhs, _rhs }; }
		}

		/// <summary>
		/// Gets the left hand.
		/// </summary>
		/// <value>The left hand.</value>
		public TagExpression LeftHand
		{
			[DebuggerStepThrough]
			get { return _lhs; }
			internal set
			{
				if (!_editable)
					throw new InvalidOperationException();
				_lhs = value;
			}
		}

		/// <summary>
		/// Gets the right hand.
		/// </summary>
		/// <value>The right hand.</value>
		public TagExpression RightHand
		{
			[DebuggerStepThrough]
			get { return _rhs; }
			internal set
			{
				if (!_editable)
					throw new InvalidOperationException();
				_rhs = value;
			}
		}

		/// <summary>
		/// Sets the LeftHand and RightHand editable state
		/// </summary>
		/// <param name="value">if set to <c>true</c> [value].</param>
		internal void SetEditable(bool value)
		{
			_editable = value;
		}

		public override string ToString()
		{
			return "(" + LeftHand.ToString() + " " + Token.ToString() + " " + RightHand.ToString() + ")";
		}
	}
}
