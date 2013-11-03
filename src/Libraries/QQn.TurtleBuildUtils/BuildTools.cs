using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Win32;
using QQn.TurtleUtils.IO;
using System.Runtime.InteropServices;

namespace QQn.TurtleBuildUtils
{
	/// <summary>
	/// 
	/// </summary>
	public static class BuildTools
	{

		static string _sdkPath;
		/// <summary>
		/// Gets the framework SDK dir.
		/// </summary>
		/// <value>The framework SDK dir.</value>
		public static string FrameworkSdkDir
		{
			get
			{
				if (_sdkPath == null)
				{
					using (RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\.NETFramework", false))
					{
						if (rk != null)
						{
							string primary = null;
							string fallback = null;
							string myVersion = ("sdkInstallRoot" + typeof(BuildTools).Assembly.ImageRuntimeVersion);

							foreach (string name in rk.GetValueNames())
							{
								if (name.StartsWith("sdkinstallroot", StringComparison.OrdinalIgnoreCase))
								{
									if (fallback == null)
									{
										string value = (string)rk.GetValue(name);

										if (Directory.Exists(value))
										{
											fallback = name;
										}
									}

									if (myVersion.StartsWith(name, StringComparison.OrdinalIgnoreCase))
									{
										string value = (string)rk.GetValue(name);

										if (Directory.Exists(value))
										{
											if ((primary == null) || (name.Length > primary.Length))
												primary = name;
										}
									}
								}
							}

							if (primary == null)
								primary = fallback;

							if (primary != null)
							{
								_sdkPath = (string)rk.GetValue(primary);
							}
						}
					}
				}
				return _sdkPath;
			}
		}

		static string _snExe;
		/// <summary>
		/// Gets the sn.exe path.
		/// </summary>
		/// <value>The sn exe path.</value>
		static string StrongNameToolPath
		{
			get
			{
				if (_snExe == null)
				{
					string sn = QQnPath.Combine(FrameworkSdkDir, "bin\\sn.exe");

					if (File.Exists(sn))
						_snExe = sn;
					else
						_snExe = QQnPath.FindFileInPath("sn.exe");

				}
				return _snExe;
			}
		}

		/// <summary>
		/// Res the sign assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="strongNameFile">The strong name file.</param>
		/// <returns></returns>
		public static bool ResignAssemblyWithFile(string assembly, string strongNameFile)
		{
			if (string.IsNullOrEmpty(assembly))
				throw new ArgumentNullException("assembly");
			else if(string.IsNullOrEmpty(strongNameFile))
				throw new ArgumentNullException("strongNameFile");
			else if (!File.Exists(assembly))
				throw new FileNotFoundException("Assembly not found", assembly);
			else if(!File.Exists(strongNameFile))
				throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, "StrongNameFile not found: {0}", strongNameFile), strongNameFile);

			ProcessStartInfo psi = new ProcessStartInfo(StrongNameToolPath, string.Format(CultureInfo.InvariantCulture, "-q -Ra \"{0}\" \"{1}\"", assembly, strongNameFile));
			psi.UseShellExecute = false;
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			using (Process p = Process.Start(psi))
			{
				p.WaitForExit();

				return p.ExitCode == 0;
			}
		}

		/// <summary>
		/// Res the sign assembly.
		/// </summary>
		/// <param name="assembly">The assembly.</param>
		/// <param name="container">The container.</param>
		/// <returns></returns>
		public static bool ResignAssemblyWithContainer(string assembly, string container)
		{
			if (string.IsNullOrEmpty(assembly))
				throw new ArgumentNullException("assembly");
			else if (string.IsNullOrEmpty(container))
				throw new ArgumentNullException("container");
			else if (!File.Exists(assembly))
				throw new FileNotFoundException("Assembly not found", assembly);

			ProcessStartInfo psi = new ProcessStartInfo(StrongNameToolPath, string.Format(CultureInfo.InvariantCulture, "-q -Rca \"{0}\" \"{1}\"", assembly, container));
			psi.UseShellExecute = false;
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			using (Process p = Process.Start(psi))
			{
				p.WaitForExit();

				return p.ExitCode == 0;
			}
		}

		internal static bool ResignAssemblyWithFileOrContainer(string assembly, string keyFile, string keyContainer)
		{
			if (!string.IsNullOrEmpty(keyFile))
				return ResignAssemblyWithFile(assembly, keyFile);
			else if (!string.IsNullOrEmpty(keyContainer))
				return ResignAssemblyWithContainer(assembly, keyContainer);
			else
				throw new ArgumentException("keyFile or keyContainer must be non-null");			
		}

        /// <summary>
        /// Gets the framework path which best matches the specified version
        /// </summary>
        /// <param name="frameworkVersion">The framework version.</param>
        /// <returns></returns>
        public static DirectoryInfo GetFrameworkDirectory(Version frameworkVersion)
        {
            if (frameworkVersion == null)
                throw new ArgumentNullException("frameworkVersion");

            string runtimeDir = QQnPath.NormalizePath(RuntimeEnvironment.GetRuntimeDirectory());
            string frameworkDir = Path.GetDirectoryName(runtimeDir);

            DirectoryInfo dir = new DirectoryInfo(frameworkDir);
            if (!dir.Exists)
                return null;

            if(frameworkVersion.Major == 4 && frameworkVersion.Minor == 5)
                frameworkVersion = new Version(4,0);

            DirectoryInfo[] dirs = dir.GetDirectories("v*.*", SearchOption.TopDirectoryOnly);

            int start = 2;
            if (frameworkVersion.Build >= 0)
                start = 4;
            else if (frameworkVersion.Revision >= 0)
                start = 3;

            for (int i = start; i >= 2; i--)
            {
                string name = "v" + frameworkVersion.ToString(i);

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
        /// Get the MSBuild tools directory for the specified tools version
        /// </summary>
        /// <param name="toolsVersion"></param>
        /// <returns></returns>
        public static DirectoryInfo GetBuildToolsDirectory(Version toolsVersion)
        {
            if (toolsVersion == null)
                throw new ArgumentNullException("toolsVersion");

            // Before Visual Studio 2013 the build tools were part of the .Net Framework
            if (toolsVersion.Major <= 4)
                return GetFrameworkDirectory(toolsVersion);

            using(RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\MSBuild\\ToolsVersions", false))
            {
                using(RegistryKey tool = rk.OpenSubKey(string.Format("{0}.{1}", toolsVersion.Major, toolsVersion.Minor), false))
                {
                    if (tool == null)
                        return null;

                    string path = tool.GetValue("MSBuildToolsPath") as string;

                    if (path != null)
                        return new DirectoryInfo(path);
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

            Version slnVersion;
            Version vsVersion;
            Version vsMinVersion;

            if (TryGetSolutionAndVisualStudioVersion(solution, out slnVersion, out vsVersion, out vsMinVersion))
                return slnVersion;

            return null;
        }

        const string _slnVersionStart = "Microsoft Visual Studio Solution File, Format Version ";
        const string _vsVersionStart = "VisualStudioVersion =";
        const string _minVersionStart = "MinimumVisualStudioVersion =";

        /// <summary>
        /// Gets the solution and visual studio from a solution file.
        /// </summary>
        /// <param name="solution">The solution file</param>
        /// <param name="slnVersion">The solution version</param>
        /// <param name="vsVersion">The visual studio version (or NULL for old solutions)</param>
        /// <param name="vsMinVersion"></param>
        /// <returns>true if obtained the requested information, otherwise false</returns>
        public static bool TryGetSolutionAndVisualStudioVersion(string solution, out Version slnVersion, out Version vsVersion, out Version vsMinVersion)
        {
            slnVersion = null;
            vsVersion = null;
            vsMinVersion = null;
            using (StreamReader sr = File.OpenText(solution))
            {
                // Match MSBuild behavior: Read 4 lines max. Add 1 lines for MinimumVisualStudioVersion
                for (int lineNr = 0; lineNr < 5; lineNr++)
                {
                    string line = sr.ReadLine();

                    if (line == null)
                        break;

                    line = line.Trim();
                    if (line.Length == 0)
                        continue;

                    if (line.StartsWith(_slnVersionStart, StringComparison.InvariantCultureIgnoreCase))
                    {
                        slnVersion = new Version(line.Substring(_slnVersionStart.Length).Trim());
                    }
                    else if (line.StartsWith(_vsVersionStart, StringComparison.OrdinalIgnoreCase))
                    {
                        vsVersion = new Version(line.Substring(_vsVersionStart.Length).Trim());
                    }
                    else if (line.StartsWith(_minVersionStart, StringComparison.OrdinalIgnoreCase))
                    {
                        vsMinVersion = new Version(line.Substring(_minVersionStart.Length).Trim());
                    }
                }
            }

            if (slnVersion == null)
                return false;
            if (slnVersion >= new Version(12, 0))
                return (vsVersion != null);

            return true;
        }

        /// <summary>
        /// Obtains the solution and current tools version
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="slnVersion"></param>
        /// <param name="vsVersion"></param>
        /// <returns></returns>
        public static bool TryGetSolutionAndVisualStudioVersion(string solution, out Version slnVersion, out Version vsVersion)
        {
            Version vsMinVersion;
            return TryGetSolutionAndVisualStudioVersion(solution, out slnVersion, out vsVersion, out vsMinVersion);
        }

        /// <summary>
        /// Obtains the solution version
        /// </summary>
        /// <param name="solution"></param>
        /// <param name="slnVersion"></param>
        /// <returns></returns>
        public static bool TryGetSolutionVersion(string solution, out Version slnVersion)
        {
            Version vsMinVersion;
            Version vsVersion;
            return TryGetSolutionAndVisualStudioVersion(solution, out slnVersion, out vsVersion, out vsMinVersion);
        }
	}
}
