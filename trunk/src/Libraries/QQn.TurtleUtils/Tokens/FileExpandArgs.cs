using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace QQn.TurtleUtils.Tokens
{
	/// <summary>
	/// 
	/// </summary>
	public enum FileExpandMode
	{
		/// <summary>
		/// 
		/// </summary>
		Normal,
		/// <summary>
		/// 
		/// </summary>
		DirectoryWildCards
	}

	/// <summary>
	/// 
	/// </summary>
	public class FileExpandArgs
	{
		string _baseDirectory;
		FileExpandMode _fileExpandMode;
		bool _removeNonExistingFiles;
		bool _matchDirectories;
		bool _dontMatchFiles;
		bool _matchHidden;
		bool _matchSystem;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileExpandArgs"/> class.
		/// </summary>
		public FileExpandArgs()
		{
			_baseDirectory = Environment.CurrentDirectory;
		}

		/// <summary>
		/// Gets or sets a value indicating whether [remove non existing files].
		/// </summary>
		/// <value>
		/// 	<c>true</c> if [remove non existing files]; otherwise, <c>false</c>.
		/// </value>
		[DefaultValue(false)]
		public bool RemoveNonExistingFiles
		{
			get { return _removeNonExistingFiles; }
			set { _removeNonExistingFiles = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [match directories].
		/// </summary>
		/// <value><c>true</c> if [match directories]; otherwise, <c>false</c>.</value>
		[DefaultValue(false)]
		public bool MatchDirectories
		{
			get { return _matchDirectories; }
			set { _matchDirectories = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to match hidden files.
		/// </summary>
		/// <value><c>true</c> if [match hidden files]; otherwise, <c>false</c>.</value>
		[DefaultValue(false)]
		public bool MatchHiddenFiles
		{
			get { return _matchHidden; }
			set { _matchHidden = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to match system files.
		/// </summary>
		/// <value><c>true</c> if [match system files]; otherwise, <c>false</c>.</value>
		[DefaultValue(false)]
		public bool MatchSystemFiles
		{
			get { return _matchSystem; }
			set { _matchSystem = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether [match files].
		/// </summary>
		/// <value><c>true</c> if [match files]; otherwise, <c>false</c>.</value>
		[DefaultValue(true)]
		public bool MatchFiles
		{
			get { return !_dontMatchFiles; }
			set { _dontMatchFiles = !value; }
		}

		/// <summary>
		/// Gets or sets the file expand mode.
		/// </summary>
		/// <value>The file expand mode.</value>
		public FileExpandMode FileExpandMode
		{
			get { return _fileExpandMode; }
			set { _fileExpandMode = value; }
		}

		/// <summary>
		/// Gets or sets the base directory.
		/// </summary>
		/// <value>The base directory.</value>
		public string BaseDirectory
		{
			get { return _baseDirectory; }
			set { _baseDirectory = value; }
		}
	}
}
