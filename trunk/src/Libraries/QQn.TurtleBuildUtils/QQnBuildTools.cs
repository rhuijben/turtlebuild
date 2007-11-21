using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using QQn.TurtleUtils.IO;

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// 
	/// </summary>
	public static class QQnBuildTools
	{
		/// <summary>
		/// Gets the framework path which best matches the specified version
		/// </summary>
		/// <param name="version">The version.</param>
		/// <returns></returns>
		public static DirectoryInfo GetFrameworkDirectory(Version version)
		{
			if (version == null)
				throw new ArgumentNullException("version");

			string runtimeDir = QQnPath.NormalizePath(RuntimeEnvironment.GetRuntimeDirectory());
			string frameworkDir = Path.GetDirectoryName(runtimeDir);

			DirectoryInfo dir = new DirectoryInfo(frameworkDir);
			if (!dir.Exists)
				return null;

			DirectoryInfo[] dirs = dir.GetDirectories("v*.*", SearchOption.TopDirectoryOnly);

			int start = 2;
			if(version.Build >= 0)
				start = 4;
			else if(version.Revision >= 0)
				start = 3;

			for (int i = start; i >= 2; i--)
			{
				string name = "v" + version.ToString(i);

				foreach (DirectoryInfo d in dirs)
				{
					if (string.Equals(d.Name, name, StringComparison.InvariantCultureIgnoreCase))
						return d;
				}

				name += ".";

				foreach (DirectoryInfo d in dirs)
				{
					if (d.Name.StartsWith(name, StringComparison.InvariantCultureIgnoreCase))
						return d;
				}
			}

			return null;
		}

		/// <summary>
		/// Gets the solution version.
		/// </summary>
		/// <param name="solution">The solution.</param>
		/// <returns></returns>
		public static Version GetSolutionVersion(string solution)
		{
			if (string.IsNullOrEmpty("solution"))
				throw new ArgumentNullException("solution");

			using (StreamReader sr = File.OpenText(solution))
			{
				string line = sr.ReadLine();
				
				// First line should be empty
				if ((line == null) || line.Trim().Length != 0)
					return null;

				line = sr.ReadLine();
				string start = "Microsoft Visual Studio Solution File, Format Version ";

				if (line.StartsWith(start, StringComparison.InvariantCultureIgnoreCase))
				{
					return new Version(line.Substring(start.Length).Trim());
				}
				else
					return null;
			}
		}
	}
}
