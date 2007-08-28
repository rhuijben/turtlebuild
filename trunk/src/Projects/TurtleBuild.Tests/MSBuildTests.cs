using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System.Diagnostics;
using System.Reflection;
using System.Xml.XPath;
using System.Xml;

using Microsoft.Build.BuildEngine;
using QQn.TurtleMSBuild;
using QQn.TurtleBuildUtils.Files.TBLog;
using QQn.TurtlePackage;
using QQn.TurtleUtils.IO;
using QQn.TurtleBuildUtils;

namespace TurtleTests
{
	[TestFixture]
	public class MSBuildTests
	{
		string _msBuild;

		public string MSBuild
		{
			get
			{
				if (_msBuild == null)
					_msBuild = Path.Combine(Path.GetDirectoryName(typeof(int).Module.FullyQualifiedName), "MSBuild.exe");
				return _msBuild;
			}
		}

		public string OtherConfiguration
		{
			get
			{
#if !DEBUG
				return "Debug";
#else
				return "Release";
#endif
			}
		}

		public string ThisConfiguration
		{
			get
			{
#if DEBUG
				return "Debug";
#else
				return "Release";
#endif
			}
		}

		[TestFixtureSetUp]
		public void Setup()
		{
			if (Directory.Exists(LoggerPath))
				Directory.Delete(LoggerPath, true);

			if (Directory.Exists(PackagePath))
				Directory.Delete(PackagePath, true);

			if (Directory.Exists(ExtractPath))
				Directory.Delete(ExtractPath, true);

			Directory.CreateDirectory(PackagePath);
			Directory.CreateDirectory(LoggerPath);
			Directory.CreateDirectory(ExtractPath);
		}

		string _testSolution;
		public string Solution
		{
			get
			{
				if (_testSolution == null)
				{
					DirectoryInfo di = new DirectoryInfo(Environment.CurrentDirectory);
					while(di.Parent.FullName != di.FullName)
					{
						foreach(FileInfo f in di.GetFiles("*.sln", SearchOption.AllDirectories))
						{
							return _testSolution = f.FullName;
						}
						di = di.Parent;
					}
					return _testSolution = "";
				}
				else
					return _testSolution;
			}
		}

		string _logger;
		public string Logger
		{
			get
			{
				if(_logger == null)
					_logger = Path.GetFullPath(Path.Combine(typeof(MSBuildTests).Module.FullyQualifiedName, string.Format("../../../../../Products/QQn.TurtleMSBuild/bin/{0}/QQn.TurtleMSBuild.dll", ThisConfiguration)));

				return _logger;
			}
		}

		string _loggerPath;
		public string LoggerPath
		{
			get
			{
				if (_loggerPath == null)
					_loggerPath = Path.Combine(Path.GetTempPath(), "BuildLogger");

				return _loggerPath;
			}
		}

		string _packagePath;
		public string PackagePath
		{
			get
			{
				if (_packagePath == null)
					_packagePath = Path.Combine(Path.GetTempPath(), "TBPackages");

				return _packagePath;
			}
		}

		string _extractPath;
		public string ExtractPath
		{
			get
			{
				if (_extractPath == null)
					_extractPath = Path.Combine(Path.GetTempPath(), "ExtractTBP");

				return _extractPath;
			}
		}

		[Test]
		public void MSBuildFound()
		{
			Assert.That(File.Exists(MSBuild), Is.True, "MSBuild exists");
			Assert.That(File.Exists(Solution), Is.True, "Solution exists");
			Assert.That(File.Exists(Logger), Is.True, "Logger exists");
			Assert.That(Directory.Exists(LoggerPath), Is.True, "Loggerpath exists");
		}

		[Test]
		public void BuildExternal()
		{
			ProcessStartInfo psi = new ProcessStartInfo(MSBuild, string.Format("/nologo \"{0}\" /v:q /p:Configuration={1} \"/logger:MSBuildLogger,{2};OutputDir={3};Indent=true\"", Solution, OtherConfiguration, Logger, LoggerPath));
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			psi.CreateNoWindow = true;

			using (Process p = Process.Start(psi))
			{
				p.WaitForExit();
				string output = p.StandardOutput.ReadToEnd();
				string err = p.StandardError.ReadToEnd();

				Assert.That(err, Is.EqualTo(""), "MSBuild gave no error");
				//Assert.That(output, Is.EqualTo(""), "MSBuild gave no output");
				Assert.That(p.ExitCode, Is.EqualTo(0), "MSBuild ran successfully");
				
			}

			Assert.That(File.Exists(Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog")), Is.True, "Logfile created");

			XPathDocument doc = new XPathDocument(Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog"));

			XPathNavigator nav = doc.CreateNavigator();
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
			nsMgr.AddNamespace("tb", "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult");
			Assert.That(nav.SelectSingleNode("//tb:Project", nsMgr).GetAttribute("outputDir", ""), Is.Not.EqualTo(""), "Outputdir is set");
			//doc.CreateNavigator().SelectSingleNode("

			TBLogFile file = TBLogFile.Load(Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog"));

			Assert.That(file, Is.Not.Null);

			using (StringWriter sw = new StringWriter())
			{
				using (XmlWriter xw = new XmlTextWriter(sw))
				{
					xw.WriteStartDocument();
					xw.WriteStartElement("TurtleBuild", "q:q");
					QQn.TurtleUtils.Tokens.Tokenizer.TryWriteXml(new XmlTextWriter(sw), file);

					xw.WriteEndDocument();
				}

				string text = sw.ToString();
				Assert.That(text.Length, Is.GreaterThan(100));
			}
		}

		void BuildInternal()
		{
			Engine engine = new Engine(Path.GetDirectoryName(new Uri(typeof(int).Assembly.CodeBase).LocalPath));
			MSBuildLogger logger = new MSBuildLogger();
			engine.GlobalProperties.SetProperty("Configuration", OtherConfiguration);
			logger.Parameters = string.Format("OutputDir={0};Indent=true", LoggerPath);
			engine.RegisterLogger(logger);

			Project p = new Project(engine);
			p.Load(Solution);
			Assert.That(p.Build(), Is.True, "Build succeeded");
		}

		[Test]
		public void BuildInternalSucceeded()
		{
			BuildInternal();			
		}

		[Test]
		public void MakePackage()
		{
			string logFile = Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog");
			if(!File.Exists(logFile))
				BuildInternal();

			TBLogFile log = TBLogFile.Load(logFile);

			DebugReference reference = null;
			foreach (TBLogItem item in log.ProjectOutput.Items)
			{
				if (!item.IsShared && !item.IsCopy)
				{
					switch (Path.GetExtension(item.Src).ToUpperInvariant())
					{
						case ".PDB":
						case ".DLL":
							if (reference == null)
							{
								reference = AssemblyUtils.GetDebugReference(item.FullFromSrc);

								Assert.That(reference, Is.Not.Null);
								Assert.That(reference.PdbFile, Is.Not.Null);
							}
							else
							{
								DebugReference dr = AssemblyUtils.GetDebugReference(item.FullFromSrc);

								Assert.That(dr, Is.Not.Null);
								// Path does not have to equal; the pdb information contains the sourcepath (obj directory for c# code)
								Assert.That(Path.GetFileName(dr.PdbFile), Is.EqualTo(Path.GetFileName(reference.PdbFile)));
								Assert.That(dr.DebugId, Is.EqualTo(reference.DebugId));
							}
							break;
					}
				}
			}

			Pack pack = null;

			Assert.That(PackUtils.TryCreatePack(log, out pack));

			string path = Path.Combine(PackagePath, "QQn.TurtleMSBuild.tbPkg");
			TurtlePackage.Create(Path.Combine(PackagePath, "QQn.TurtleMSBuild.tbPkg"), pack, log.Project.Path);

			using (TurtlePackage pkg = TurtlePackage.OpenFrom(path, VerificationMode.Full))
			using(DirectoryMap dm = DirectoryMap.Get(ExtractPath))
			{
				Assert.That(pkg, Is.Not.Null);

				pkg.ExtractTo(dm);
			}

			using (TurtlePackage pkg = TurtlePackage.OpenFrom(path, VerificationMode.Full))
			using (DirectoryMap dm = DirectoryMap.Get(ExtractPath))
			{
				Assert.That(pkg, Is.Not.Null);

				pkg.ExtractTo(dm);
			}
		}
	}
}