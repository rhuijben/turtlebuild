using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using System.IO;

namespace QQn.TurtlePackage
{
	public class TurtlePackage : TurtleItem
	{
		readonly FileInfo _package;
		readonly IXPathNavigable _manifest;
		internal List<TurtleContainer> _containers;

		protected TurtlePackage(string fileName, IXPathNavigable manifest)
			: this(new FileInfo(fileName), manifest)
		{
		}

		protected TurtlePackage(FileInfo info, IXPathNavigable manifest)
			: base(true)
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
			if (!ReadOnly)
				return;
			else if (FileInfo.Exists)
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

		public TurtleContainer AddContainer(string containerName)
		{
			foreach (TurtleContainer tc in Containers)
			{
				if (tc.Name == containerName)
					throw new ArgumentException("Container already exists", "containerName");
			}

			TurtleContainer tt = new TurtleContainer(false, containerName);
			_containers.Add(tt);

			return tt;
		}

		public void Commit()
		{
			EnsureLoaded();
			throw new Exception("The method or operation is not implemented.");
		}

		public void SetManifest(XmlDocument doc)
		{
			throw new Exception("The method or operation is not implemented.");
		}



		public override string Name
		{
			get { return _package.Name; }
		}
	}
}
