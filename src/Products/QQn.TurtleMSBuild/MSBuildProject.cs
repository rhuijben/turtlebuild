using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Build.Framework;
using QQn.TurtleBuildUtils;

namespace QQn.TurtleMSBuild
{
	class MSBuildProject : Project
	{
		readonly string _targetNames;
		readonly IEnumerable _properties;
		readonly IEnumerable _items;
		readonly TurtleParameters _parameters;

		public MSBuildProject(string projectFile, string targetNames, IEnumerable properties, IEnumerable items, TurtleParameters parameters)
			: base(projectFile, parameters)
		{
			if (targetNames == null)
				throw new ArgumentNullException("targetNames");
			else if (properties == null)
				throw new ArgumentNullException("properties");
			else if (items == null)
				throw new ArgumentNullException("items");
			else if (parameters == null)
				throw new ArgumentNullException("parameters");

			_targetNames = targetNames;
			_properties = properties;
			_items = items;
			_parameters = parameters;
		}

		public string BuildTargetName
		{
			get { return _targetNames; }
		}

		internal IEnumerable PropertiesEnumerable
		{
			get { return _properties; }
		}

		internal IEnumerable ItemsEnumerable
		{
			get { return _items; }
		}

		SortedList<string, string> _buildProperties;

		List<ProjectItem> _buildItems = new List<ProjectItem>();

		public void Refresh()
		{
			SortedList<string, string> buildProperties = new SortedList<string, string>();
			foreach (DictionaryEntry o in PropertiesEnumerable)
			{
				buildProperties[(string)o.Key] = (string)o.Value;
			}

			List<ProjectItem> buildItems = new List<ProjectItem>();
			foreach (DictionaryEntry o in ItemsEnumerable)
			{
				buildItems.Add(new ProjectItem(this, (string)o.Key, (ITaskItem)o.Value));
			}

			_buildProperties = buildProperties;
			_buildItems = buildItems;

			// Default properties, set from Solution
			ProjectConfiguration = GetProperty("Configuration");
			ProjectPlatform = GetProperty("Platform");

			// Common properties
			OutputDir = GetProperty("OutputDir");
			TargetPlatform = GetProperty("PlatformTarget");
			ProcessorArchitecture = GetProperty("ProcessorArchitecture");			

			TargetName = GetProperty("TargetName");
			TargetExt = GetProperty("TargetExt");
		}

		public SortedList<string, string> BuildProperties
		{
			get { return _buildProperties; }
		}

		public List<ProjectItem> BuildItems
		{
			get { return _buildItems; }
		}

		public override void ParseBuildResult(Project parentProject)
		{
			base.ParseBuildResult(parentProject);

			Refresh();

			if (string.IsNullOrEmpty(TargetName))
				return;

			ParseProjectOutput();
		}

		private void ParseProjectOutput()
		{
			ProjectOutputList items = ProjectOutput;
			SortedFileList<bool> localCopyItems = new SortedFileList<bool>();
			SortedFileList<TargetType> keys = new SortedFileList<TargetType>();

			localCopyItems.BaseDirectory = ProjectPath;
			keys.BaseDirectory = ProjectPath;

			SortedList<string, bool> copyKeys = new SortedList<string, bool>();

			foreach (string v in GetParameters("SharedItems", Parameters.SharedItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, TargetType.SharedItem);
			}

			foreach (string v in GetParameters("LocalItems", Parameters.LocalItems, ""))
			{
				if (!keys.ContainsKey(v))
					keys.Add(v, TargetType.Item);
			}

			foreach (string v in GetParameters("CopyItems", Parameters.LocalItems, "None;Compile;Content;EmbeddedResource"))
			{
				if (!copyKeys.ContainsKey(v))
					copyKeys.Add(v, true);
			}

			string primaryTarget = Path.Combine(OutputDir, TargetName + TargetExt);
			string itemTarget = primaryTarget;
			items.Add(new TargetItem(itemTarget, Path.Combine(IntermediateOutputPath, TargetName + TargetExt), TargetType.Item));

			if (BuildProperties.ContainsKey("_SGenDllCreated") && GetProperty("_SGenDllCreated") == "true" && BuildProperties.ContainsKey("_SGenDllName"))
			{
				string dllName = GetProperty("_SGenDllName");
				itemTarget = Path.Combine(OutputDir, dllName);
				items.Add(new TargetItem(itemTarget, Path.Combine(IntermediateOutputPath, dllName), TargetType.Item));
			}

			if (BuildProperties.ContainsKey("_DebugSymbolsProduced") && GetProperty("_DebugSymbolsProduced") == "true")
			{
				string pdbName = GetProperty("TargetName") + ".pdb";
				itemTarget = Path.Combine(OutputDir, pdbName);
				items.Add(new TargetItem(itemTarget, Path.Combine(IntermediateOutputPath, pdbName), TargetType.Item));
			}

			foreach (ProjectItem pi in BuildItems)
			{
				if (string.IsNullOrEmpty(pi.Include))
					continue;				

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
						string condition;
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
						string fusionName = pi.GetMetadata("FusionName");
						if (!string.IsNullOrEmpty(fusionName))
							References.Add(new AssemblyReference(fusionName, pi, null));

						type = TargetType.SharedItem;
						break;
					default:
						if (!keys.TryGetValue(pi.Name, out type))
							type = TargetType.None;
						break;
				}

				if (type != TargetType.None)
				{					
					string target = CalculateTarget(pi);

					if (!items.ContainsKey(target))
						items.Add(new TargetItem(EnsureRelativePath(target), EnsureRelativePath(pi.Include), type, pi));
				}

				if (copyKeys.ContainsKey(pi.Name))
				{
					string copyCondition;
					if (pi.TryGetMetaData("CopyToOutputDirectory", out copyCondition))
						switch (copyCondition)
						{
							case "Always":
							case "PreserveNewest":
								{
									localCopyItems[pi.Include] = true;
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
		}

		private string CalculateTarget(ProjectItem pi)
		{
			string target;
			string destinationSubDirectory;

			if (pi.TryGetMetaData("TargetPath", out target))
				return Path.Combine(OutputDir, target);
			else if (pi.TryGetMetaData("DestinationSubDirectory", out destinationSubDirectory))
				return Path.Combine(Path.Combine(OutputDir, destinationSubDirectory), pi.Filename);
			else
				return Path.Combine(OutputDir, pi.Filename);
		}

		string _intermediateOutputPath;
		public string IntermediateOutputPath
		{
			get { return _intermediateOutputPath ?? (_intermediateOutputPath = EnsureRelativePath(GetProperty("IntermediateOutputPath"))); }
		}

		public string GetProperty(string key)
		{
			if (string.IsNullOrEmpty(key))
				throw new ArgumentNullException("key");

			if (BuildProperties == null)
				return null;

			string r;

			if (BuildProperties.TryGetValue(key, out r))
				return r;
			else
				return null;
		}

		protected override void WriteAssemblyInfo(XmlWriter xw, bool forReadability)
		{
			if (TargetAssembly == null)
			{
				try
				{
					AssemblyName name = AssemblyName.GetAssemblyName(Path.Combine(ProjectPath, TargetPath));
					if (name != null)
						this.TargetAssembly = new AssemblyReference(name.FullName, null, this);
				}
				catch
				{ }
			}
			base.WriteAssemblyInfo(xw, forReadability);
		}

		/// <summary>
		/// Writes the project info.
		/// </summary>
		/// <param name="xw">The xw.</param>
		/// <param name="forReadability">if set to <c>true</c> [for readability].</param>
		protected override void WriteProjectInfo(XmlWriter xw, bool forReadability)
		{
			string key;
			if (BuildProperties.TryGetValue("AssemblyOriginatorKeyFile", out key))
			{
				KeyFile = key;
			}
			if (BuildProperties.TryGetValue("AssemblyKeyContainerName", out key))
			{
				KeyContainer = key;
			}

			base.WriteProjectInfo(xw, forReadability);
		}

		protected override void WriteProjectReferences(XmlWriter xw, bool forReadability)
		{
			base.WriteProjectReferences(xw, forReadability);

			foreach (ProjectItem i in BuildItems)
			{
				bool isProject = false;
				if (i.Name == "Reference")
				{
					xw.WriteStartElement("Reference");
				}
				else if (i.Name == "ProjectReference")
				{
					xw.WriteStartElement("Project");
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
		}

		static readonly string _dotSlash = "." + Path.DirectorySeparatorChar;

		protected override void WriteProjectOutput(XmlWriter xw, bool forReadability)
		{
			base.WriteProjectOutput(xw, forReadability);
		}

		void UpdateContent()
		{
			SortedList<string, string> keys = new SortedList<string, string>();

			foreach (string v in GetParameters("ContentItems", Parameters.ContentItems, "Content"))
			{
				keys.Add(v, "Item");
			}

			foreach (ProjectItem i in BuildItems)
			{
				if (i.Name.StartsWith("_"))
					continue;

				string name;
				if (!keys.TryGetValue(i.Name, out name))
					continue;

				ContentFiles.AddUnique(i.Include);
			}
		}

		protected override void WriteContent(XmlWriter xw, bool forReadability)
		{
			UpdateContent();
			base.WriteContent(xw, forReadability);
		}

		void UpdateScripts()
		{
			SortedList<string, string> items = new SortedList<string, string>();
			SortedList<string, string> extensions = new SortedList<string, string>(StringComparer.InvariantCultureIgnoreCase);

			foreach (string v in GetParameters("ScriptItems", Parameters.ScriptItems, "Content;Compile;EmbeddedResource;None"))
			{
				if (!items.ContainsKey(v))
					items.Add(v, "Item");
			}

			foreach (string v in GetParameters("ScriptExtensions", Parameters.ScriptExtensions, null))
			{
				string ext = v;

				if (!ext.StartsWith("."))
					ext = '.' + ext;

				if (!extensions.ContainsKey(ext))
					extensions.Add(ext, "Item");
			}

			foreach (ProjectItem i in BuildItems)
			{
				string name;
				if (!items.TryGetValue(i.Name, out name))
					continue;

				string extension = Path.GetExtension(i.Include);

				if (!extensions.ContainsKey(extension))
					continue;

				ScriptFiles.AddUnique(i.Include);
			}
		}

		protected override void WriteScripts(XmlWriter xw, bool forReadability)
		{
			UpdateScripts();

			base.WriteScripts(xw, forReadability);
		}

		#region /// Helper methods
		IEnumerable<string> GetParameters(string name, IEnumerable<string> furtherItems, string alternateValue)
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
			if (!BuildProperties.TryGetValue("TurtleMSBuild_" + name, out propertyValue))
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
		#endregion
	}
}
