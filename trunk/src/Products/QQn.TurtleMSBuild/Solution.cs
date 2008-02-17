using System;
using System.Collections;
using System.Collections.Generic;
using QQn.TurtleMSBuild.ExternalProjects;
using QQn.TurtleBuildUtils;
using System.IO;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleUtils.IO;
using System.Xml;


namespace QQn.TurtleMSBuild
{
	class Solution : MSBuildProject
	{
		bool _usedVcBuild;
		SortedFileList<ExternalProject> _externalProjects;

		public Solution(string projectFile, string targetNames, IEnumerable properties, IEnumerable items, TurtleParameters parameters)
			: base(projectFile, targetNames, properties, items, parameters)
		{
			ProjectType = "Solution";
		}

		internal bool UsedVCBuild
		{
			get { return _usedVcBuild; }
			set { _usedVcBuild = value; }
		}

		public override bool IsSolution
		{
			get { return true; }
		}

		public override void ParseBuildResult(Project parentProject)
		{
			Refresh();

			if (UsedVCBuild)
				ExternalBuildHandler.HandleProject(this);
		}

		public SortedFileList<ExternalProject> ExternalProjects
		{
			get
			{
				if (_externalProjects == null)
					_externalProjects = CreateExternalProjectsList();

				return _externalProjects;
			}
		}

		static string FilterWord(string word)
		{
			return word.TrimEnd(',').Trim('\"');
		}

		static readonly Guid solutionItem = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
		private SortedFileList<ExternalProject> CreateExternalProjectsList()
		{
			SortedFileList<ExternalProject> externalProjects = new SortedFileList<ExternalProject>();

			using (StreamReader sr = File.OpenText(ProjectFile))
			{
				string line;

				while (null != (line = sr.ReadLine()))
				{
					if (line.StartsWith("Project("))
					{
						IList<string> words = Tokenizer.GetCommandlineWords(line);

						if (words.Count < 5 || words[1] != "=")
							continue;

						Guid projectType = new Guid(words[0].Substring(8).TrimEnd(')').Trim('\"'));
						string projectName = FilterWord(words[2]);
						string projectFile = QQnPath.Combine(ProjectPath, FilterWord(words[3]));
						Guid projectGuid = new Guid(FilterWord(words[4]));

						if (projectType != solutionItem && File.Exists(projectFile))
						{
							if (QQnPath.ExtensionEquals(projectFile, ".vcproj"))
								externalProjects.Add(projectFile, new VCBuildProject(projectGuid, projectFile, projectName, Parameters));
						}
					}
				}
			}

			if (BuildProperties == null)
				Refresh();

			// The property CurrentSolutionConfigurationContents contains the 'real' configuration of external projects
			string configData;
			if (BuildProperties.TryGetValue("CurrentSolutionConfigurationContents", out configData))
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(configData);

				foreach (ExternalProject ep in externalProjects)
				{
					XmlNode node = doc.SelectSingleNode("//ProjectConfiguration[@Project='" + ep.ProjectGuid.ToString("B").ToUpperInvariant() + "']");

					if (node != null)
					{
						ep.AddBuildConfiguration(node.InnerText);
					}
				}
			}

			return externalProjects;
		}
	}
}