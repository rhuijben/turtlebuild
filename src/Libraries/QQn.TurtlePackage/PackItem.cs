using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.IO;
using QQn.TurtleUtils.IO;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class PackItem
	{
		Pack _pack;
		PackItem _parent;
		string _name;
		/// <summary>
		/// Occurs when [pack changed].
		/// </summary>
		public event EventHandler PackChanged;
		/// <summary>
		/// Occurs when [parent changed].
		/// </summary>
		public event EventHandler ParentChanged;

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
				EnsureWritable();
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
		/// Gets or sets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public PackItem Parent
		{
			get { return _parent; }
			protected internal set
			{
				EnsureWritable();
				if (_parent != value)
				{
					_parent = value;
					OnParentChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:ParentChanged"/> event.
		/// </summary>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnParentChanged(EventArgs eventArgs)
		{
			if(ParentChanged != null)
				ParentChanged(this, EventArgs.Empty);
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

		string _baseDir;
		/// <summary>
		/// Gets or sets the base dir.
		/// </summary>
		/// <value>The base dir.</value>
		public string BaseDir
		{
			get 
			{
				if((Pack == this) || (Parent == null))
					return _baseDir;
				
				string baseDir = Parent.BaseDir;

				if(baseDir == null)
				{
					return ((_baseDir != null) && Path.IsPathRooted(_baseDir)) ? _baseDir : null;
				}
				else if(_baseDir == null)
					return baseDir;
				else
					return Path.GetFullPath(Path.Combine(baseDir, _baseDir));
			}
			set
			{
				EnsureWritable();

				if(value == null)
					_baseDir = null;
				else
				{
					if((Pack == this) || (Parent == null))
						_baseDir = Path.GetFullPath(value);
					else
					{
						string baseDir = Parent.BaseDir;

						if(baseDir == null)
							_baseDir = Path.GetFullPath(value);
						else
							_baseDir = QQnPath.MakeRelativePath(baseDir, Path.Combine(baseDir, value));
					}
				}
			}
		}

		/// <summary>
		/// Normalizes the path.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected static string NormalizePath(string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			return value.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
		}

		/// <summary>
		/// Normalizes the directory.
		/// </summary>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		protected static string NormalizeDirectory(string value)
		{
			value = NormalizePath(value);

			if (value.Length > 0 && value[value.Length - 1] != '/')
				value += '/';

			return value;
		}
	}
}
