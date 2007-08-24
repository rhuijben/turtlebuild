using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Win32;

namespace QQn.TurtleBuildUtils
{
	static class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool UpdateResource(IntPtr hUpdate, IntPtr type, IntPtr name, ushort wLanguage, byte[] lpData, int cbData);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool EndUpdateResource(IntPtr hUpdate, bool fDiscard);

		[DllImport("version.dll", SetLastError = true)]
		public static extern bool GetFileVersionInfo(string pFilename, [MarshalAs(UnmanagedType.I4)]int handle, [MarshalAs(UnmanagedType.I4)]int len, [Out] byte[] data);

		[DllImport("version.dll", SetLastError = true)]
		public static extern int GetFileVersionInfoSize(string pFilename, out int handle);
	}

	/// <summary>
	/// Helper functions for updating file versions
	/// </summary>
	public static class VersionInfo
	{
		/// <summary>
		/// Updates the version info in the file header from the attributes defined on the assembly.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="keyFile">The key file.</param>
		/// <param name="keyContainer">The key container.</param>
		/// <returns></returns>
		public static bool RefreshVersionInfoFromAttributes(string file, string keyFile, string keyContainer)
		{
			if (!File.Exists(file))
				throw new FileNotFoundException("File to update not found", file);			

			string tmpDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tmpDir);
			try
			{
				string tmpAssembly = GenerateAttributeAssembly(file, tmpDir);

				if (tmpAssembly == null)
					return false;

				return CopyFileVersionInfo(tmpAssembly, file, keyFile, keyContainer);
			}
			finally
			{
				if (Directory.Exists(tmpDir))
					Directory.Delete(tmpDir, true);
			}
		}

		/// <summary>
		/// Copies the file version info header from one file to an other
		/// </summary>
		/// <param name="fromFile">Source file.</param>
		/// <param name="toFile">Destination file.</param>
		/// <param name="keyFile">The key file.</param>
		/// <param name="keyContainer">The key container.</param>
		/// <returns></returns>
		public static bool CopyFileVersionInfo(string fromFile, string toFile, string keyFile, string keyContainer)
		{
			if (string.IsNullOrEmpty(fromFile))
				throw new ArgumentNullException("fromFile");
			else if (string.IsNullOrEmpty(toFile))
				throw new ArgumentNullException("toFile");
			else if (!File.Exists(fromFile))
				throw new FileNotFoundException("File not found", fromFile);
			else if (!File.Exists(toFile))
				throw new FileNotFoundException("File not found", toFile);

			bool signFile = !string.IsNullOrEmpty(keyFile) || !string.IsNullOrEmpty(keyContainer);

			int handle;
			int size = NativeMethods.GetFileVersionInfoSize(fromFile, out handle);

			if (size <= 0)
				return false;

			byte[] versionInfo = new byte[size];

			if (NativeMethods.GetFileVersionInfo(fromFile, handle, size, versionInfo))
			{
				int originalSize = NativeMethods.GetFileVersionInfoSize(toFile, out handle);
				if (originalSize == size)
				{
					byte[] versionInfo2 = new byte[size];

					if (NativeMethods.GetFileVersionInfo(toFile, handle, size, versionInfo2))
					{
						bool changed = false;
						for (int i = 0; i < size; i++)
						{
							if (versionInfo[i] != versionInfo2[i])
							{
								changed = true;
								break;
							}
						}

						if (!changed)
							return true; // No need to update resource
					}
				}

				IntPtr resHandle = NativeMethods.BeginUpdateResource(toFile, false);

				if (resHandle != IntPtr.Zero)
				{
					bool ok = NativeMethods.UpdateResource(resHandle, (IntPtr)16, (IntPtr)1, 0, versionInfo, size);
					ok = NativeMethods.EndUpdateResource(resHandle, false) && ok;

					if(ok && signFile)
						ok = BuildTools.ReSignAssemblyWithFileOrContainer(toFile, keyFile, keyContainer);

					return ok;
				}
				else
					return false;
			}

			return false;
		}

		/// <summary>
		/// Generates an attribute assembly from the specified file in the given directory.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="outputDirectory">The output directory.</param>
		/// <returns></returns>
		private static string GenerateAttributeAssembly(string file, string outputDirectory)
		{
			AssemblyName srcName = AssemblyName.GetAssemblyName(file);

			if (srcName == null || string.IsNullOrEmpty(srcName.Name))
				return null;

			// Prepare dynamic assembly for resources
			AssemblyName asmName = new AssemblyName(srcName.FullName);
			asmName.Name = "Tmp." + srcName.Name;

			AssemblyBuilder newAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndSave, outputDirectory);

			string extension = Path.GetExtension(file);
			string tmpFile = Path.GetFileNameWithoutExtension(file) + ".resTmp" + extension;
			newAssembly.DefineDynamicModule(asmName.Name, tmpFile);

			// Load source assembly for reflection
			Assembly srcAssembly = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(file));

			foreach (CustomAttributeData attr in CustomAttributeData.GetCustomAttributes(srcAssembly))
			{
				if ((attr.NamedArguments.Count > 0) || (attr.Constructor == null))
				{
					// We don't use named arguments at this time; not needed for the version resources
					continue;
				}

				Type type = attr.Constructor.ReflectedType;

				if (type.Assembly != typeof(AssemblyVersionAttribute).Assembly)
				{
					continue;
				}

				List<object> values = new List<object>();
				object value = null;
				foreach (CustomAttributeTypedArgument arg in attr.ConstructorArguments)
				{
					if (value == null)
						value = arg.Value;
					values.Add(arg.Value);
				}

				CustomAttributeBuilder cb = new CustomAttributeBuilder(attr.Constructor, values.ToArray());

				newAssembly.SetCustomAttribute(cb);
			}

			newAssembly.DefineVersionInfoResource();
			newAssembly.Save(tmpFile);

			return Path.Combine(outputDirectory, tmpFile);
		}
	}
}
