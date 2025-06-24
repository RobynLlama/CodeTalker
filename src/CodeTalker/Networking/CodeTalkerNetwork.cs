using System;
using System.Collections.Generic;
using System.Text;
using CodeTalker.Packets;
using Newtonsoft.Json;
using Steamworks;

namespace CodeTalker.Networking;

/// <summary>
/// The main network entry point for Code Talker
/// </summary>
public static class CodeTalkerNetwork
{
  /// <summary>
  /// A delegate for receiving packet events from Code Talker
  /// </summary>
  /// <param name="header">The received packet's header</param>
  /// <param name="packet">The received packet's body</param>
  public delegate void PacketListener(PacketHeader header, PacketBase packet);
  internal static string CODE_TALKER_SIGNATURE = "!!CODE_TALKER_NETWORKING!!";
  private static readonly Dictionary<Type, PacketListener> packetListeners = [];

  /// <summary>
  /// Registers a listener by packet type. Code Talker will call
  /// your listener when the specific System.Type is created from
  /// a serial network message and pass it to your listener
  /// </summary>
  /// <typeparam name="T">The exact runtime type of your packet</typeparam>
  /// <param name="listener">A PacketListener delegate to notify</param>
  /// <returns>
  /// Will return <em>TRUE</em> if the listener is added to the list and
  /// <em>FALSE</em> is a listener already exists for this type
  /// </returns>
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

  /// <summary>
  /// Wraps and sends a message to all clients on the Code Talker network
  /// </summary>
  /// <param name="packet">The packet to send, must be derived from PacketBase</param>
  public static void SendNetworkPacket(PacketBase packet)
  {
    var rawPacket = $"{CODE_TALKER_SIGNATURE}{JsonConvert.SerializeObject(packet, PacketSerializer.JSONOptions)}";
    var bytes = Encoding.UTF8.GetBytes(rawPacket);
    SteamMatchmaking.SendLobbyChatMsg(new(SteamLobby._current._currentLobbyID), bytes, bytes.Length);
  }

  internal static void OnNetworkMessage(LobbyChatMsg_t message)
  {
    CodeTalkerPlugin.Log.LogDebug("Called back!");

    int bufferSize = 10240; //10kb buffer
    byte[] rawData = new byte[bufferSize];

    var ret = SteamMatchmaking.GetLobbyChatEntry(new(message.m_ulSteamIDLobby), (int)message.m_iChatID, out var senderID, rawData, bufferSize, out var messageType);
    string data = Encoding.UTF8.GetString(rawData[..ret]);

    if (!data.StartsWith(CODE_TALKER_SIGNATURE))
      return;

    data = data.Replace(CODE_TALKER_SIGNATURE, string.Empty);
    CodeTalkerPlugin.Log.LogDebug($"Heard {ret} from GetLobbyChat. Sender {senderID}, type {messageType}");
    CodeTalkerPlugin.Log.LogDebug($"Full message: {data}");

    try
    {
      if (JsonConvert.DeserializeObject<PacketBase>(data, PacketSerializer.JSONOptions) is PacketBase packet)
      {
        var inType = packet.GetType();
        if (packetListeners.TryGetValue(inType, out var listener))
        {
          CodeTalkerPlugin.Log.LogDebug($"Sending an event for type {inType.Name}");
          listener.Invoke(new(senderID.m_SteamID), packet);
        }
      }
    }
    catch (Exception ex)
    {
      CodeTalkerPlugin.Log.LogError($"Error while hearing CodeTalker network message\n{ex}\n");
      return;
    }
  }
}
