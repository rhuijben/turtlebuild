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
		public const string PackageFileType = "application/x-QQn-TurtlePackage";
		//readonly FileInfo _package;
		//readonly IXPathNavigable _manifest;
		readonly List<IDisposable> _disposeAtClose;
		readonly MultipleStreamReader _reader;
		MultipleStreamReader _contentReader;
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
			using (Stream r = _reader.GetNextStream(0x01))
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
		protected MultipleStreamReader ContentReader
		{
			get
			{
				if (_contentReader == null)
				{
					_reader.Reset();
					_contentReader = _contentReader = new MultipleStreamReader(_reader.GetNextStream(0x02));
				}

				return _contentReader;
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

			MultipleStreamArgs fileCreateArgs = new MultipleStreamArgs();
			fileCreateArgs.StreamType = 0x04;
			fileCreateArgs.Assured = true;
			fileCreateArgs.GZipped = true;

			MultipleStreamCreateArgs msca = new MultipleStreamCreateArgs();
			msca.MaximumNumberOfStreams = 8;

			using (FileStream fs = File.Create(fileName, 16384))
			using (AssuredStream assurance = new AssuredStream(fs, args))
			using (MultipleStreamWriter msw = new MultipleStreamWriter(assurance))
			{
				MultipleStreamArgs msa = new MultipleStreamArgs();
				msa.StreamType = 0x01;
				msa.Assured = true;
				using (XmlWriter xw = new XmlTextWriter(msw.CreateStream(msa), Encoding.UTF8))
				{
					xw.WriteStartDocument();
					xw.WriteStartElement("TurtlePackage", "http://schemas.qqn.nl/2007/TurtlePackage");
					Tokenizer.TryWriteXml(xw, definition);
					xw.WriteEndDocument();
				}

				MultipleStreamCreateArgs zipcArgs = new MultipleStreamCreateArgs();
				zipcArgs.MaximumNumberOfStreams = definition.Containers.Count;
				using (MultipleStreamWriter zipBase = new MultipleStreamWriter(msw.CreateStream(0x02), zipcArgs))
				{
					foreach (PackContainer container in definition.Containers)
					{
						MultipleStreamCreateArgs ccArgs = new MultipleStreamCreateArgs();
						ccArgs.MaximumNumberOfStreams = container.Files.Count;
						using (MultipleStreamWriter containerWriter = new MultipleStreamWriter(zipBase.CreateStream(0x03), ccArgs))
						{
							foreach (PackFile file in container.Files)
							{
								using (FileStream fileSrc = File.OpenRead(QQnPath.Combine(file.BaseDir, file.Name)))
								using (Stream fileBlob = containerWriter.CreateStream(fileCreateArgs))
								{
									QQnPath.CopyStream(fileSrc, fileBlob);
								}
							}
						}
					}
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

		delegate void Extractor(PackFile file, Stream fileStream);

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

			ContentReader.Reset();
			
			foreach (PackContainer container in _pack.Containers)
			{
				using (Stream s = ContentReader.GetNextStream(0x03))
				{
					if (!args.ExtractContainer(container.Name))
						continue;

					using(MultipleStreamReader fr = new MultipleStreamReader(s))
					{
						foreach (PackFile pf in container.Files)
						{
							using (Stream fs = fr.GetNextStream(0x04))
							{
								extractor(pf, fs);
							}
						}
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

			ExtractFiles(args, delegate(PackFile file, Stream fileStream)
			{
				DirectoryMapFile dmf = directory.GetFile(file.RelativePath);

				if ((dmf == null) || !dmf.Unmodified() || !QQnCryptoHelpers.HashComparer.Equals(dmf.FileHash, file.FileHash))
					using (Stream s = directory.CreateFile(file.RelativePath, file.FileHash, file.FileSize))
					{
						QQnPath.CopyStream(fileStream, s);
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

			ExtractFiles(args, delegate(PackFile file, Stream fileStream)
			{
				using (Stream s = File.Create(QQnPath.Combine(directory, file.RelativePath)))
				{
					QQnPath.CopyStream(fileStream, s);
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
}
