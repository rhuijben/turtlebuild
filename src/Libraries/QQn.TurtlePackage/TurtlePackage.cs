using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using QQn.TurtleUtils.ItemSets;

namespace QQn.TurtlePackage
{
	public class TurtlePackage : Pack
	{
		//readonly FileInfo _package;
		//readonly IXPathNavigable _manifest;

		public TurtlePackage()
		{
		}


		public static TurtlePackage Create(string fileName, Pack definition)
		{
			return null;
		}

		public static TurtlePackage LoadFrom(Uri uri)
		{
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
