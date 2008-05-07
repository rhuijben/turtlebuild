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
        Version _version;
        Uri _origin;
        IList<Uri> _hints;
        string _name;
        string _edition;
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
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [TokenGroup("version")]
        public Version Version
        {
            get { return _version; }
            set { EnsureWritable(); _version = value; }
        }

        /// <summary>
        /// Gets or sets the name of the package.
        /// </summary>
        /// <value>The name of the package.</value>
        [Token("name")]
        public string PackageName
        {
            get { return _name; }
            set { EnsureWritable(); _name = value; }
        }

        /// <summary>
        /// Gets or sets the package variant.
        /// </summary>
        /// <value>The package variant.</value>
        [Token("variant")]
        public string PackageVariant
        {
            get { return _edition; }
            set { EnsureWritable(); _edition = value; }
        }

        /// <summary>
        /// Gets or sets the origin.
        /// </summary>
        /// <value>The origin.</value>
        [Token("origin")]
        public Uri Origin
        {
            get { return _origin; }
            set { EnsureWritable(); _origin = value; }
        }

        /// <summary>
        /// Gets the origin hints.
        /// </summary>
        /// <value>The origin hints.</value>
        [Token("OriginHint")]
        public IList<Uri> OriginHints
        {
            get { return _hints ?? (_hints = new List<Uri>()); }
        }
		/// <summary>
		/// Ensures the writability of the node
		/// </summary>
		protected internal sealed override void EnsureWritable()
		{
			// Called indirectly from constructor
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
            if (_hints != null)
                _hints = new System.Collections.ObjectModel.ReadOnlyCollection<Uri>(_hints);
            else
                _hints = new Uri[0];

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

		/// <summary>
		/// Raises the <see cref="E:PackChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected sealed override void OnPackChanged(EventArgs e)
		{
			// Called indirectly from constructor
			base.OnPackChanged(e);
		}
	}
}
