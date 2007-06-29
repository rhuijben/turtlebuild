using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Globalization;

namespace QQn.TurtlePackage
{
	[DebuggerVisualizer("FileHash [Sha256={Sha256}, Length={Length}]")]
	public class FileHash : IXmlSerializable
	{
		string _sha256;
		long _length;

		public FileHash(string sha256, long length)
		{
			if (sha256 == null)
				throw new ArgumentNullException("sha256");
			else if (sha256.Length != 64)
				throw new ArgumentException("Invalid sha hash", "sha256");

			_sha256 = sha256;
			_length = Length;
		}

		public string Sha256
		{
			get { return _sha256; }
		}

		public long Length
		{
			get { return _length; }
		}

		System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			string data = reader.ReadString();

			if (data.Length < 66 || data[64] != '-')
				throw new XmlException("Failed to read Sha256+Length hash");

			_sha256 = data.Substring(0, 64);
			_length = long.Parse(data.Substring(65));
		}

		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			writer.WriteString(string.Format("{0}-{1}", Sha256, Length));
		}

		public override string ToString()
		{
			return string.Format("{0}-{1}", Sha256, Length);
		}

		public static string HashString(byte[] hashcode)
		{
			StringBuilder sb = new StringBuilder(64);
			foreach (byte b in hashcode)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "{0:X2}", b);
			}

			return sb.ToString();
		}

		public static FileHash CreateFromFile(string filename)
		{
			if(string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");

			long length;
			byte[] hash;
			using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				length = fs.Length;
				
				using (SHA256 sha = SHA256.Create())
				{
					hash = sha.ComputeHash(fs);					
				}
			}

			return new FileHash(HashString(hash), length);
		}
	}
}
