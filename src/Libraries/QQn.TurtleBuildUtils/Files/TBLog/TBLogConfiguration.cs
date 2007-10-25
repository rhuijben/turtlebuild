using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokens;
using System.Collections.ObjectModel;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	/// <summary>
	/// 
	/// </summary>
	public class TBLogConfiguration : IHasFullPath, ITokenizerInitialize
	{
		TBLogReferences _references;
		TBLogTarget _target;
		TBLogProjectOutput _projectOutput;
		TBLogContent _content;

		string _name;
		string _platform;
		string _outputPath;
		string _basePath;

		#region IHasFullPath Members

		IHasFullPath _parent;
		string IHasFullPath.FullPath
		{
			get { return _basePath; }
		}

		internal IHasFullPath Parent
		{
			get { return _parent; }
			set
			{
				_parent = value;
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>The target.</value>
		[TokenGroup("Target")]
		public TBLogTarget Target
		{
			get { return _target ?? (_target = new TBLogTarget()); }
			set { EnsureWritable(); _target = value; }
		}

		/// <summary>
		/// Gets or sets the project output.
		/// </summary>
		/// <value>The project output.</value>
		[TokenGroup("ProjectOutput")]
		public TBLogProjectOutput ProjectOutput
		{
			get { return _projectOutput ?? (_projectOutput = new TBLogProjectOutput()); }
			set { EnsureWritable(); _projectOutput = value; }
		}

		/// <summary>
		/// Gets or sets the content.
		/// </summary>
		/// <value>The content.</value>
		[TokenGroup("Content")]
		public TBLogContent Content
		{
			get { return _content ?? (_content = new TBLogContent()); }
			set { EnsureWritable(); _content = value; }
		}

		/// <summary>
		/// Gets or sets the references.
		/// </summary>
		/// <value>The references.</value>
		[TokenGroup("References")]
		public TBLogReferences References
		{
			get { return _references ?? (_references = new TBLogReferences()); }
			set { EnsureWritable(); _references = value; }
		}

		/// <summary>
		/// Gets or sets the configuration name.
		/// </summary>
		/// <value>The name.</value>
		[Token("name")]
		public string Name
		{
			get { return _name; }
			set { EnsureWritable(); _name = value; }
		}

		/// <summary>
		/// Gets or sets the platform.
		/// </summary>
		/// <value>The platform.</value>
		[Token("platform")]
		public string Platform
		{
			get { return _platform; }
			set { EnsureWritable(); _platform = value; }
		}

		/// <summary>
		/// Gets or sets the output path.
		/// </summary>
		/// <value>The output path.</value>
		[Token("outputPath")]
		public string OutputPath
		{
			get { return _outputPath; }
			set { EnsureWritable(); _outputPath = value; }
		}

		/// <summary>
		/// Gets or sets the output path.
		/// </summary>
		/// <value>The output path.</value>
		[Token("basePath")]
		public string BasePath
		{
			get { return _basePath; }
			set { EnsureWritable(); _basePath = value; }
		}

		bool _completed;
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

			ProjectOutput.Parent = this;
			Content.Parent = this;
		}

		#endregion
	}

	public class TBLogConfigurationCollection : Collection<TBLogConfiguration>
	{
		IHasFullPath _parent;

		internal IHasFullPath Parent
		{
			get { return _parent; }
			set
			{
				_parent = value;
				foreach (TBLogConfiguration c in this)
					c.Parent = value;
			}
		}

		/// <summary>
		/// Determines whether the list contains the specified configuration
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <param name="platform">The platform.</param>
		/// <returns>
		/// 	<c>true</c> if the list contains the specified configuration; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string configuration, string platform)
		{
			foreach (TBLogConfiguration config in this)
			{
				if (string.Equals(configuration, config.Name, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(platform, config.Platform, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether the list contains the specified configuration
		/// </summary>
		/// <param name="configuration">The configuration.</param>
		/// <returns>
		/// 	<c>true</c> if the list contains the specified configuration; otherwise, <c>false</c>.
		/// </returns>
		public bool Contains(string configuration)
		{
			foreach (TBLogConfiguration config in this)
			{
				if (string.Equals(configuration, config.Name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the <see cref="QQn.TurtleBuildUtils.Files.TBLog.TBLogConfiguration"/> with the specified configuration and platform.
		/// </summary>
		/// <value></value>
		public TBLogConfiguration this[string configuration, string platform]
		{
			get
			{
				foreach (TBLogConfiguration config in this)
				{
					if (string.Equals(configuration, config.Name, StringComparison.OrdinalIgnoreCase) &&
						string.Equals(platform, config.Platform, StringComparison.OrdinalIgnoreCase))
					{
						return config;
					}
				}

				throw new ArgumentException("Configuration not found", "configuration");
			}
		}

		/// <summary>
		/// Gets the <see cref="QQn.TurtleBuildUtils.Files.TBLog.TBLogConfiguration"/> with the specified configuration.
		/// </summary>
		/// <value></value>
		public TBLogConfiguration this[string configuration]
		{
			get
			{
				foreach (TBLogConfiguration config in this)
				{
					if (string.Equals(configuration, config.Name, StringComparison.OrdinalIgnoreCase))
					{
						return config;
					}
				}

				throw new ArgumentException("Configuration not found", "configuration");
			}
		}
	}
}
