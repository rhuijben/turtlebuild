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
			xw.WriteAttributeString("targetName", project.TargetName);
			xw.WriteAttributeString("targetExt", project.TargetExt);
			string keyFile;
			if (project.BuildProperties.TryGetValue("AssemblyOriginatorKeyFile", out keyFile))
			{
				xw.WriteAttributeString("keyFile", keyFile);
			}
			xw.WriteEndElement();

			string buildManifest;
			if (project.BuildProperties.TryGetValue("BuildManifest", out buildManifest))
			{
				if (!File.Exists(buildManifest))
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

			if (propertyValue != null)
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

		enum TargetType
		{
			None,
			Item,
			Copy,
			Shared,
			SharedCopy
		}

		sealed class TargetItem
		{
			readonly string _target;
			readonly string _include;
			readonly ProjectItem _projectItem;
			TargetType _targetType;

			public TargetItem(string targetPath, string include, TargetType targetType)
				: this(targetPath, include, targetType, null)
			{
			}
			public TargetItem(string targetPath, string include, TargetType targetType, ProjectItem projectItem)
			{
				_target = targetPath;
				_include = include;
				_targetType = targetType;
				_projectItem = projectItem;
			}

			public ProjectItem Item
			{
				get { return _projectItem; }
			}

			public string Include
			{
				get { return _include; }
			}

			public string Target
			{
				get { return _target; }
			}

			public TargetType Type
			{
				get { return _targetType; }
				set { _targetType = value; }
			}
		}

		static readonly string _dotSlash = "." + Path.DirectorySeparatorChar;

		private static void WriteProjectOutput(XmlWriter xw, BuildProject project)
		{
			SortedList<string, TargetItem> items = new SortedList<string, TargetItem>(StringComparer.InvariantCultureIgnoreCase);
			SortedList<string, bool> localCopyItems = new SortedList<string, bool>(StringComparer.InvariantCultureIgnoreCase);

			SortedList<string, TargetType> keys = new SortedList<string, TargetType>();
			SortedList<string, bool> copyKeys = new SortedList<string, bool>();

			foreach (string v in GetParameters(project, "SharedItems", project.Parameters.SharedItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, TargetType.Shared);
			}

			foreach (string v in GetParameters(project, "LocalItems", project.Parameters.LocalItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, TargetType.Item);
			}

			foreach (string v in GetParameters(project, "CopyItems", project.Parameters.LocalItems, "None;Compile;Content;EmbeddedResource"))
			{
				if (!copyKeys.ContainsKey(v))
					copyKeys.Add(v, true);
			}

			string target = Path.Combine(project.OutDir, project.TargetName + project.TargetExt);
			items.Add(target, new TargetItem(target, Path.Combine(project.IntermediateOutputPath, project.TargetName + project.TargetExt), TargetType.Item));

			if (project.BuildProperties.ContainsKey("_SGenDllCreated") && project.GetProperty("_SGenDllCreated") == "true" && project.BuildProperties.ContainsKey("_SGenDllName"))
			{
				string dllName = project.GetProperty("_SGenDllName");
				target = Path.Combine(project.OutDir, dllName);
				items.Add(target, new TargetItem(target, Path.Combine(project.IntermediateOutputPath, dllName), TargetType.Item));
			}

			if (project.BuildProperties.ContainsKey("_DebugSymbolsProduced") && project.GetProperty("_DebugSymbolsProduced") == "true")
			{
				string pdbName = project.GetProperty("TargetName") + ".pdb";
				target = Path.Combine(project.OutDir, pdbName);
				items.Add(target, new TargetItem(target, Path.Combine(project.IntermediateOutputPath, pdbName), TargetType.Item));
			}

			foreach (ProjectItem pi in project.BuildItems)
			{
				if (string.IsNullOrEmpty(pi.Include))
					continue;

				string include = pi.Include;

				string destinationSubDirectory;
				string copyCondition;

				if (pi.TryGetMetaData("TargetPath", out target))
					target = Path.Combine(project.OutDir, target);
				else if (pi.TryGetMetaData("DestinationSubDirectory", out destinationSubDirectory))
					target = Path.Combine(Path.Combine(project.OutDir, destinationSubDirectory), pi.Filename);
				else
					target = Path.Combine(project.OutDir, pi.Filename);

				if (Path.IsPathRooted(include) || include.Contains(_dotSlash))
					include = project.MakeRelativePath(include);

				if (Path.IsPathRooted(target) || target.Contains(_dotSlash))
					target = project.MakeRelativePath(include);

				if (!items.ContainsKey(target))
				{
					string condition;
					TargetType type = TargetType.None;
					switch (pi.Name)
					{
						// TODO: Rewrite to a per-language infrastructure
						case "IntermediateAssembly":
						case "AddModules":
						case "DocFileItem":
						case "IntermediateSatelliteAssembliesWithTargetPath":
							type = TargetType.Item;
							break;
						case "ContentWithTargetPath":
						case "AllItemsFullPathWithTargetPath":
						case "ReferenceCopyLocal":
							if (pi.TryGetMetaData("CopyToOutputDirectory", out condition))
							{
								switch (condition)
								{
									case "Always":
									case "PreserveNewest":
										type = TargetType.SharedCopy;
										break;
									default:
										break;
								}
							}
							if (type == TargetType.None)
								goto default;
							break;
						case "ReferenceComWrappersToCopyLocal":
						case "ResolvedIsolatedComModules":
						case "_DeploymentLooseManifestFile":
						case "ReferenceCopyLocalPaths":
						case "NativeReferenceFile":
							type = TargetType.Shared;
							break;
						default:
							if (!keys.TryGetValue(pi.Name, out type))
								type = TargetType.None;
							break;
					}

					if (type != TargetType.None)
						items.Add(target, new TargetItem(target, include, type, pi));
				}

				if (copyKeys.ContainsKey(pi.Name))
				{
					if (pi.TryGetMetaData("CopyToOutputDirectory", out copyCondition))
						switch (copyCondition)
						{
							case "Always":
							case "PreserveNewest":
								{
									localCopyItems[include] = true;
								}
								break;
						}

				}
			}

			foreach (TargetItem ti in items.Values)
			{
				if (ti.Type == TargetType.SharedCopy)
				{
					if (localCopyItems.ContainsKey(ti.Include))
						ti.Type = TargetType.Copy;
				}
			}

			xw.WriteStartElement("ProjectOutput");

			bool writeComments = (xw.Settings != null) && xw.Settings.Indent;
			if (writeComments)
				xw.WriteComment("Project Items");

			foreach (TargetItem ti in items.Values)
			{
				if (ti.Type == TargetType.Item)
					WriteProjectOutputItem(xw, ti);
			}

			if (writeComments)
				xw.WriteComment("Project Copy Items");

			foreach (TargetItem ti in items.Values)
			{
				if (ti.Type == TargetType.Copy)
				{
					WriteProjectOutputItem(xw, ti);
				}
			}

			if (writeComments)
				xw.WriteComment("Shared Items");

			foreach (TargetItem ti in items.Values)
			{
				if (ti.Type == TargetType.Shared)
					WriteProjectOutputItem(xw, ti);
			}

			if (writeComments)
				xw.WriteComment("Shared Copy Items");

			foreach (TargetItem ti in items.Values)
			{
				if (ti.Type == TargetType.SharedCopy)
					WriteProjectOutputItem(xw, ti);
			}

			xw.WriteEndElement();
		}

		private static void WriteProjectOutputItem(XmlWriter xw, TargetItem ti)
		{
			xw.WriteStartElement(ti.Type.ToString(), Ns);
			xw.WriteAttributeString("src", ti.Target);
			xw.WriteAttributeString("fromSrc", ti.Include);

			xw.WriteEndElement();
		}

		private static void WriteContent(XmlWriter xw, BuildProject project)
		{
			SortedList<string, string> keys = new SortedList<string, string>();

			foreach (string v in GetParameters(project, "ContentItems", project.Parameters.ContentItems, "Content"))
			{
				keys.Add(v, "Item");
			}

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
				if (!items.ContainsKey(v))
					items.Add(v, "Item");
			}

			foreach (string v in GetParameters(project, "ScriptExtensions", project.Parameters.ScriptExtensions, null))
			{
				string ext = v;

				if (!ext.StartsWith("."))
					ext = '.' + ext;

				if (!extensions.ContainsKey(ext))
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

				added.Add(i.Include, name);

				xw.WriteStartElement(extensionAs, Ns);
				xw.WriteAttributeString("src", i.Include);

				xw.WriteEndElement();
			}
			xw.WriteEndElement();
		}
	}
}
