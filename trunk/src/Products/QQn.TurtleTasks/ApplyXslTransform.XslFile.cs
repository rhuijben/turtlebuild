using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Xsl;
using System.IO;
using System.Xml;

namespace QQn.TurtleTasks
{
	partial class ApplyXslTransform
	{
		class XslFile : XslFilename
		{
			DateTime _lastModified;

			XslCompiledTransform _transform;

			public XslFile(XslFilename filename)
				: base(filename)
			{
				_lastModified = DateTime.MinValue;
			}

			public void EnsureCompiled()
			{
				FileInfo fif = new FileInfo(Filename);

				if (!fif.Exists)
					throw new FileNotFoundException("Xsl not found", fif.FullName);

				if ((_transform != null) && fif.LastWriteTime == fif.LastWriteTime)
					return;


				XslCompiledTransform xc = new XslCompiledTransform(Debug);
				XmlUrlResolver resolver = new XmlUrlResolver();
				XsltSettings settings = new XsltSettings();


				settings.EnableDocumentFunction = true;
				settings.EnableScript = true;

				xc.Load(fif.FullName, settings, resolver);

				_lastModified = fif.LastWriteTime;
				_transform = xc;
			}

			public bool Equals(XslFile other)
			{
				return base.Equals(other);
			}

			public override bool Equals(object obj)
			{
				return Equals(obj as XslFile);
			}

			public override int GetHashCode()
			{
				return Filename.GetHashCode();
			}

			public XslCompiledTransform Transform
			{
				get 
				{
					if (_transform == null)
						throw new InvalidOperationException("Please call EnsureCompiled first");
					
					return _transform; 
				}
			}
		}
	}
}
