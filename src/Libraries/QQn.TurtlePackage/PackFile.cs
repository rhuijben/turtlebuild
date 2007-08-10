using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using QQn.TurtleUtils.ItemSets;
using QQn.TurtleUtils.Streams;

namespace QQn.TurtlePackage
{
	[XmlRoot("File")]
	public class PackFile : ItemSetItem<PackContainer, PackFile, Pack>, IVerifiableFile
	{
		long _size;
		string _hash;
		DateTime _lastWriteTimeUtc;

		[XmlAttribute("size")]
		public long FileSize
		{
			get { return _size; }
			set { EnsureWritable(); _size = value; }
		}

		[XmlAttribute("hash")]
		public string FileHash
		{
			get { return _hash; }
			set { EnsureWritable(); _hash = value; }
		}

		[XmlAttribute("lastWritten")]
		public DateTime LastWriteTimeUtc
		{
			get { return _lastWriteTimeUtc; }
			set { EnsureWritable(); _lastWriteTimeUtc = value.ToUniversalTime(); }
		}


		#region IVerifiableFile Members

		string IVerifiableFile.Filename
		{
			get { return Name; }
		}

		string IVerifiableFile.FileHash
		{
			get { return FileHash; }
		}

		long IVerifiableFile.FileSize
		{
			get { return FileSize; }
		}

		DateTime? IVerifiableFile.LastWriteTimeUtc
		{
			get { return LastWriteTimeUtc; }
		}

		#endregion
	}
}
