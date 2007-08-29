using System;
using System.Collections.Generic;
using QQn.TurtleUtils.Tokens;
using System.ComponentModel;
using QQn.TurtleUtils.Cryptography;
using QQn.TurtleUtils.Tokens.Definitions;


namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class Pack : PackItem
	{
		/// <summary>
		/// Gets "http://schemas.qqn.nl/2007/TurtlePackage"
		/// </summary>
		public const string Namespace = "http://schemas.qqn.nl/2007/TurtlePackage";
		bool _readOnly;

		/// <summary>
		/// Initializes a new instance of the <see cref="Pack"/> class.
		/// </summary>
		public Pack()
		{
			Pack = this;
		}

		PackContainerCollection _containers;

		/// <summary>
		/// Gets the containers.
		/// </summary>
		/// <value>The containers.</value>
		[TokenGroup("Container")]
		public virtual PackContainerCollection Containers
		{
			get { return _containers ?? (_containers = new PackContainerCollection(this)); }
		}

		/// <summary>
		/// Ensures the writability of the node
		/// </summary>
		protected internal override void EnsureWritable()
		{
			if (_readOnly)
				throw new InvalidOperationException();
		}

		#region ISupportInitialize Members

		/// <summary>
		/// Handles tokenizer initialization
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected override void OnBeginInitialize(TokenizerEventArgs e)
		{
			_readOnly = false;
			base.OnBeginInitialize(e);			
		}

		/// <summary>
		/// Handles tokenizer initialization
		/// </summary>
		/// <param name="e">The <see cref="QQn.TurtleUtils.Tokens.TokenizerEventArgs"/> instance containing the event data.</param>
		protected override void OnEndInitialize(TokenizerEventArgs e)
		{			
			base.OnEndInitialize(e);
			_readOnly = true;
		}

		#endregion

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
		/// </value>
		public bool IsReadOnly
		{
			get { return _readOnly; }
		}

		StrongNameKey _strongNameKey;
		/// <summary>
		/// Gets or sets the strong name key.
		/// </summary>
		/// <value>The strong name key.</value>
		public StrongNameKey StrongNameKey
		{
			get { return _strongNameKey; }
			set
			{
				EnsureWritable();
				_strongNameKey = value;
			}
		}
	}
}
