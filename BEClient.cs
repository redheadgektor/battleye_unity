using System;
using System.Runtime.InteropServices;

namespace BattlEye
{
	public class BEClient
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string path);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

		[DllImport("kernel32.dll")]
		public static extern int FreeLibrary(IntPtr hModule);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int BEClientGetVerFn();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BEClientInitFn(int iIntegrationVersion, [MarshalAs(UnmanagedType.LPStruct)][In] BECL_GAME_DATA pGameData, [MarshalAs(UnmanagedType.LPStruct)][Out] BECL_BE_DATA pBEData);

		[StructLayout(LayoutKind.Sequential)]
		public class BECL_GAME_DATA
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string pstrGameVersion;

			public uint ulAddress;
			public ushort usPort;
			public PrintMessageFn pfnPrintMessage;
			public RequestRestartFn pfnRequestRestart;
			public SendPacketFn pfnSendPacket;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void PrintMessageFn([MarshalAs(UnmanagedType.LPStr)] string pstrMessage);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void RequestRestartFn(int iReason);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void SendPacketFn(IntPtr pvPacket, int nLength);
		}

		[StructLayout(LayoutKind.Sequential)]
		public class BECL_BE_DATA
		{
			public ExitFn pfnExit;
			public RunFn pfnRun;
			public CommandFn pfnCommand;
			public ReceivedPacketFn pfnReceivedPacket;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate bool ExitFn();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void RunFn();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void CommandFn([MarshalAs(UnmanagedType.LPStr)] string pstrCommand);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void ReceivedPacketFn(IntPtr pvPacket, int nLength);
		}
	}
}
