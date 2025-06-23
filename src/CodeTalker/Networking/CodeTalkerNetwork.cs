using System;
using System.Collections.Generic;
using System.Text;
using CodeTalker.Packets;
using Newtonsoft.Json;
using Steamworks;

namespace CodeTalker.Networking;

public static class CodeTalkerNetwork
{
  public delegate void PacketListener(PacketHeader header, PacketBase packet);
  private static readonly Dictionary<Type, PacketListener> packetListeners = [];

  public static bool RegisterListener<T>(PacketListener listener) where T : PacketBase
  {
    var inType = typeof(T);

    if (packetListeners.ContainsKey(inType))
    {
      return false;
    }

    packetListeners.Add(inType, listener);

    return true;
  }

  public static void SendNetworkPacket(PacketBase packet)
  {
    var rawPacket = JsonConvert.SerializeObject(packet, PacketSerializer.JSONOptions);
    var bytes = Encoding.UTF8.GetBytes(rawPacket);
    SteamMatchmaking.SendLobbyChatMsg(new(SteamLobby._current._currentLobbyID), bytes, bytes.Length);
  }

  internal static void OnNetworkMessage(LobbyChatMsg_t message)
  {
    CodeTalkerPlugin.Log.LogMessage("Called back!");

    int bufferSize = 10240; //10kb buffer
    byte[] rawData = new byte[bufferSize];

    var ret = SteamMatchmaking.GetLobbyChatEntry(new(message.m_ulSteamIDLobby), (int)message.m_iChatID, out var senderID, rawData, bufferSize, out var messageType);
    string data = Encoding.UTF8.GetString(rawData[..ret]);

    CodeTalkerPlugin.Log.LogMessage($"Heard {ret} from GetLobbyChat. Sender {senderID}, type {messageType}");
    CodeTalkerPlugin.Log.LogMessage($"Full message: {data}");

    try
    {
      if (JsonConvert.DeserializeObject<PacketBase>(data, PacketSerializer.JSONOptions) is PacketBase packet)
      {
        var inType = packet.GetType();
        if (packetListeners.TryGetValue(inType, out var listener))
        {
          CodeTalkerPlugin.Log.LogMessage($"Sending an event for type {inType.Name}");
          listener.Invoke(new(senderID.m_SteamID), packet);
        }
      }
    }
    catch (Exception ex)
    {
      CodeTalkerPlugin.Log.LogError($"Error while hearing network message\n{ex}\n");
      return;
    }
  }
}
