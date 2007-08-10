using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using QQn.TurtleUtils.Tokenizer;

namespace QQn.TurtleMSBuild
{
	public class VCBuildHandler
	{
		internal static void HandleProject(BuildProject project)
		{
#if DEBUG
			if(!".sln".Equals(Path.GetExtension(project.ProjectFile), StringComparison.InvariantCultureIgnoreCase))
				return;
			
			SortedList<string,string> VCProjects = new SortedList<string,string>(StringComparer.InvariantCultureIgnoreCase);

			using(StreamReader sr = File.OpenText(project.ProjectFile))
			{
				string line;

				while(null != (line = sr.ReadLine()))
				{
					if(line.StartsWith("Project("))
					{
						IList<string> words = Tokenizer.GetCommandlineWords(line);

						GC.KeepAlive(words);
					}
				}
			}


			foreach (ProjectItem pi in project.BuildItems)
			{
				Console.WriteLine("{0}: {1}", pi.Name, pi.Include);

			}

			

			Console.WriteLine("======================");
			foreach (KeyValuePair<String, String> i in project.BuildProperties)
			{
				Console.WriteLine("{0}: {1}", i.Key, i.Value);
			}

		}
#endif
	}
}
