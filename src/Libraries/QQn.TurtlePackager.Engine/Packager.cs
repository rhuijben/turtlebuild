using System;
using System.Collections.Generic;
using System.Text;
using QQn.TurtlePackage;
using QQn.TurtleBuildUtils;
using QQn.TurtleBuildUtils.Files.TBLog;
using System.IO;
using QQn.TurtleUtils.IO;

namespace QQn.TurtlePackager
{
	public class PackageList : SortedFileList<TPack>
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

			state.LoadExternalOrigins();
			state.CreateBuildOrigins();
			state.AddRequirements();

			state.CalculateDependencies();

            newPackages = new PackageList();

            List<TBLogFile> filesToRun = new List<TBLogFile>(state.Logs);

            while(filesToRun.Count > 0)
            {
                int n = 0;
                for (int i = 0; i < filesToRun.Count; i++)
                {
                    TBLogFile file = filesToRun[i];
                    if (state.CanPackage(file))
                    {
                        filesToRun.RemoveAt(i--);

                        string target = QQnPath.Combine(args.OutputDir, file.Project.Name + ".tpZip");
                        FileInfo targetInfo = new FileInfo(target);

                        TPack pack;
                        if (targetInfo.Exists && targetInfo.LastWriteTime > file.GetLastWriteTime())
                        {
                            pack = TPack.OpenFrom(target, VerificationMode.None);
                            state.SetOriginPack(file, pack.Pack);
                        }
                        else
                        {
                            pack = TPack.Create(target, state.CreateDefinition(file));
                        }
                        newPackages.Add(target, pack);
                        n++;
                    }
                }

                if (n == 0)
                    break; // Can't package anything
            }
			
			return true;
		}
	}
}
