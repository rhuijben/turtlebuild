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
					Type resolveAssemblyReferenceType = Type.GetType("Microsoft.Build.Tasks.ResolveAssemblyReference, Microsoft.Build.Tasks.v3.5", false, false);
					if (resolveAssemblyReferenceType == null)
						resolveAssemblyReferenceType = Type.GetType("Microsoft.Build.Tasks.ResolveAssemblyReference, Microsoft.Build.Tasks", false, false);

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

		protected void ResolveToOutput(ITaskItem[] files)
		{
			ITaskItem primaryOutput = files[0];

			List<string> searchPaths = new List<string>();

			searchPaths.Add(Path.GetDirectoryName(primaryOutput.ItemSpec));
			List<string> candidates = new List<string>();

			foreach (ProjectItem pi in BuildItems)
			{
				string include = pi.Include;

				if (File.Exists(include))
				{
					string path = Path.GetDirectoryName(include);

					if (!searchPaths.Contains(path))
						searchPaths.Add(path);

					if (!candidates.Contains(include))
						candidates.Add(include);
				}
			}

			if (candidates.Count > 0)
			{
				searchPaths.Clear();
				searchPaths.Insert(0, "{CandidateAssemblyFiles}");
				SetTaskParameter(ResolveReferencesTask, "CandidateAssemblyFiles", candidates.ToArray());
			}

			searchPaths.Add("{HintPathFromItem}");
			searchPaths.Add("{RawFileName}");

			SetTaskParameter(ResolveReferencesTask, "SearchPaths", searchPaths.ToArray());
			SetTaskParameter(ResolveReferencesTask, "AssemblyFiles", files);			
			SetTaskParameter(ResolveReferencesTask, "FindRelatedFiles", true);
			SetTaskParameter(ResolveReferencesTask, "FindSatellites", true);
			SetTaskParameter(ResolveReferencesTask, "FindSerializationAssemblies", true);

			SetTaskParameter(ResolveReferencesTask, "Silent", true);

			for (int i = 0; i < 2; i++)
			{
				bool local = (i == 0);

				SetTaskParameter(ResolveReferencesTask, "FindDependencies", !local);

				if (!ResolveReferencesTask.Execute())
					return;

				foreach (ITaskItem item in GetTaskParameter<ITaskItem[]>(ResolveReferencesTask, "ResolvedFiles"))
				{
					AddItem(local, TargetType.Item, item);

					if (local)
					{
						string fusionName = item.GetMetadata("FusionName");
						TargetAssembly = new AssemblyReference(fusionName, item, this);
					}
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

				if (local)
				{
					// Resources assemblies are not found by the resolver code if FindDependencies is false
					// So look through them 

					SortedFileList extraCopyItems = null;
					foreach (TargetItem target in ProjectOutput.Values)
					{
						if (target.Type == TargetType.Item && QQnPath.IsAssemblyFile(target.Target))
						{
							Assembly asm = null;
							try
							{
								asm = Assembly.ReflectionOnlyLoadFrom(Path.Combine(ProjectPath, target.Target));

								if (asm == null)
									continue;
							}
							catch (IOException)
							{ } // Can't load file
							catch(SystemException)
							{ } // Not an assembly

							if(asm != null)
								foreach (Module module in asm.GetModules(true))
								{
									string file = module.FullyQualifiedName;

									if (!ProjectOutput.ContainsKey(file))
									{
										if (extraCopyItems == null)
										{
											extraCopyItems = new SortedFileList();
											extraCopyItems.BaseDirectory = ProjectOutput.BaseDirectory;
										}

										extraCopyItems.AddUnique(file); // Don't edit projectoutput while walking through it
									}
								}
						}						
					}
					if(extraCopyItems != null)
					{
						foreach (string file in extraCopyItems)
						{
							// Add resources as Item instead of Copy: Dependencies will see it as SharedItems
							ProjectOutput.Add(new TargetItem(file, file, TargetType.Item));
						}
					}
				}
			}
		}

		private void AddItem(bool local, TargetType type, ITaskItem item)
		{
			string path = item.ItemSpec;

			string src = EnsureRelativePath(path);

			if (!ProjectOutput.ContainsKey(src))
			{
				ProjectOutput.Add(new TargetItem(src, src, local ? type : (TargetType)((int)type | 0x10)));
			}
		}
	}
}
