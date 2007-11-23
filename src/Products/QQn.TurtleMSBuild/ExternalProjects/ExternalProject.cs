using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using QQn.TurtleUtils.IO;
using System.Reflection;
using QQn.TurtleBuildUtils;

namespace QQn.TurtleMSBuild.ExternalProjects
{
	abstract class ExternalProject : Project
	{
		readonly Guid _projectGuid;
		readonly List<ProjectItem> _buildItems;
		readonly ReferenceList _references = new ReferenceList();

		protected ExternalProject(Guid projectGuid, string projectFile, string projectName, TurtleParameters parameters)
			: base(projectFile, projectName, parameters)
		{
			_projectGuid = projectGuid;
			_buildItems = new List<ProjectItem>();
			ProjectType = "External";
			_projectReferences = new SortedFileList();
			_projectReferences.BaseDirectory = ProjectPath;
		}

		/// <summary>
		/// Gets the project GUID.
		/// </summary>
		/// <value>The project GUID.</value>
		public Guid ProjectGuid
		{
			get { return _projectGuid; }
		}

		/// <summary>
		/// Gets the build items.
		/// </summary>
		/// <value>The build items.</value>
		public List<ProjectItem> BuildItems
		{
			get { return _buildItems; }
		}

		/// <summary>
		/// Gets or sets the full project configuration.
		/// </summary>
		/// <value>The full project configuration.</value>
		public virtual string FullProjectConfiguration
		{
			get { return ProjectConfiguration; }
			set { ProjectConfiguration = value; }
		}


		Type _resolveAssemblyReferenceType;
		Type ResolveAssemblyReferenceType
		{
			get
			{
				if (_resolveAssemblyReferenceType == null)
				{
					Type resolveAssemblyReferenceType;

					string name = typeof(ITask).Assembly.FullName.Contains("Version=3.5")
						? "Microsoft.Build.Tasks.ResolveAssemblyReference, Microsoft.Build.Tasks.v3.5, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
						: "Microsoft.Build.Tasks.ResolveAssemblyReference, Microsoft.Build.Tasks, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";

					resolveAssemblyReferenceType = Type.GetType(name, false);

					if(typeof(ITask).IsAssignableFrom(resolveAssemblyReferenceType))
						_resolveAssemblyReferenceType = resolveAssemblyReferenceType;
				}
				return _resolveAssemblyReferenceType;

			}
		}

		protected ITask _resolveAssemblyReferenceTask;
		protected ITask ResolveReferencesTask
		{
			get
			{
				if (_resolveAssemblyReferenceTask == null && ResolveAssemblyReferenceType != null)
				{
					_resolveAssemblyReferenceTask = (ITask)Activator.CreateInstance(ResolveAssemblyReferenceType);
					_resolveAssemblyReferenceTask.BuildEngine = new SimpleBuildEngine();
				}
				return _resolveAssemblyReferenceTask;
			}
		}

		protected static void SetTaskParameter(ITask task, string name, object value)
		{
			task.GetType().InvokeMember(name, System.Reflection.BindingFlags.SetProperty, null, task, new object[] { value });
		}

		protected static T GetTaskParameter<T>(ITask task, string name)
		{
			return (T)task.GetType().InvokeMember(name, System.Reflection.BindingFlags.GetProperty, null, task, null);
		}

		protected void ResolveAdditionalOutput()
		{
			ResolveAdditionalOutput(Assembly.ReflectionOnlyLoadFrom(GetFullPath(TargetPath)));
		}

		SortedFileList _projectReferences;
		public SortedFileList ProjectReferences
		{
			get { return _projectReferences ?? (_projectReferences = new SortedFileList()); }
		}

		protected override void WriteProjectReferences(System.Xml.XmlWriter xw, bool forReadability)
		{
			base.WriteProjectReferences(xw, forReadability);

			if (_projectReferences != null)
			{
				ProjectReferences.BaseDirectory = ProjectPath;

				foreach (string src in ProjectReferences.Keys)
				{
					xw.WriteStartElement("Project");
					xw.WriteAttributeString("src", src);
					xw.WriteEndElement();
				}
			}
		}

		protected void ResolveAdditionalOutput(Assembly asm)
		{
			List<string> searchPaths = new List<string>();

			searchPaths.Add(GetFullPath(TargetPath));
			List<string> candidates = new List<string>();

			List<ITaskItem> assemblyFiles = new List<ITaskItem>();
			SortedList<string, string> refAssemblies = new SortedList<string,string>(StringComparer.Ordinal);

			foreach (AssemblyName a in asm.GetReferencedAssemblies())
			{
				refAssemblies.Add(a.Name, a.FullName);
			}

			ITaskItem targetItem = new SimpleTaskItem(GetFullPath(TargetPath));
			assemblyFiles.Add(targetItem);

			foreach (ProjectItem pi in BuildItems)
			{
				string include = GetFullPath(pi.Include);

				if (File.Exists(include))
				{
					string path = Path.GetDirectoryName(include);

					if (!searchPaths.Contains(path))
						searchPaths.Add(path);

					if (!candidates.Contains(include))
						candidates.Add(include);

					string name = Path.GetFileNameWithoutExtension(include);
					if (refAssemblies.ContainsKey(name))
					{
						AssemblyName aname = AssemblyName.GetAssemblyName(include);
						string fullName = refAssemblies[name];

						if (aname.FullName == fullName)
						{
							SimpleTaskItem sti = new SimpleTaskItem(include);
							sti.SetMetadata("Private", "true"); // Set as Copy local, see Microsoft.Common.targets
							sti.SetMetadata("FusionName", fullName); // Used by writing ResolvedDependencyFiles
							assemblyFiles.Add(sti);
							refAssemblies.Remove(name);
							References.Add(new AssemblyReference(fullName, pi, this));
						}
					}
				}

				string origSpec = pi.GetMetadata("OriginalItemSpec");
				if (!string.IsNullOrEmpty(origSpec))
				{
					ProjectReferences.AddUnique(EnsureRelativePath(origSpec));
				}
			}

			foreach(KeyValuePair<string, string> k in refAssemblies)
			{
				References.Add(new AssemblyReference(k.Value, null, this));
			}

			/*foreach (AssemblyName a in refAssemblies)
			{
				string fileName = a.Name;

				string dllPath = GetFullPath(Path.Combine(OutputPath, fileName + ".dll"));
				if (File.Exists(dllPath))
				{
					SimpleTaskItem sti = new SimpleTaskItem(dllPath);
					sti.SetMetadata("Private", "true"); // Is a copy local item
				}
			}*/

			if (candidates.Count > 0)
			{
				searchPaths.Clear();				
				SetTaskParameter(ResolveReferencesTask, "CandidateAssemblyFiles", candidates.ToArray());
			}

			searchPaths.Insert(0, "{CandidateAssemblyFiles}");
			searchPaths.Add("{HintPathFromItem}");
			searchPaths.Add("{RawFileName}");

			SetTaskParameter(ResolveReferencesTask, "SearchPaths", searchPaths.ToArray());			
			SetTaskParameter(ResolveReferencesTask, "FindRelatedFiles", true);
			SetTaskParameter(ResolveReferencesTask, "FindSatellites", true);
			SetTaskParameter(ResolveReferencesTask, "FindSerializationAssemblies", true);

			SetTaskParameter(ResolveReferencesTask, "Silent", true);

			for (int i = 0; i < 2; i++)
			{
				bool local = (i == 0);

				SetTaskParameter(ResolveReferencesTask, "AssemblyFiles", local ? new ITaskItem[] { targetItem } : assemblyFiles.ToArray());
				SetTaskParameter(ResolveReferencesTask, "FindDependencies", !local);

				if (!ResolveReferencesTask.Execute())
					return;

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "ResolvedFiles"))
				{
					AddItem(local, TargetType.Item, item);
				}

				if (!local)
				{
					foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "ResolvedDependencyFiles"))
					{
						References.Add(new AssemblyReference(item.GetMetadata("FusionName"), item, this));
						AddItem(local, TargetType.Item, item);
					}
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "SatelliteFiles"))
				{
					AddItem(local, TargetType.Item, item);
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "ScatterFiles"))
				{
					AddItem(local, TargetType.Item, item);
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "SerializationAssemblyFiles"))
				{
					AddItem(local, TargetType.Item, item);
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "RelatedFiles"))
				{
					AddItem(local, TargetType.Item, item);
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "CopyLocalFiles"))
				{
					AddItem(local, TargetType.Copy, item);
				}

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "FilesWritten"))
				{
					AddItem(local, TargetType.Copy, item);
				}
			}
		}

		private void AddItem(bool local, TargetType type, ITaskItem item)
		{
			string path = item.ItemSpec;

			string src = EnsureRelativePath(path);
			string target = (item != null) ? EnsureRelativePath(CalculateTarget(item)) : src;

			if (!ProjectOutput.ContainsKey(target))
			{
				ProjectOutput.Add(new TargetItem(target, src, local ? type : (TargetType)((int)type | 0x10)));
			}
		}

		protected internal virtual void AddBuildConfiguration(string configuration)
		{
			FullProjectConfiguration = configuration;
		}
	}
}
