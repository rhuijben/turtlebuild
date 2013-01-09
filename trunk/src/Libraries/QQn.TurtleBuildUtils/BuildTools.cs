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

            if(version.Major == 4 && version.Minor == 5)
                version = new Version(4,0);

            DirectoryInfo[] dirs = dir.GetDirectories("v*.*", SearchOption.TopDirectoryOnly);

            int start = 2;
            if (version.Build >= 0)
                start = 4;
            else if (version.Revision >= 0)
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
                while (line != null && line.Trim().Length == 0)
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
