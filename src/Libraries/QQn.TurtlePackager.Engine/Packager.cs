using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtlePackage;
using QQn.TurtleBuildUtils;
using QQn.TurtleBuildUtils.Files.TBLog;

namespace QQn.TurtlePackager
{
	public class PackageList : SortedFileList<Pack>
	{
	}


	public static class Packager
	{
		/// <summary>
		/// Tries to create packages from the specified logfiles; walking dependencies if needed.
		/// </summary>
		/// <param name="args">The args.</param>
		/// <param name="newPackages">The new packages.</param>
		/// <returns></returns>
		public static bool TryCreatePackages(PackageArgs args, out PackageList newPackages)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			PackageState state = new PackageState(args);

			state.CreateBuildOrigins();
			state.AddRequirements();

			state.CalculateDependencies();

			newPackages = new PackageList();
			
			return true;
		}
	}
}
