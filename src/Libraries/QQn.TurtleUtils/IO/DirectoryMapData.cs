using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.XPath;
using System.Xml;
using QQn.TurtleUtils.Cryptography;
using QQn.TurtleUtils.Tokens;

namespace QQn.TurtleUtils.IO
{
	class DirectoryMapData
	{
		readonly DirectoryMapCollection _files; 
		readonly DirectoryMapCollection _directories;
		HashType _hashType = QQnCryptoHelpers.DefaultHashType;
		string _directory;
		bool _dirty;
		public const string DirMapFile = ".tDirMap";
		public const string DirMapNs = "http://schemas.qqn.nl/2007/08/DirMap";

		public DirectoryMapData()
		{
			_files = new DirectoryMapCollection(this);
			_directories = new DirectoryMapCollection(this);
		}
		internal DirectoryMapData(string directory)
			: this()
		{
			_directory = Path.GetFullPath(directory);
		}

		[TokenGroup("File", typeof(DirectoryMapFile))]
		public DirectoryMapCollection Files
		{
			get { return _files; }
		}

		[TokenGroup("Directory", typeof(DirectoryMapDirectory))]
		public DirectoryMapCollection Directories
		{
			get { return _directories; }
		}

		[Token("hashType")]
		public HashType HashType
		{
			get { return _hashType; }

			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")] // Used by tokenizer
			set { _hashType = value; }
		}

		internal string FullPath
		{
			get { return _directory; }
		}

		public static DirectoryMapData Load(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");

			path = Path.GetFullPath(path);

			using(FileStream fs = File.OpenRead(Path.Combine(path, DirMapFile)))
			{
				XPathDocument doc = new XPathDocument(fs);

				XPathNavigator nav = doc.CreateNavigator();
				nav.MoveToRoot();
				nav.MoveToFirstChild();

				DirectoryMapData data;
				TokenizerArgs ta = new TokenizerArgs();
				ta.SkipUnknownNamedItems = true;
				if(Tokenizer.TryParseXml(nav, ta, out data))
				{
					data._directory = path;
					return data;
				}
			}
			return null;
		}

		public void Write()
		{
			string file = Path.Combine(_directory, DirMapFile);
			FileInfo fif = new FileInfo(file);

			if (fif.Exists && (fif.Attributes & (FileAttributes.Hidden | FileAttributes.ReadOnly)) != 0)
			{
				// The hidden flag is threated for some read-only like flag by .Net, so we must reset it
				fif.Attributes = FileAttributes.Normal;
			}
			
			using(FileStream fs = File.Create(file))
			using (XmlWriter xw = XmlWriter.Create(fs))
			{
				xw.WriteStartDocument();
				xw.WriteStartElement("DirectoryMap", DirMapNs);

				Tokenizer.TryWriteXml(xw, this);
			}
			File.SetAttributes(file, FileAttributes.Normal | FileAttributes.Hidden);
		}

		internal bool Dirty
		{
			get { return _dirty; }
			set { _dirty = value; }
		}
	}

	class DirectoryMapCollection : KeyedCollection<String, DirectoryMapItem>
	{
		DirectoryMapData _map;

		public DirectoryMapCollection(DirectoryMapData map)
			: base(StringComparer.InvariantCultureIgnoreCase, 16)
		{
			_map = map;
		}

		protected override string GetKeyForItem(DirectoryMapItem item)
		{
			return item.Filename;
		}

		protected override void SetItem(int index, DirectoryMapItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			this[index].Map = null;
			base.SetItem(index, item);
			item.Map = _map;
			_map.Dirty = true;
		}

		protected override void InsertItem(int index, DirectoryMapItem item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			base.InsertItem(index, item);
			item.Map = _map;
			_map.Dirty = true;
		}

		protected override void RemoveItem(int index)
		{
			this[index].Map = null;
			_map.Dirty = true;
			base.RemoveItem(index);
		}
	}
}