using BattlEye;
using ENet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public sealed partial class GameClient : ENetClient
{
	IntPtr battlEyeClientHandle = IntPtr.Zero;
	BEClient.BECL_GAME_DATA battlEyeClientInitData = null;
	BEClient.BECL_BE_DATA battlEyeClientRunData = null;
	private bool BattlEyeInitialized = false;

	private bool RequireInitializeBattlEye(ByteStream stream)
	{

		string text = Path.Combine(Environment.CurrentDirectory, "BEClient_x64.dll");
		if (!File.Exists(text))
		{
			text = Path.Combine(Environment.CurrentDirectory, "BEClient.dll");
		}
		if (!File.Exists(text))
		{
			Debug.LogError("Missing BattlEye client library! (" + text + ")");
			return false;
		}

		Debug.Log("Try loading BattlEye server library from: " + text);
		bool result = false;
		try
		{
			battlEyeClientHandle = BEClient.LoadLibraryW(text);
			if (battlEyeClientHandle == IntPtr.Zero)
			{
				Debug.LogError("Failed to load BattlEye client library!");
				result = false;
			}
			BEClient.BEClientInitFn beclientInitFn = Marshal.GetDelegateForFunctionPointer(BEClient.GetProcAddress(battlEyeClientHandle, "Init"), typeof(BEClient.BEClientInitFn)) as BEClient.BEClientInitFn;
			if (beclientInitFn == null)
			{
				BEClient.FreeLibrary(battlEyeClientHandle);
				battlEyeClientHandle = IntPtr.Zero;
				Debug.LogError("Failed to get BattlEye client init delegate!");
				result = false;
			}
			uint ulAddress = stream.Read<uint>();
			ushort usPort = stream.Read<ushort>();
			battlEyeClientInitData = new BEClient.BECL_GAME_DATA();
			battlEyeClientInitData.pstrGameVersion = Application.version;
			battlEyeClientInitData.ulAddress = ulAddress;
			battlEyeClientInitData.usPort = usPort;
			battlEyeClientInitData.pfnPrintMessage = new BEClient.BECL_GAME_DATA.PrintMessageFn(battlEyeClientPrintMessage);
			battlEyeClientInitData.pfnRequestRestart = new BEClient.BECL_GAME_DATA.RequestRestartFn(battlEyeClientRequestRestart);
			battlEyeClientInitData.pfnSendPacket = new BEClient.BECL_GAME_DATA.SendPacketFn(battlEyeClientSendPacket);
			battlEyeClientRunData = new BEClient.BECL_BE_DATA();
			if (!beclientInitFn(2, battlEyeClientInitData, battlEyeClientRunData))
			{
				BEClient.FreeLibrary(battlEyeClientHandle);
				battlEyeClientHandle = IntPtr.Zero;
				Debug.LogError("Failed to call BattlEye client init!");
				result = false;
			}
		}
        catch { }

		return result;
	}

	private void UpdateBattlEye()
	{
		if (BattlEyeInitialized && battlEyeClientRunData != null && battlEyeClientRunData.pfnRun != null)
		{
			battlEyeClientRunData.pfnRun();
		}
	}

	private void ShutdownBattlEye()
	{
		if (BattlEyeInitialized)
		{
			Debug.LogWarning("[GameClient] Shutting down BattlEye");
			bool flag = battlEyeClientRunData.pfnExit();
			if (flag) { Debug.Log("[GameClient] BattlEye shutdown complete!"); }
			if (!flag) { Debug.Log("[GameClient] BattlEye shutdown failed!"); }
			BEServer.FreeLibrary(battlEyeClientHandle);
			BattlEyeInitialized = false;
		}
	}

	private void battlEyeClientPrintMessage(string message)
	{
		SendLocalMessage(Color.red, "BattlEye", Color.yellow, message);
		Debug.Log($"[GameClient] BattlEye: {message}");
	}

	private void battlEyeClientRequestRestart(int reason)
	{
		string reasonStr = string.Empty;
		if (reason == 0)
		{
			reasonStr = "BattlEye broken!";
		}
		else if (reason == 1)
		{
			reasonStr = "BattlEye need update!";
		}
		else
		{
			reasonStr = "BattlEye unknown!";
		}

		Debug.Log($"BattlEye client requested restart with reason: {reason} [{reasonStr}]");
	}

	private void battlEyeClientSendPacket(IntPtr packetHandle, int length)
	{
		using(ByteStream bs = ByteStream.GetBitStream())
        {
			bs.Write(MessageID.BattlEye);
			bs.Write(length);
			bs.Write(packetHandle, length);
			Send(bs.GetData(), bs.Length, PacketFlags.None, (byte)NetworkChannel.NetEvents);
        }
	}

	private unsafe void OnReceivedBattlEye(ByteStream stream, int length)
	{
		if (battlEyeClientHandle != IntPtr.Zero && battlEyeClientRunData != null && battlEyeClientRunData.pfnReceivedPacket != null)
		{
			int l = stream.Read<int>();
			if (l > 0)
			{
				byte[] data = stream.ReadBytes(l);
				fixed (byte* p = &data[0])
				{
					battlEyeClientRunData.pfnReceivedPacket(new IntPtr(p), l);
				}
				data = null;
			}
		}
	}
}
