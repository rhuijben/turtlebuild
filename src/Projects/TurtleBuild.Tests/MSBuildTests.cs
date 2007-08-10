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

			Directory.CreateDirectory(LoggerPath);
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
				Assert.That(output, Is.EqualTo(""), "MSBuild gave no output");
				Assert.That(p.ExitCode, Is.EqualTo(0), "MSBuild ran successfully");
				
			}

			Assert.That(File.Exists(Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog")), Is.True, "Logfile created");

			XPathDocument doc = new XPathDocument(Path.Combine(LoggerPath, "QQn.TurtleMSBuild.tbLog"));

			XPathNavigator nav = doc.CreateNavigator();
			XmlNamespaceManager nsMgr = new XmlNamespaceManager(nav.NameTable);
			nsMgr.AddNamespace("tb", "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult");
			Assert.That(nav.SelectSingleNode("//tb:Project", nsMgr).GetAttribute("outputDir", ""), Is.Not.EqualTo(""), "Outputdir is set");
			//doc.CreateNavigator().SelectSingleNode("
		}

		[Test]
		public void BuildInternal()
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
	}
}