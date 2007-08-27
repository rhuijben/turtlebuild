using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace QQn.TurtleBuildUtils
{
	[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
	struct SYMSRV_INDEX_INFO
	{
		const int MAX_PATH = 260;

		[MarshalAs(UnmanagedType.U4)]
		public Int32 sizeofstruct;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_PATH+1)]
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

		[DllImport("dbghelp.dll", SetLastError = false, CharSet = CharSet.Unicode)] // SetLastError=true, but if we set it to false we don't have to catch exceptions
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SymSrvGetFileIndexInfo(string file, ref SYMSRV_INDEX_INFO info, [MarshalAs(UnmanagedType.U4)] int flags);
	}
}
