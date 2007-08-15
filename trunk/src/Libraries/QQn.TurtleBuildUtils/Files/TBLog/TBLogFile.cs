using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtleUtils.Tokenizer;
using System.Xml;
using System.Xml.XPath;

namespace QQn.TurtleBuildUtils.Files.TBLog
{
	public class TBLogFile
	{
		[TokenGroup("Generator")]
		public TBLogGenerator Generator = new TBLogGenerator();

		[TokenGroup("Project")]
		public TBLogProject Project = new TBLogProject();

		[TokenGroup("References")]
		public TBLogReferences References = new TBLogReferences();

		[TokenGroup("ProjectOutput")]
		public TBLogProjectOutput ProjectOutput = new TBLogProjectOutput();

		[TokenGroup("Content")]
		public TBLogContent Content = new TBLogContent();

		[TokenGroup("Scripts")]
		public TBLogScripts Scripts = new TBLogScripts();


		public static TBLogFile Load(string path)
		{
			using (XmlReader xr = XmlReader.Create(path))
			{
				TBLogFile file;

				XPathDocument doc = new XPathDocument(xr);

				XPathNavigator nav = doc.CreateNavigator();
				nav.MoveToRoot();
				nav.MoveToFirstChild();

				TokenizerArgs args = new TokenizerArgs();
				args.SkipUnknownNamedItems = true;

				if (Tokenizer.TryParseXml(nav, args, out file))
				{
					return file;
				}
				else
					return null;
			}
		}
	}
}
