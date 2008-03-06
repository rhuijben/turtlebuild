using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.Cryptography;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Collections.Specialized;
using QQn.TurtleBuildUtils;

namespace QQn.TurtlePackage
{
	/// <summary>
	/// 
	/// </summary>
	public class TPack : IDisposable
	{
		/// <summary>
		/// 
		/// </summary>
		public const string PackageFileType = "QQn/tpZip-Package/1.0";
		//readonly FileInfo _package;
		//readonly IXPathNavigable _manifest;
		readonly List<IDisposable> _disposeAtClose;
		readonly MultipleStreamReader _reader;
		Stream _zipStream;
		readonly Pack _pack;

		internal TPack(Stream baseStream, params IDisposable[] disposeAtClose)
		{
			if (baseStream == null)
				throw new ArgumentNullException("baseStream");
			_disposeAtClose = new List<IDisposable>();
			_disposeAtClose.Add(baseStream);
			if (disposeAtClose == null)
				_disposeAtClose.AddRange(disposeAtClose);

			MultipleStreamCreateArgs msa = new MultipleStreamCreateArgs();
			msa.VerificationMode = VerificationMode.Full;

			_reader = new MultipleStreamReader(baseStream, msa);

			Pack pack;
			using (Stream r = _reader.GetNextStream(PackageDefinitionId))
			{
				XPathDocument doc = new XPathDocument(r);
				XPathNavigator nav = doc.CreateNavigator();
				nav.MoveToRoot();
				nav.MoveToFirstChild();

				TokenizerArgs ta = new TokenizerArgs();
				ta.SkipUnknownNamedItems = true;

				if (!Tokenizer.TryParseXml(nav, out pack) || pack == null)
					throw new IOException();
				else
					_pack = pack;
			}
		}

		/// <summary>
		/// Gets the content reader.
		/// </summary>
		/// <value>The content reader.</value>
		protected Stream ZipStream
		{
			get
			{
				if (_zipStream == null)
				{
					_reader.Reset();
					_zipStream = _reader.GetNextStream(ZipFileId);
				}

				return _zipStream;
			}
		}

		internal TPack()
		{
		}

		/// <summary>
		/// Closes this instance.
		/// </summary>
		public void Close()
		{
			Dispose(true);
		}


		[SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly")]
		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		bool _disposed;
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				if (_reader != null)
					_reader.Close();

				foreach (IDisposable d in _disposeAtClose)
					d.Dispose();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public const int PackageDefinitionId = 0x10;
		/// <summary>
		/// 
		/// </summary>
		public const int PackagingLogId = 0x11;
		/// <summary>
		/// 
		/// </summary>
		public const int ZipFileId = 0xFFFF;


		/// <summary>
		/// Creates the specified package
		/// </summary>
		/// <param name="fileName">Name of the file.</param>
		/// <param name="definition">The definition.</param>
		/// <returns></returns>
		public static TPack Create(string fileName, Pack definition)
		{
			if (fileName == null)
				throw new ArgumentNullException("fileName");

			if (definition == null)
				throw new ArgumentNullException("definition");

			AssuredStreamCreateArgs args = new AssuredStreamCreateArgs();
			if (definition.StrongNameKey != null)
				args.StrongNameKey = definition.StrongNameKey;

			args.FileType = PackageFileType;

			MultipleStreamCreateArgs msca = new MultipleStreamCreateArgs();
			msca.MaximumNumberOfStreams = 4;
			msca.VerificationMode = VerificationMode.None;

			using (FileStream fs = File.Create(fileName, 65536))
			using (AssuredStream assurance = new AssuredStream(fs, args))
			using (MultipleStreamWriter msw = new MultipleStreamWriter(assurance, msca))
			{
				MultipleStreamArgs msa = new MultipleStreamArgs();
				msa.StreamType = 0x10;
				msa.Assured = true;
				msa.GZipped = true;
				using (XmlWriter xw = new XmlTextWriter(msw.CreateStream(msa), Encoding.UTF8))
				{
					xw.WriteStartDocument();
					xw.WriteStartElement("TurtlePackage", "http://schemas.qqn.nl/2007/TurtlePackage");
					Tokenizer.TryWriteXml(xw, definition);
					xw.WriteEndDocument();
				}

				msa = new MultipleStreamArgs();
				msa.StreamType = 0x11;

				using (XmlWriter xw = new XmlTextWriter(msw.CreateStream(msa), Encoding.UTF8))
				{
					// TODO: Write tblog file
				}


				// Last stream: We add a zip file
				msa = new MultipleStreamArgs();
				msa.StreamType = ZipFileId; // Defined
				msa.Assured = false; // Use the whole file assurance for the zip
				msa.GZipped = false; // Don't compress again

				using(Stream ms = msw.CreateStream(msa))
				using (ZipFile zipFile = ZipFile.Create(ms))
				{
					zipFile.BeginUpdate();
					zipFile.UseZip64 = UseZip64.Dynamic;

					SetName setName = new SetName();

					zipFile.NameTransform = setName;

					SortedFileList added = new SortedFileList();
					added.BaseDirectory = "c:\\" + Guid.NewGuid();

					foreach (PackContainer container in definition.Containers)
					{
						foreach (PackFile file in container.Files)
						{
							if (!QQnPath.IsRelativeSubPath(file.StreamName) || added.Contains(file.StreamName))
							{
								string name = Path.GetFileNameWithoutExtension(file.StreamName);
								string ext = Path.GetExtension(file.StreamName);

								string attempt = "_/" + name + ext;
								int n = 0;
								do
								{
									if (!added.Contains(attempt))
									{
										file.StreamName = attempt;
										break;
									}

									attempt = string.Format("_/{0}.{1}.{2}", name, n++, ext);
								}
								while (true);
							}
								
							if (file.StreamName.Contains("\\"))
								file.StreamName = file.StreamName.Replace('\\', '/');

							added.Add(file.StreamName);
							setName.NextName = file.StreamName;

							zipFile.Add(file.FullName);
						}
					}

					zipFile.CommitUpdate();
				}
			}

			return null;
		}

		/// <summary>
		/// Opens from.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="verificationMode">The verification mode.</param>
		/// <returns></returns>
		public static TPack OpenFrom(string file, VerificationMode verificationMode)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");

			Uri uri;

			if (Uri.TryCreate(file, UriKind.Absolute, out uri))
			{
				if (!uri.IsFile && !uri.IsUnc)
					return OpenFrom(uri, verificationMode);
				else
					file = uri.LocalPath;
			}
			else
				uri = null;

			if (!File.Exists(file))
				throw new FileNotFoundException("Package not found", file);

			FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
			try
			{
				return Open(fs, verificationMode);
			}
			catch (Exception)
			{
				fs.Close();
				throw;
			}
		}


		/// <summary>
		/// Opens a TurtlePackage from the specified uri.
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="verificationMode">The verification mode.</param>
		/// <returns></returns>
		public static TPack OpenFrom(Uri uri, VerificationMode verificationMode)
		{
			return OpenFrom(uri, verificationMode, null);
		}

		/// <summary>
		/// Opens a TurtlePackage from the specified uri with the specified credentials
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="verificationMode">The verification mode.</param>
		/// <param name="credentials">The credentials.</param>
		/// <returns></returns>
		public static TPack OpenFrom(Uri uri, VerificationMode verificationMode, ICredentials credentials)
		{
			if (uri == null)
				throw new ArgumentNullException("uri");

			WebRequest request = WebRequest.Create(uri);

			HttpWebRequest httpRequest = request as HttpWebRequest;
			if (httpRequest != null)
			{
				httpRequest.AllowAutoRedirect = true;
				httpRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.Revalidate);
				if (credentials != null)
					httpRequest.Credentials = credentials;

				httpRequest.Pipelined = true;
				httpRequest.Method = "GET";
			}
			FtpWebRequest ftpRequest = request as FtpWebRequest;
			if (ftpRequest != null)
			{
				ftpRequest.CachePolicy = new System.Net.Cache.RequestCachePolicy(RequestCacheLevel.Revalidate);
				ftpRequest.EnableSsl = true;
				if (credentials != null)
					ftpRequest.Credentials = credentials;
			}

			WebResponse rsp = request.GetResponse();
			try
			{
				Stream sr = rsp.GetResponseStream();
				try
				{
					if (!sr.CanSeek)
						sr = new SeekableStream(sr, rsp.ContentLength);

					return Open(sr, verificationMode);
				}
				catch (Exception)
				{
					sr.Close();
					throw;
				}
			}
			catch (Exception)
			{
				rsp.Close();
				throw;
			}
		}

		/// <summary>
		/// Opens the specified stream.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="verificationMode">The verification mode.</param>
		/// <returns></returns>
		static TPack Open(Stream stream, VerificationMode verificationMode)
		{
			AssuredStream rootStream = new AssuredStream(stream, verificationMode);

			return new TPack(rootStream, stream);
		}

		class ExtractorEventArgs : EventArgs, IDisposable
		{
			PackFile _file;
			Stream _stream;
			ZipFile _zipFile;
			ZipEntry _entry;
			bool _closed;

			public ExtractorEventArgs(PackFile file, Stream stream)
			{
				if(file == null)
					throw new ArgumentNullException("file");
				else if(stream == null)
					throw new ArgumentNullException("stream");

				_file = file;
				_stream = stream;
			}

			internal ExtractorEventArgs(PackFile file, ZipFile zipFile, ZipEntry entry)
			{
				if (file == null)
					throw new ArgumentNullException("file");
				else if (zipFile == null)
					throw new ArgumentNullException("zipFile");
				else if (entry == null)
					throw new ArgumentNullException("entry");

				_file = file;
				_zipFile = zipFile;
				_entry = entry;
			}

			public PackFile PackFile
			{
				get { return _file; }
			}

			public Stream Stream
			{
				get 
				{
					if (_stream == null)
						_stream = _zipFile.GetInputStream(_entry);
					return _stream; 
				}
			}

			#region IDisposable Members

			public void Dispose()
			{
				if (_closed)
					return;

				if (_stream != null)
				{
					_closed = true;
					_stream.Close();
				}
			}

			#endregion
		}

		delegate void Extractor(ExtractorEventArgs e);

		/// <summary>
		/// Extracts to.
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="containers">The containers.</param>
		public void ExtractTo(DirectoryMap directory, ICollection<string> containers)
		{
			ExtractTo(directory, new TurtleExtractArgs(containers));
		}		

		/// <summary>
		/// Extracts to.
		/// </summary>
		/// <param name="directory">The directory.</param>
		public void ExtractTo(DirectoryMap directory)
		{
			ExtractTo(directory, new TurtleExtractArgs());
		}

		private void ExtractFiles(TurtleExtractArgs args, Extractor extractor)
		{
			if (args == null)
				throw new ArgumentNullException("args");
			else if (extractor == null)
				throw new ArgumentNullException("extract");

			ZipStream.Seek(0, SeekOrigin.Begin);

			Dictionary<string, PackFile> extract = new Dictionary<string, PackFile>(StringComparer.OrdinalIgnoreCase);
			
			// Prepare a list of items to extract
			foreach (PackContainer container in _pack.Containers)
			{
				if (!args.ExtractContainer(container.Name))
					continue;
					
				foreach (PackFile pf in container.Files)
				{
					extract.Add(pf.StreamName, pf);
				}
			}

			// Extract them in zip-order
			using(ZipFile zip = new ZipFile(ZipStream))
			{
				foreach(ZipEntry entry in zip)
				{
					PackFile pf;

					if(extract.TryGetValue(entry.Name, out pf))
					{
						extractor(new ExtractorEventArgs(pf, zip, entry));
					}
				}
			}			
		}

		/// <summary>
		/// Extracts the package to the specified directory, optionally using the specified 
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="useMap"></param>
		public void ExtractTo(string directory, bool useMap)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("directory");

			ExtractTo(directory, useMap, null);
		}

		/// <summary>
		/// Extracts the package to the specified <see cref="DirectoryMap"/>
		/// </summary>
		/// <param name="directory"></param>
		/// <param name="args"></param>
		public void ExtractTo(DirectoryMap directory, TurtleExtractArgs args)
		{
			if (directory == null)
				throw new ArgumentNullException("directory");
			else if (args == null)
				throw new ArgumentNullException("args");

			ExtractFiles(args, delegate(ExtractorEventArgs e)
			{
				PackFile file = e.PackFile;
				DirectoryMapFile dmf = directory.GetFile(file.RelativePath);

				if ((dmf == null) || !dmf.Unmodified() || !QQnCryptoHelpers.HashComparer.Equals(dmf.FileHash, file.FileHash))
					using (Stream s = directory.CreateFile(file.RelativePath, file.FileHash, file.FileSize))
					{
						QQnPath.CopyStream(e.Stream, s);
					}
				else
					directory.UnscheduleDelete(file.RelativePath); // Make sure it stays
			});
		}

		/// <summary>
		/// Extracts the package to the specified directory
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="args">The args.</param>
		public void ExtractTo(string directory, TurtleExtractArgs args)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("directory");
			else if (args == null)
				throw new ArgumentNullException("args");

			if(args.UseDirectoryMap)
			{
				using (DirectoryMap dm = DirectoryMap.Get(directory))
				{
					ExtractTo(dm, args);
				}
				return;
			}

			ExtractFiles(args, delegate(ExtractorEventArgs e)
			{
				using (Stream s = File.Create(QQnPath.Combine(directory, e.PackFile.RelativePath)))
				{
					QQnPath.CopyStream(e.Stream, s);
				}
			});
		}

		/// <summary>
		/// Extracts the specified containers to the specified directory
		/// </summary>
		/// <param name="directory">The directory.</param>
		/// <param name="useMap">if set to <c>true</c> [use map].</param>
		/// <param name="containers">The containers.</param>
		public void ExtractTo(string directory, bool useMap, string[] containers)
		{
			if (string.IsNullOrEmpty(directory))
				throw new ArgumentNullException("directory");

			TurtleExtractArgs args = new TurtleExtractArgs();

			if (containers != null)
				args.Containers = containers;

			args.UseDirectoryMap = useMap;

			ExtractTo(directory, args);
		}

		/// <summary>
		/// Extracts the package to the specified directory
		/// </summary>
		/// <param name="directory">The directory.</param>
		public void ExtractTo(string directory)
		{
			ExtractTo(directory, false, null);
		}

		/// <summary>
		/// Gets the pack.
		/// </summary>
		/// <value>The pack.</value>
		public Pack Pack
		{
			get { return _pack; }
		}
	}

	class FileSource : IStaticDataSource
	{
		readonly string _path;

		public FileSource(string path)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			_path = path;
		}


		#region IStaticDataSource Members

		public Stream GetSource()
		{
			return File.OpenRead(_path);
		}

		#endregion
	}

	class SetName : INameTransform
	{
		string _nextDirectory;
		string _nextName;

		public string NextDirectory
		{
			get { return _nextDirectory; }
			set { _nextDirectory = (value != null) ? ZipEntry.CleanName(value) : null; }
		}

		public string NextName
		{
			get { return _nextName; }
			set { _nextName = (value != null) ? ZipEntry.CleanName(value) : null; }
		}

		public string TransformDirectory(string name)
		{
			if (NextDirectory != null)
				name = NextDirectory;

			return name;
		}

		public string TransformFile(string name)
		{
			if (NextName != null)
				name = NextName;

			return name;
		}
	}
}
