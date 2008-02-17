using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.Win32;
using System.Globalization;
using System.Diagnostics;
using QQn.TurtleUtils.IO;
using System.Security.Policy;
using System.Runtime.Remoting;

namespace QQn.TurtleBuildUtils
{
	sealed class ReflectionOnlyLoader : MarshalByRefObject
	{
		public Assembly ReflectionOnlyLoad(string filename)
		{
			if (string.IsNullOrEmpty(filename))
				throw new ArgumentNullException("filename");
			else if (!File.Exists(filename))
				throw new ArgumentException();

			return Assembly.ReflectionOnlyLoad(File.ReadAllBytes(filename));
		}
	}

	/// <summary>
	/// Helper functions for updating file versions
	/// </summary>
	public static class AssemblyUtils
	{
		/// <summary>
		/// Updates the version info in the file header from the attributes defined on the assembly.
		/// </summary>
		/// <param name="assemblyFile">The assembly.</param>
		/// <param name="keyFile">The key file.</param>
		/// <param name="keyContainer">The key container.</param>
		/// <returns></returns>
		public static bool RefreshVersionInfoFromAttributes(string assemblyFile, string keyFile, string keyContainer)
		{
			if (string.IsNullOrEmpty(assemblyFile))
				throw new ArgumentNullException("assemblyFile");

			if (!File.Exists(assemblyFile))
				throw new FileNotFoundException("File to update not found", assemblyFile);
			else if (!string.IsNullOrEmpty(keyFile) && !File.Exists(keyFile))
				throw new FileNotFoundException("Keyfile not found", keyFile);

			string tmpDir = QQnPath.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tmpDir);
			try
			{
				string tmpName = QQnPath.Combine(tmpDir, Path.GetFileNameWithoutExtension(assemblyFile) + ".resTmp.dll");

				Assembly myAssembly = typeof(AssemblyUtils).Assembly;
				AppDomainSetup setup = new AppDomainSetup();
				setup.ApplicationName = "TB-AttributeRefresher";
				setup.ApplicationBase = Path.GetDirectoryName(new Uri(myAssembly.CodeBase).LocalPath);
				setup.AppDomainInitializer = new AppDomainInitializer(OnRefreshVersionInfo);			
				setup.AppDomainInitializerArguments = new string[] { assemblyFile, tmpDir, tmpName};
			
				GC.KeepAlive(AppDomain.CreateDomain("AttributeRefresher", myAssembly.Evidence, setup)); // The appdomain will auto destruct

				if(!File.Exists(tmpName))
					return false;

				return CopyFileVersionInfo(tmpName, assemblyFile, keyFile, keyContainer);
			}
			finally
			{
				if (Directory.Exists(tmpDir))
					Directory.Delete(tmpDir, true);
			}
		}

		// Called in it's own appdomain
		static void OnRefreshVersionInfo(string[] args)
		{
			string fromFile = args[0];
			string toDir = args[1];
			string toFile = args[2];

			Assembly asm = Assembly.ReflectionOnlyLoad(File.ReadAllBytes(fromFile));

			string result = GenerateAttributeAssembly(asm, toDir);

			Debug.Assert(string.Equals(result, toFile, StringComparison.OrdinalIgnoreCase));
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

				using (ResourceUpdateHandle resHandle = NativeMethods.BeginUpdateResource(toFile, false))
				{
					if (resHandle != null)
					{
						bool ok = NativeMethods.UpdateResource(resHandle, (IntPtr)16, (IntPtr)1, 0, versionInfo, size);
						ok = resHandle.Commit() && ok;

						if (ok && signFile)
							ok = BuildTools.ResignAssemblyWithFileOrContainer(toFile, keyFile, keyContainer);

						return ok;
					}
					else
						return false;
				}
			}

			return false;
		}

		static void InitLoader(string[] args)
		{
			foreach (string file in args)
			{
				GC.KeepAlive(Assembly.ReflectionOnlyLoad(File.ReadAllBytes(file)));
			}
		}

		/// <summary>
		/// Generates an attribute assembly from the specified file in the given directory.
		/// </summary>
		/// <param name="file">The file.</param>
		/// <param name="outputDirectory">The output directory.</param>
		/// <returns></returns>
		private static string GenerateAttributeAssembly(string file, string outputDirectory)
		{
			if (string.IsNullOrEmpty(file))
				throw new ArgumentNullException("file");
			else if (string.IsNullOrEmpty(outputDirectory))
				throw new ArgumentNullException("outputDirectory");
			else if (!File.Exists(file))
				throw new FileNotFoundException("Source Assembly", file);

			Evidence ed = typeof(AssemblyUtils).Assembly.Evidence;


			AppDomainSetup domainSetup = new AppDomainSetup();
			domainSetup.AppDomainInitializer = new AppDomainInitializer(InitLoader);
			domainSetup.AppDomainInitializerArguments = new string[] { file };
			Assembly myAssembly = typeof(ReflectionOnlyLoader).Assembly;
			domainSetup.ApplicationBase = Path.GetDirectoryName(new Uri(myAssembly.CodeBase).LocalPath);
			AppDomain domain = AppDomain.CreateDomain("AssemblyRefresher", typeof(AssemblyUtils).Assembly.Evidence, domainSetup);

			// Ok, we have an appdomain with the required assembly loaded

			Assembly srcAssembly = null;
			AssemblyName name = AssemblyName.GetAssemblyName(file);

			foreach (Assembly asm in domain.ReflectionOnlyGetAssemblies())
			{
				if (asm.FullName == name.FullName)
				{
					srcAssembly = asm;
					break;
				}
			}

			return GenerateAttributeAssembly(srcAssembly, outputDirectory);
		}

		static Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			//throw new NotImplementedException();
			return null;
		}

		static void domain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
		{
			//throw new NotImplementedException();
		}

		static SortedFileList<Assembly> _reflectionAssemblies = new SortedFileList<Assembly>();
		/// <summary>
		/// Gets the reflection assembly; cache te reference for future requests
		/// </summary>
		/// <param name="assemblyPath">The assembly path.</param>
		/// <returns></returns>
		public static Assembly GetCachedReflectionAssembly(string assemblyPath)
		{
			if (string.IsNullOrEmpty(assemblyPath))
				throw new ArgumentNullException("assemblyPath");

			lock (_reflectionAssemblies)
			{
				Assembly r;

				if (_reflectionAssemblies.TryGetValue(assemblyPath, out r))
					return r;


				r = Assembly.ReflectionOnlyLoadFrom(assemblyPath);
				_reflectionAssemblies.Add(assemblyPath, r);

				return r;
			}
		}


		private static string GenerateAttributeAssembly(Assembly assembly, string outputDirectory)
		{
			AssemblyName srcName = new AssemblyName(assembly.FullName);

			if (srcName == null || string.IsNullOrEmpty(srcName.Name))
				return null;

			try
			{
				// Prepare dynamic assembly for resources
				AssemblyName asmName = new AssemblyName(srcName.FullName);
				asmName.Name = "Tmp." + srcName.Name;

				// Only create an on-disk assembly. We never have to execute anything
				AssemblyBuilder newAssembly = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.ReflectionOnly, outputDirectory);

				string tmpFile = srcName.Name + ".resTmp.dll";
				newAssembly.DefineDynamicModule(asmName.Name, tmpFile);

				AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(OnReflectionOnlyAssemblyResolve);

				try
				{
					Assembly mscorlib = Assembly.ReflectionOnlyLoad(typeof(int).Assembly.FullName);
					Assembly system = Assembly.ReflectionOnlyLoad(typeof(Uri).Assembly.FullName);

					foreach (CustomAttributeData attr in CustomAttributeData.GetCustomAttributes(assembly))
					{
						if ((attr.NamedArguments.Count > 0) || (attr.Constructor == null))
						{
							// We don't use named arguments at this time; not needed for the version resources
							continue;
						}

						Type type = attr.Constructor.ReflectedType;

						if (type.Assembly != typeof(AssemblyVersionAttribute).Assembly && type.Assembly != mscorlib && type.Assembly != system)
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
				}
				finally
				{
					AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve -= new ResolveEventHandler(OnReflectionOnlyAssemblyResolve);
				}

				newAssembly.DefineVersionInfoResource();
				newAssembly.Save(tmpFile);

				return QQnPath.Combine(outputDirectory, tmpFile);
			}
			catch (FileLoadException)
			{
				return null;
			}
			catch (IOException)
			{
				return null;
			}
		}

		/// <summary>
		/// Called when [reflection only assembly resolve].
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="args">The <see cref="System.ResolveEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		static Assembly OnReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
		{
			return Assembly.ReflectionOnlyLoad(args.Name);
		}

		static bool _enfDebugReference;

		/// <summary>
		/// Gets the debug information id. This guid+age pair is used by symbolserver to provide pdb's of released code
		/// </summary>
		/// <param name="path">The path of a library, executable or pdb file</param>
		/// <returns>A string containing a guid as a 32 character uppercase hexadecimal string and an 
		/// age (lowercase hexadecimal) suffix without separation in between</returns>
		/// <exception cref="FileNotFoundException">Path not found</exception>
		public static DebugReference GetDebugReference(string path)
		{
			if (string.IsNullOrEmpty(path))
				throw new ArgumentNullException("path");
			else if (!File.Exists(path))
				throw new FileNotFoundException("Reference file not found", path);

			if (_enfDebugReference)
				return null;

			try
			{
				SYMSRV_INDEX_INFO info = new SYMSRV_INDEX_INFO();
				info.sizeofstruct = Marshal.SizeOf(info);

				if (NativeMethods.SymSrvGetFileIndexInfo(path, ref info, 0))
				{
					if (!info.stripped)
					{
						return new DebugReference(
							string.IsNullOrEmpty(info.pdbfile) ? path : info.pdbfile, info.guid, info.age);
					}
				}

				return null;
			}
			catch (EntryPointNotFoundException)
			{
				_enfDebugReference = true;
				return null;
			}
		}
	}
}
