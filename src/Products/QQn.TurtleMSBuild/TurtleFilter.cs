using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
//using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using System.IO;
using System.Xml;
using System.Reflection;

namespace QQn.TurtleMSBuild
{
	static class TurtleFilter
	{
		const string Ns = "http://schemas.qqn.nl/2007/TurtleBuild/BuildResult";

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

					WriteGenerator(xw, project);

					WriteProjectInfo(xw, project);

					WriteReferences(xw, project);
					WriteProjectOutput(xw, project);
					WriteContent(xw, project);

					WriteScripts(xw, project);

					//WriteItem(xw, project, "ReferenceCopyLocalPaths");
				}
			}
		}

		static T GetAttribute<T>(ICustomAttributeProvider provider)
			where T : Attribute
		{
			object[] attrs = provider.GetCustomAttributes(typeof(T), false);

			if (attrs != null && attrs.Length > 0)
				return (T)attrs[0];
			else
				return null;
		}

		private static void WriteGenerator(XmlWriter xw, BuildProject project)
		{
			xw.WriteStartElement("Generator", Ns);
			Assembly assembly = Assembly.GetExecutingAssembly();

			string product = GetAttribute<AssemblyProductAttribute>(assembly).Product;
			string version = assembly.GetName().Version.ToString(4);

			xw.WriteAttributeString("name", typeof(MSBuildLogger).FullName);
			xw.WriteAttributeString("product", product);
			xw.WriteAttributeString("version", version);

			xw.WriteEndElement();
		}

		private static void WriteProjectInfo(XmlWriter xw, BuildProject project)
		{
			xw.WriteStartElement("Project", Ns);
			xw.WriteAttributeString("name", project.ProjectName);
			xw.WriteAttributeString("path", project.ProjectPath);
			xw.WriteAttributeString("configuration", project.Configuration);
			xw.WriteAttributeString("outputDir", project.OutDir);
			string keyFile;
			if (project.BuildProperties.TryGetValue("AssemblyOriginatorKeyFile", out keyFile))
			{
				xw.WriteAttributeString("keyFile", keyFile);
			}
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

		static IEnumerable<string> GetParameters(BuildProject project, string name, IEnumerable<string> furtherItems, string alternateValue)
		{
			Dictionary<string, string> used = new Dictionary<string, string>();

			if (furtherItems != null)
				foreach (string i in furtherItems)
				{
					if (!used.ContainsKey(i))
					{
						used.Add(i, i);
						yield return i;
					}
				}

			string propertyValue;
			if (!project.BuildProperties.TryGetValue("TurtleMSBuild_" + name, out propertyValue))
				propertyValue = alternateValue;

			if(propertyValue != null)
			{
				foreach (string i in propertyValue.Split(';'))
				{
					if (string.IsNullOrEmpty(i))
						continue;

					if (!used.ContainsKey(i))
					{
						used.Add(i, i);
						yield return i;
					}
				}
			}
		}

		private static void WriteProjectOutput(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> sharedItems = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, string> localItems = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, string> copyItems = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

			SortedList<string, string> keys = new SortedList<string, string>();
			SortedList<string, string> copyKeys = new SortedList<string, string>();

			foreach (string v in GetParameters(project, "SharedItems", project.Parameters.SharedItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, "Shared");
			}

			foreach (string v in GetParameters(project, "LocalItems", project.Parameters.LocalItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, "Item");
			}

			foreach (string v in GetParameters(project, "CopyItems", project.Parameters.LocalItems, "None;Compile;Content;EmbeddedResource"))
			{
				if (!copyKeys.ContainsKey(v))
					copyKeys.Add(v, "Copy");
			}

			string outDir = project.OutDir.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
			string outPath = Path.Combine(project.ProjectPath, outDir);
			string outPathS = Path.Combine(project.ProjectPath, outDir).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;

			if (project.BuildProperties.ContainsKey("_SGenDllCreated") && project.GetProperty("_SGenDllCreated") == "true" && project.BuildProperties.ContainsKey("_SGenDllName"))
			{
				string dllName = project.GetProperty("_SGenDllName");
				localItems[Path.Combine(project.OutDir, dllName)] = Path.Combine(project.IntermediateOutputPath, dllName);
			}

			if (project.BuildProperties.ContainsKey("_DebugSymbolsProduced") && project.GetProperty("_DebugSymbolsProduced") == "true")
			{
				string pdbName = project.GetProperty("TargetName") + ".pdb";
				localItems[Path.Combine(project.OutDir, pdbName)] = Path.Combine(project.IntermediateOutputPath, pdbName);
			}

			foreach (ProjectItem pi in project.BuildItems)
			{
				if (string.IsNullOrEmpty(pi.Include))
					continue;

				string include = pi.Include;
				string target;
				string destinationSubDirectory;
				string copyCondition;

				if (pi.TryGetMetaData("TargetPath", out target))
					target = Path.Combine(outDir, target);
				else if (pi.TryGetMetaData("DestinationSubDirectory", out destinationSubDirectory))
					target = Path.Combine(Path.Combine(outDir, destinationSubDirectory), pi.Filename);
				else
					target = Path.Combine(outDir, pi.Filename);

				if (Path.IsPathRooted(include))
					include = project.MakeRelativePath(include);

				switch (pi.Name)
				{
					case "IntermediateAssembly":
						localItems[target] = include;
						break;
					case "AddModules":
					case "DocFileItem":
					case "IntermediateSatelliteAssembliesWithTargetPath":
						// Default Microsoft items
						if (!localItems.ContainsKey(target))
							localItems.Add(target, include);
						break;
					case "ContentWithTargetPath":
					case "AllItemsFullPathWithTargetPath":
						if (!copyItems.ContainsKey(target))
						{
							string condition;
							if (pi.TryGetMetaData("CopyToOutputDirectory", out condition))
								switch (condition)
								{
									case "Always":
									case "PreserveNewest":
										copyItems.Add(target, include);
										break;
								}
						}
						break;
					case "ReferenceComWrappersToCopyLocal":
					case "ResolvedIsolatedComModules":
					case "_DeploymentLooseManifestFile":
					case "ReferenceCopyLocalPaths":
					case "NativeReferenceFile":
						if (!sharedItems.ContainsKey(target))
							sharedItems.Add(target, include);
						break;
					case "ReferenceCopyLocal":
						if (!copyItems.ContainsKey(target))
							copyItems.Add(target, include);
						break;
					default:
						if (keys.ContainsKey(pi.Name))
							switch (keys[pi.Name])
							{
								case "Shared":
									if (!sharedItems.ContainsKey(target))
										sharedItems.Add(target, pi.Include);
									break;
								case "Item":
									if (!localItems.ContainsKey(target))
										localItems.Add(target, pi.Include);
									break;
							}
						break;
				}

				if (copyKeys.ContainsKey(pi.Name))
					switch (copyKeys[pi.Name])
					{
						case "Copy":
							if (pi.TryGetMetaData("CopyToOutputDirectory", out copyCondition) && !copyItems.ContainsKey(target))
								switch (copyCondition)
								{
									case "Always":
									case "PreserveNewest":
										copyItems.Add(target, include);
										break;
								}
							break;
						default:
							break;
					}
			}

			xw.WriteStartElement("ProjectOutput");

			foreach (KeyValuePair<string, string> v in localItems)
			{
				if (!sharedItems.ContainsKey(v.Key))
				{
					xw.WriteStartElement("Item", Ns);
					xw.WriteAttributeString("src", v.Key);
					xw.WriteAttributeString("fromSrc", v.Value);

					xw.WriteEndElement();
				}
			}

			if ((xw.Settings != null) && xw.Settings.Indent)
				xw.WriteComment("Copy Items");

			foreach (KeyValuePair<string, string> v in copyItems)
			{
				if (!sharedItems.ContainsKey(v.Key) && !localItems.ContainsKey(v.Key))
				{
					xw.WriteStartElement("Copy", Ns);
					xw.WriteAttributeString("src", v.Key);
					xw.WriteAttributeString("fromSrc", v.Value);

					xw.WriteEndElement();
				}
			}

			if ((xw.Settings != null) && xw.Settings.Indent)
				xw.WriteComment("Shared Items");

			foreach (KeyValuePair<string, string> v in sharedItems)
			{
				xw.WriteStartElement("Shared", Ns);
				xw.WriteAttributeString("src", v.Key);
				xw.WriteAttributeString("fromSrc", v.Value);

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

		private static void WriteScripts(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> items = new SortedList<string, string>();
			SortedList<string, string> extensions = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, string> added = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string v in GetParameters(project, "ScriptItems", project.Parameters.ScriptItems, "Content;Compile;EmbeddedResource;None"))
			{
				if(!items.ContainsKey(v))
					items.Add(v, "Item");
			}

			foreach (string v in GetParameters(project, "ScriptExtensions", project.Parameters.ScriptExtensions, null))
			{
				string ext = v;

				if (!ext.StartsWith("."))
					ext = '.' + ext;

				if(!extensions.ContainsKey(ext))
					extensions.Add(ext, "Item");
			}
			
			xw.WriteStartElement("Scripts");
			foreach (ProjectItem i in project.BuildItems)
			{
				string name;
				if (!items.TryGetValue(i.Name, out name))
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
