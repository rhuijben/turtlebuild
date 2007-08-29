using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics.CodeAnalysis;

namespace QQn.TurtleBuildUtils
{
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct SYMSRV_INDEX_INFO
	{
		const int MAX_PATH = 260;

		[MarshalAs(UnmanagedType.U4)]
		public Int32 sizeofstruct;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH + 1)]
		public string file;

		[MarshalAs(UnmanagedType.Bool)]
		public bool stripped;

		[MarshalAs(UnmanagedType.U4)]
		public int timestamp;

		[MarshalAs(UnmanagedType.U4)]
		public int size;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH + 1)]
		public string dbgfile;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH + 1)]
		public string pdbfile;

		[MarshalAs(UnmanagedType.Struct)]
		public Guid guid;

		[MarshalAs(UnmanagedType.U4)]
		public int sig;

		[MarshalAs(UnmanagedType.U4)]
		public int age;
	}

	class ResourceUpdateHandle : SafeHandle
	{
		public ResourceUpdateHandle()
			: base(IntPtr.Zero, true)
		{
		}

		protected override bool ReleaseHandle()
		{
			if (handle != IntPtr.Zero)
				NativeMethods.EndUpdateResource(handle, true);

			return true;
		}

		public override bool IsInvalid
		{
			get { return handle != IntPtr.Zero; }
		}

		public bool Commit()
		{
			IntPtr h = handle;
			if (h != IntPtr.Zero)
			{
				handle = h;
				return NativeMethods.EndUpdateResource(h, false);
			}

			return false;
		}
	}

	static class NativeMethods
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern ResourceUpdateHandle BeginUpdateResource(string pFileName, [MarshalAs(UnmanagedType.Bool)]bool bDeleteExistingResources);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UpdateResource(ResourceUpdateHandle hUpdate, IntPtr type, IntPtr name, ushort wLanguage, byte[] lpData, int cbData);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EndUpdateResource(IntPtr hUpdate, [MarshalAs(UnmanagedType.Bool)] bool fDiscard);		

		[DllImport("dbghelp.dll", SetLastError = false, CharSet = CharSet.Unicode)] // SetLastError=true, but if we set it to false we don't have to catch exceptions
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymSrvGetFileIndexInfo(string file, ref SYMSRV_INDEX_INFO info, [MarshalAs(UnmanagedType.U4)] int flags);

		[SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")] // We need the raw data instead of just the information
		[DllImport("version.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetFileVersionInfo(string pFilename, [MarshalAs(UnmanagedType.I4)]int handle, [MarshalAs(UnmanagedType.I4)]int len, [Out] byte[] data);

		[SuppressMessage("Microsoft.Usage", "CA2205:UseManagedEquivalentsOfWin32Api")] // We need the raw data instead of just the information
		[DllImport("version.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.U4)]
		public static extern int GetFileVersionInfoSize(string pFilename, [MarshalAs(UnmanagedType.U4)] out int handle);
	}
}
