using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtlePackage
{
	public class PackItem
	{
		Pack _pack;
		string _name;
		public event EventHandler PackChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="PackItem"/> class.
		/// </summary>
		protected PackItem()
		{
		}

		/// <summary>
		/// Ensures the writability of the node
		/// </summary>
		protected internal virtual void EnsureWritable()
		{
			if (_pack != null)
				_pack.EnsureWritable();
		}

		/// <summary>
		/// Gets or sets the pack.
		/// </summary>
		/// <value>The pack.</value>
		public Pack Pack
		{
			get { return _pack; }
			protected internal set
			{
				if (_pack != value)
				{
					_pack = value;
					OnPackChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:PackChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnPackChanged(EventArgs e)
		{
			if (PackChanged != null)
				PackChanged(this, e);
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		[Token("name")]
		public virtual string Name
		{
			get { return _name; }
			set 
			{
				if (_name != null)
					throw new InvalidOperationException();

				EnsureWritable(); 
				_name = value; 
			}
		}
	}
}
