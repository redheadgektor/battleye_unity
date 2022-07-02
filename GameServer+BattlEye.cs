using BattlEye;
using ENet;
using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

public sealed partial class GameServer : ENetServer
{
    IntPtr battlEyeServerHandle = IntPtr.Zero;
    BEServer.BESV_GAME_DATA battlEyeServerInitData = null;
    BEServer.BESV_BE_DATA battlEyeServerRunData = null;
    private bool BattlEyeInitialized = false;

    private bool InitializeBattlEye()
    {
        string text = Path.Combine(Environment.CurrentDirectory, "BEServer_x64.dll");
        if (!File.Exists(text))
        {
            text = Path.Combine(Environment.CurrentDirectory, "BEServer.dll");
        }
        if (!File.Exists(text))
        {
            Debug.LogError("Missing BattlEye server library! (" + text + ")");
            return false;
        }

        Debug.Log("Try loading BattlEye server library from: " + text);
        bool result = false;
        try
        {
            battlEyeServerHandle = BEServer.LoadLibraryW(text);

            if (battlEyeServerHandle != IntPtr.Zero)
            {
                BEServer.BEServerInitFn beserverInitFn = Marshal.GetDelegateForFunctionPointer(BEServer.GetProcAddress(battlEyeServerHandle, "Init"), typeof(BEServer.BEServerInitFn)) as BEServer.BEServerInitFn;
                if (beserverInitFn != null)
                {
                    battlEyeServerInitData = new BEServer.BESV_GAME_DATA();
                    battlEyeServerInitData.pstrGameVersion = Application.version;
                    battlEyeServerInitData.pfnPrintMessage = new BEServer.BESV_GAME_DATA.PrintMessageFn(battlEyeServerPrintMessage);
                    battlEyeServerInitData.pfnKickPlayer = new BEServer.BESV_GAME_DATA.KickPlayerFn(battlEyeServerKickPlayer);
                    battlEyeServerInitData.pfnSendPacket = new BEServer.BESV_GAME_DATA.SendPacketFn(battlEyeServerSendPacket);
                    battlEyeServerRunData = new BEServer.BESV_BE_DATA();

                    Debug.Log("[BattlEye] Initialized...");

                    if (beserverInitFn(0, battlEyeServerInitData, battlEyeServerRunData))
                    {
                        result = true;
                    }
                    else
                    {
                        BEServer.FreeLibrary(battlEyeServerHandle);
                        battlEyeServerHandle = IntPtr.Zero;
                        Debug.LogError("Failed to call BattlEye server init!");
                        result = false;
                    }
                }
                else
                {
                    BEServer.FreeLibrary(battlEyeServerHandle);
                    battlEyeServerHandle = IntPtr.Zero;
                    Debug.LogError("Failed to get BattlEye server init delegate!");
                    result = false;
                }
            }
            else
            {
                Debug.LogError("Failed to load BattlEye server library!");
                result = false;
            }
        }
        catch
        {

        }

        return result;
    }

    private void UpdateBattlEye()
    {
        if (BattlEyeInitialized && battlEyeServerRunData != null && battlEyeServerRunData.pfnRun != null)
        {
            battlEyeServerRunData.pfnRun();
        }
    }

    private void ShutdownBattlEye()
    {
        if (BattlEyeInitialized && battlEyeServerRunData != null)
        {
            Debug.LogWarning("[GameServer] Shutting down BattlEye");
            bool flag = battlEyeServerRunData.pfnExit();
            if (flag) { Debug.Log("[GameServer] BattlEye shutdown complete!"); }
            if (!flag) { Debug.Log("[GameServer] BattlEye shutdown failed!"); }
            BEServer.FreeLibrary(battlEyeServerHandle);
            BattlEyeInitialized = false;
        }
    }

    private void battlEyeServerPrintMessage(string message)
    {
        SendChatMessage(Color.red, "BattlEye", Color.yellow, $"{message}");
        Debug.Log($"[GameServer] BattlEye: {message}");
    }

    private void battlEyeServerKickPlayer(int clientID, string reason)
    {
        SendChatMessage(Color.red, "BattlEye", Color.yellow, $"Kick player {Clients_Array[clientID].playerName} - {reason}");
        Debug.Log($"[GameServer] BattlEye pfnKickPlayer: {Clients_Array[clientID].playerName} - {reason}");
        Kick(Clients_Array[clientID].Peer, DisconnectReason.BattlEye, reason);
    }

    private void battlEyeServerSendPacket(int playerID, IntPtr packetHandle, int length)
    {
        if (Clients_Array[playerID] != null)
        {
            using (ByteStream bs = ByteStream.GetBitStream())
            {
                bs.Write(MessageID.BattlEye);
                bs.Write(length);
                bs.Write(packetHandle, length);
                Send(Clients_Array[playerID].Peer.ID, bs.GetData(), bs.Length, PacketFlags.None, (byte)NetworkChannel.NetEvents);
            }
        }
    }

    private unsafe void OnReceivedBattlEye(ServerClientInfo info, ByteStream stream)
    {
        if (battlEyeServerHandle != IntPtr.Zero && battlEyeServerRunData != null && battlEyeServerRunData.pfnReceivedPacket != null)
        {
            int length = stream.Read<int>();
            if (length > 0)
            {
                byte[] data = stream.ReadBytes(length);
                fixed (byte* p = &data[0])
                {
                    battlEyeServerRunData.pfnReceivedPacket(info.clientID, new IntPtr(p), length);
                }
                data = null;
                return;
            }
            Debug.LogWarning($"[GameServer] Received empty BattlEye payload from [{info.clientID}] {info.playerName}, so we're refusing them");
            Kick(info.Peer, DisconnectReason.BattlEye, "Refused by BattlEye");
        }
    }
}
