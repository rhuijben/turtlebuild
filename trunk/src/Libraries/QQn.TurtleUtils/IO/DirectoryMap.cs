using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using QQn.TurtleUtils.Tokens;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace QQn.TurtleUtils.IO
{
	/// <summary>
	/// Container of file instances in a map and its submaps
	/// </summary>
	public class DirectoryMap
	{
		DirectoryInfo _directoryInfo;

		/// <summary>
		/// Gets the DirectoryMap for the specified path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns></returns>
		public static DirectoryMap Get(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			DirectoryInfo dirInfo = new DirectoryInfo(path);
			if (!dirInfo.Exists)
				return null;

			string mapFile = Path.Combine(dirInfo.FullName, ".tuMap");

			DirectoryMap map = null;
			if(File.Exists(mapFile))
			{
				using(FileStream fs = File.Open(mapFile, FileMode.Open, FileAccess.Read, FileShare.Read))
				{
					XPathDocument doc = new XPathDocument(fs);

					if (!Tokenizer.TryParseXml(doc, out map))
						map = null;
				}
			}
			if (map == null)
				map = new DirectoryMap();

			map.Initialize(dirInfo);

			return map;
		}

		private void Initialize(DirectoryInfo dirInfo)
		{
			_directoryInfo = dirInfo;
		}

		readonly Collection<DirectoryMapFile> _files = new Collection<DirectoryMapFile>();
		[TokenGroup("file")]
		Collection<DirectoryMapFile> Files
		{
			get { return _files; }
		}


	}
}
