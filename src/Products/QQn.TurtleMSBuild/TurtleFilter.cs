using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
//using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;

namespace QQn.TurtleMSBuild
{
	static class TurtleFilter
	{
		const string Ns = "http://schemas.qqn.nl/2007/TurtleBuild/BuildLog";

		internal static void Execute(BuildProject project)
		{
			if (project == null)
				throw new ArgumentNullException("project");

			if (project.OutDir == null)
				return;

			string outDir = project.Parameters.OutputDir ?? Path.Combine(project.ProjectPath, project.OutDir);

			if (!Directory.Exists(outDir))
				Directory.CreateDirectory(outDir);

			string atPath = Path.Combine(outDir, project.ProjectName + ".tbLog");

			using (StreamWriter sw = new StreamWriter(atPath, false, Encoding.UTF8))
			{
				XmlWriterSettings xs = new XmlWriterSettings();

				xs.Indent = project.Parameters.Indent;

				using (XmlWriter xw = XmlWriter.Create(sw, xs))
				{
					xw.WriteStartDocument();
					xw.WriteStartElement(null, "TurtleBuild", Ns);

					WriteProjectInfo(xw, project);

					WriteReferences(xw, project);
					WriteResources(xw, project);
					WriteContent(xw, project);

					WriteSpecialFiles(xw, project);
				}
			}
		}

		private static void WriteProjectInfo(XmlWriter xw, BuildProject project)
		{
			xw.WriteStartElement("Project", Ns);
			xw.WriteAttributeString("name", project.ProjectName);
			xw.WriteAttributeString("path", project.ProjectPath);
			xw.WriteAttributeString("configuration", project.Configuration);
			xw.WriteAttributeString("outputDir", project.OutDir);
			xw.WriteEndElement();

			string buildManifest;
			if (project.BuildProperties.TryGetValue("BuildManifest", out buildManifest))
			{
				if(!File.Exists(buildManifest))
					throw new FileNotFoundException("BuildManifest specified in the BuildManifestProperty not found", buildManifest);

				XmlDocument doc = new XmlDocument();
				doc.Load(buildManifest);

				xw.WriteStartElement("BuildManifest", Ns);

				doc.DocumentElement.WriteContentTo(xw);

				xw.WriteEndElement();
			}
		}

		private static void WriteReferences(XmlWriter xw, BuildProject project)
		{
			xw.WriteStartElement("References");
			foreach (ProjectItem i in project.BuildItems)
			{
				bool isProject = false;
				if (i.Name == "Reference")
				{
					xw.WriteStartElement("Reference", Ns);
				}
				else if (i.Name == "ProjectReference")
				{
					xw.WriteStartElement("Project", Ns);
					isProject = true;
				}
				else
					continue;

				xw.WriteAttributeString(isProject ? "src" : "assembly", i.Include);
				if (i.HasMetadata("Name"))
					xw.WriteAttributeString("name", i.GetMetadata("Name"));
				if (i.HasMetadata("HintPath"))
					xw.WriteAttributeString("src", i.GetMetadata("HintPath"));

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}

		
		private static void WriteResources(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> keys = new SortedList<string,string>();

			string sharedKeys = project.GetProperty("TurtleLogger_SharedItemNames") ?? "FileWritesShareable";
			string publishedKeys = project.GetProperty("TurtleLogger_ItemNames") ?? "FileWrites";

			bool outDirOnly = string.Equals(project.GetProperty("TurtleLogger_OutDirOnly"), "true", StringComparison.InvariantCultureIgnoreCase);
			
			foreach(string n in sharedKeys.Split(';'))
				keys.Add(n, "Shared");

			foreach(string n in publishedKeys.Split(';'))
					keys.Add(n, "Item");

			SortedList<string, string> added = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			xw.WriteStartElement("Resources");
			foreach (ProjectItem i in project.BuildItems)
			{
				if (i.Name.StartsWith("_"))
					continue;

				if (i.Include.StartsWith(project.IntermediateOutputPath))
				{
					// Never copy from intermediate path
					continue;
				}
				else if(outDirOnly && !i.Include.StartsWith(project.OutDir))
				{
					continue;
				}

				string name;
				if (!keys.TryGetValue(i.Name, out name))
					continue;

				string include = i.Include;

				if (Path.IsPathRooted(include))
				{
					include = project.MakeRelativePath(include);	
				}
				
				if(added.ContainsKey(include))
					continue;

				if(!File.Exists(Path.Combine(project.ProjectPath, include)))
				{
					xw.WriteComment(name +": " + include);
					continue;
				}

				added.Add(include, name);

				xw.WriteStartElement(name, Ns);
				xw.WriteAttributeString("src", include);
				
				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}

		private static void WriteContent(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> keys = new SortedList<string, string>();

			string contentKeys = project.GetProperty("TurtleLogger_ContentItemNames") ?? "Content";

			foreach (string n in contentKeys.Split(';'))
				keys.Add(n, "Item");

			SortedList<string, string> added = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			xw.WriteStartElement("Content");
			foreach (ProjectItem i in project.BuildItems)
			{
				if (i.Name.StartsWith("_"))
					continue;

				string name;
				if (!keys.TryGetValue(i.Name, out name))
					continue;

				if (added.ContainsKey(i.Include))
					continue;

				if (!File.Exists(Path.Combine(project.ProjectPath, i.Include)))
				{
					xw.WriteComment(name + ": " + i.Include);
					continue;
				}

				added.Add(i.Include, name);

				xw.WriteStartElement(name, Ns);
				xw.WriteAttributeString("src", i.Include);

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}

		private static void WriteSpecialFiles(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> keys = new SortedList<string, string>();

			string scriptKeys = project.GetProperty("TurtleLogger_ScriptItemNames") ?? "Content;None";

			foreach (string n in scriptKeys.Split(';'))
				keys.Add(n, "Item");

			SortedList<string, string> extensions = new SortedList<string, string>();

			if(project.Parameters.ScriptExtensions != null)
				foreach (string n in project.Parameters.ScriptExtensions)
				{
					string[] parts = n.Split('=');
					extensions.Add(parts[0], (parts.Length > 1) ? parts[1] : "Item");
				}

			SortedList<string, string> added = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			xw.WriteStartElement("Scripts");
			foreach (ProjectItem i in project.BuildItems)
			{
				if (i.Name.StartsWith("_"))
					continue;

				string name;
				if (!keys.TryGetValue(i.Name, out name))
					continue;

				if (added.ContainsKey(i.Include))
					continue;

				string extension = Path.GetExtension(i.Include);

				string extensionAs;
				if (!extensions.TryGetValue(extension, out extensionAs))
					continue;

				if (!File.Exists(Path.Combine(project.ProjectPath, i.Include)))
				{
					xw.WriteComment(name + ": " + i.Include);
					continue;
				}

				added.Add(i.Include, name);

				xw.WriteStartElement(extensionAs, Ns);
				xw.WriteAttributeString("src", i.Include);

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}
	}
}
