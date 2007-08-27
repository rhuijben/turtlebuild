using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using QQn.TurtleUtils.Tokens;
using QQn.TurtleMSBuild.ExternalProjects;
using Microsoft.Build.Framework;
using System.Xml;

namespace QQn.TurtleMSBuild
{
	class VCBuildHandler
	{
		static string FilterWord(string word)
		{
			return word.TrimEnd(',').Trim('\"');
		}

		static readonly Guid solutionItem = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");

		internal static void HandleProject(Solution solution)
		{
			List<ExternalProject> externalProjects = LoadExternalProjects(solution);

			// The property CurrentSolutionConfigurationContents contains the 'real' configuration of external projects
			string configData;
			if (solution.BuildProperties.TryGetValue("CurrentSolutionConfigurationContents", out configData))
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(configData);

				foreach (ExternalProject ep in externalProjects)
				{
					XmlNode node = doc.SelectSingleNode("//ProjectConfiguration[@Project='" + ep.ProjectGuid.ToString("B").ToUpperInvariant() + "']");

					if (node != null)
					{
						ep.Configuration = node.InnerText;
					}
				}
			}

			foreach (ExternalProject ep in externalProjects)
			{
				string prefix = "Project_" + ep.ProjectGuid.ToString().ToUpperInvariant() + "_";

				foreach (ProjectItem pi in solution.BuildItems)
				{
					if (pi.Name.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
					{
						ep.BuildItems.Add(pi);
					}
				}
			}

			foreach (ExternalProject ep in externalProjects)
			{
				ep.ParseBuildResult(solution);
				ep.WriteTBLog();
			}
		}

		private static List<ExternalProject> LoadExternalProjects(MSBuildProject solution)
		{
			List<ExternalProject> externalProjects = new List<ExternalProject>();

			using (StreamReader sr = File.OpenText(solution.ProjectFile))
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
						string projectFile = Path.Combine(solution.ProjectPath, FilterWord(words[3]));
						Guid projectGuid = new Guid(FilterWord(words[4]));

						if (projectType != solutionItem && File.Exists(projectFile))
						{
							switch (Path.GetExtension(projectFile).ToUpperInvariant())
							{
								case ".VCPROJ":
									externalProjects.Add(new VCBuildProject(projectGuid, projectFile, projectName, solution.Parameters));
									break;
							}
						}
					}
				}
			}
			return externalProjects;
		}
	}
}
