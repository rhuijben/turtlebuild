using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;

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
							string myVersion = ("sdkInstallRoot" + typeof(BuildTools).Assembly.ImageRuntimeVersion).ToLower();

							foreach (string vn in rk.GetValueNames())
							{
								string name = vn.ToLower();
								if (name.StartsWith("sdkinstallroot"))
								{
									if (fallback == null)
									{
										string value = (string)rk.GetValue(name);

										if (Directory.Exists(value))
										{
											fallback = name;
										}
									}

									if (myVersion.StartsWith(name))
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
		public static string SnExePath
		{
			get
			{
				if (_snExe == null)
				{
					string sn = Path.Combine(FrameworkSdkDir, "bin\\sn.exe");

					if (File.Exists(sn))
						_snExe = sn;
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
		public static bool ReSignAssemblyWithFile(string assembly, string strongNameFile)
		{
			if (string.IsNullOrEmpty(assembly))
				throw new ArgumentNullException("assembly");
			else if(string.IsNullOrEmpty(strongNameFile))
				throw new ArgumentNullException("strongNameFile");
			else if (!File.Exists(assembly))
				throw new FileNotFoundException("Assembly not found", assembly);
			else if(!File.Exists(strongNameFile))
				throw new FileNotFoundException(string.Format("StrongNameFile not found: {0}", strongNameFile), strongNameFile);

			ProcessStartInfo psi = new ProcessStartInfo(SnExePath, string.Format("-Ra \"{0}\" \"{1}\"", assembly, strongNameFile));
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
		public static bool ReSignAssemblyWithContainer(string assembly, string container)
		{
			if (string.IsNullOrEmpty(assembly))
				throw new ArgumentNullException("assembly");
			else if (string.IsNullOrEmpty(container))
				throw new ArgumentNullException("container");
			else if (!File.Exists(assembly))
				throw new FileNotFoundException("Assembly not found", assembly);

			ProcessStartInfo psi = new ProcessStartInfo(SnExePath, string.Format("-Rca \"{0}\" \"{1}\"", assembly, container));
			psi.UseShellExecute = false;
			psi.WindowStyle = ProcessWindowStyle.Hidden;
			psi.CreateNoWindow = true;
			using (Process p = Process.Start(psi))
			{
				p.WaitForExit();

				return p.ExitCode == 0;
			}
		}

		internal static bool ReSignAssemblyWithFileOrContainer(string assembly, string keyFile, string keyContainer)
		{
			if (!string.IsNullOrEmpty(keyFile))
				return ReSignAssemblyWithFile(assembly, keyFile);
			else if (!string.IsNullOrEmpty(keyContainer))
				return ReSignAssemblyWithContainer(assembly, keyContainer);
			else
				throw new ArgumentException("keyFile or keyContainer must be non-null");			
		}
	}
}
