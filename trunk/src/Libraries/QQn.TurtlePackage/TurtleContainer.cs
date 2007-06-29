using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace QQn.TurtlePackage
{
	public class TurtleContainer : TurtleItem
	{
		readonly string _name;
		List<TurtleFileItem> _files;

		protected internal TurtleContainer(bool readOnly, string name)
			: base(readOnly)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException("name");

			_name = name;
			_files = new List<TurtleFileItem>();
		}

		public ICollection<TurtleFileItem> Files
		{
			get { return _files; }
		}

		public void AddFile(string filename, string baseDirectory)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");
			else if (string.IsNullOrEmpty(baseDirectory))
				throw new ArgumentNullException("baseDirectory");

			EnsureWritable();

			filename = Path.GetFullPath(filename);
			baseDirectory = Path.GetFullPath(baseDirectory);

			filename = filename.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			baseDirectory = baseDirectory.Replace(Path.AltDirectorySeparatorChar, Path.AltDirectorySeparatorChar);

			string name;

			if(!filename.StartsWith(baseDirectory, StringComparison.InvariantCultureIgnoreCase))
				name = Path.GetFileName(filename);
			else if(baseDirectory[baseDirectory.Length-1] == Path.DirectorySeparatorChar)
				name = filename.Substring(baseDirectory.Length);
			else if(filename[0] == Path.DirectorySeparatorChar)
				name = filename.Substring(1);
			else
				name = Path.GetFileName(filename);

			_files.Add(new TurtleFileItem(name, filename));
		}

		public override string Name
		{
			get { return _name; }
		}
	}
}
