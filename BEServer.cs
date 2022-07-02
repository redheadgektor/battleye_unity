using System;
using System.Runtime.InteropServices;

namespace BattlEye
{
    public class BEServer
	{
		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibraryW([MarshalAs(UnmanagedType.LPWStr)] string path);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

		[DllImport("kernel32.dll")]
		public static extern int FreeLibrary(IntPtr hModule);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate int BEServerGetVerFn();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate bool BEServerInitFn(int iIntegrationVersion, [MarshalAs(UnmanagedType.LPStruct)][In] BESV_GAME_DATA pGameData, [MarshalAs(UnmanagedType.LPStruct)][Out] BESV_BE_DATA pBEData);

		[StructLayout(LayoutKind.Sequential)]
		public class BESV_GAME_DATA
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string pstrGameVersion;
			public PrintMessageFn pfnPrintMessage;
			public KickPlayerFn pfnKickPlayer;
			public SendPacketFn pfnSendPacket;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void PrintMessageFn([MarshalAs(UnmanagedType.LPStr)] string pstrMessage);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void KickPlayerFn(int iPID, [MarshalAs(UnmanagedType.LPStr)] string pstrReason);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void SendPacketFn(int iPID, IntPtr pvPacket, int nLength);
		}

		[StructLayout(LayoutKind.Sequential)]
		public class BESV_BE_DATA
		{
			public ExitFn pfnExit;
			public RunFn pfnRun;
			public CommandFn pfnCommand;
			public AddPlayerFn pfnAddPlayer;
			public ChangePlayerStatusFn pfnChangePlayerStatus;
			public ReceivedPlayerGUIDFn pfnReceivedPlayerGUID;
			public PlayerGUIDIsValidFn pfnPlayerGUIDIsValid;
			public ReceivedPacketFn pfnReceivedPacket;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate bool ExitFn();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void RunFn();

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void CommandFn(string pstrCommand);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void AddPlayerFn(int iPID, uint ulAddress, ushort usPort, [MarshalAs(UnmanagedType.LPStr)] string pstrName);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void ChangePlayerStatusFn(int iPID, int iStatus);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void ReceivedPlayerGUIDFn(int iPID, IntPtr pvGUID, int nGUIDLength);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void PlayerGUIDIsValidFn(int iPID, IntPtr pvOwnerGUID, int nGUIDLength);

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void ReceivedPacketFn(int iPID, IntPtr pvPacket, int nLength);
		}
	}
}
