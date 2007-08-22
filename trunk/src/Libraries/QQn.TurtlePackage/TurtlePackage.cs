using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using QQn.TurtleUtils.IO;
using QQn.TurtleUtils.Tokens;
using System.Net;
using System.Net.Cache;

namespace QQn.TurtlePackage
{
	public class TurtlePackage
	{
		public const string PackageFileType = "application/x-QQn-TurtlePackage"; 
		//readonly FileInfo _package;
		//readonly IXPathNavigable _manifest;

		public TurtlePackage()
		{
		}


		public static TurtlePackage Create(string fileName, Pack definition, string basePath)
		{
			if(fileName == null)
				throw new ArgumentNullException("fileName");

			if(definition == null)
				throw new ArgumentNullException("definition");

			AssuredStreamCreateArgs args = new AssuredStreamCreateArgs();
			if(definition.StrongNameKey != null)
				args.StrongNameKey = definition.StrongNameKey;

			args.FileType = PackageFileType;

			MultiStreamArgs fileCreateArgs = new MultiStreamArgs();
			fileCreateArgs.Assured = true;
			fileCreateArgs.GZipped = true;

			MultiStreamCreateArgs msca = new MultiStreamCreateArgs();
			msca.MaximumNumberOfStreams = 8;

			using(FileStream fs = File.Create(fileName, 16384))
			using(AssuredStream assurance = new AssuredStream(fs, args))
			using (MultiStreamWriter msw = new MultiStreamWriter(assurance))
			{
				MultiStreamArgs msa = new MultiStreamArgs();
				msa.StreamType = 0x00;
				msa.Assured = true;
				using (XmlWriter xw = new XmlTextWriter(msw.CreateStream(0x01), Encoding.UTF8))
				{
					xw.WriteStartDocument();
					xw.WriteStartElement("TurtlePackage", "http://schemas.qqn.nl/2007/TurtlePackage");
					Tokenizer.TryWriteXml(xw, definition);
					xw.WriteEndDocument();
				}

				MultiStreamCreateArgs zipcArgs = new MultiStreamCreateArgs();
				zipcArgs.MaximumNumberOfStreams = definition.Containers.Count;
				using (MultiStreamWriter zipBase = new MultiStreamWriter(msw.CreateStream(0x01)))
				{
					foreach (PackContainer container in definition.Containers)
					{
						MultiStreamCreateArgs ccArgs = new MultiStreamCreateArgs();
						ccArgs.MaximumNumberOfStreams = container.Files.Count;
						using (MultiStreamWriter containerWriter = new MultiStreamWriter(zipBase.CreateStream(0x02)))
						{
							foreach (PackFile file in container.Files)
							{
								using(FileStream fileSrc = File.OpenRead(Path.Combine(file.BaseDir, file.Name)))
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

		public static TurtlePackage OpenFrom(string file)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");

			Uri uri;

			if (Uri.TryCreate(file, UriKind.Absolute, out uri))
			{
				if (!uri.IsFile && !uri.IsUnc)
					return OpenFrom(uri);
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
				return Open(fs);
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
		/// <returns></returns>
		public static TurtlePackage OpenFrom(Uri uri)
		{
			return OpenFrom(uri, null);
		}

		/// <summary>
		/// Opens a TurtlePackage from the specified uri with the specified credentials
		/// </summary>
		/// <param name="uri">The URI.</param>
		/// <param name="credentials">The credentials.</param>
		/// <returns></returns>
		public static TurtlePackage OpenFrom(Uri uri, ICredentials credentials)
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

					return Open(sr);
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
		/// <returns></returns>
		static TurtlePackage Open(Stream stream)
		{
			stream.Close();

			return null;
		}

		/*protected TurtlePackage(string fileName, IXPathNavigable manifest)
			: this(new FileInfo(fileName), manifest)
		{
		}

		protected TurtlePackage(FileInfo info, IXPathNavigable manifest)
		{
			if(info == null)
				throw new ArgumentNullException("info");
			else if(manifest == null)
				throw new ArgumentNullException("manifest");

			_package = info;
			_manifest = manifest;
			_containers = new List<TurtleContainer>();
		}

		public XPathNavigator Manifest
		{
			get { return _manifest.CreateNavigator(); }
		}

		public FileInfo FileInfo
		{
			get { return _package; }
		}

		public ICollection<TurtleContainer> Containers
		{
			get { EnsureLoaded(); return _containers; }
		}

		protected void EnsureLoaded()
		{
			EnsureWritable();

			if (FileInfo.Exists)
				throw new InvalidOperationException();
		}

		public void ExtractTo(string path)
		{
			ExtractTo(path, new TurtleExtractArgs());
		}

		public void ExtractTo(string path, ICollection<string> containers)
		{
			ExtractTo(path, new TurtleExtractArgs(containers));
		}

		public void ExtractTo(string path, TurtleExtractArgs args)
		{
		}

		public static TurtlePackage CreateNew(string filename, IXPathNavigable nn)
		{
			TurtlePackage tp = new TurtlePackage(filename, nn);

			return tp;
		}

		public void Commit()
		{
			EnsureLoaded();
			throw new Exception("The method or operation is not implemented.");
		}

		public void SetManifest(XmlDocument doc)
		{
			throw new Exception("The method or operation is not implemented.");
		}*/
	}
}
