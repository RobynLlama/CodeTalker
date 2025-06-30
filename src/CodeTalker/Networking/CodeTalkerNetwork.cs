using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Bootstrap;
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
    bool dbg = CodeTalkerPlugin.EnablePacketDebugging.Value;

    if (dbg)
      CodeTalkerPlugin.Log.LogDebug("Called back!");

    int bufferSize = 4096; //4kb buffer
    byte[] rawData = new byte[bufferSize];

    var ret = SteamMatchmaking.GetLobbyChatEntry(new(message.m_ulSteamIDLobby), (int)message.m_iChatID, out var senderID, rawData, bufferSize, out var messageType);
    string data = Encoding.UTF8.GetString(rawData[..ret]);

    if (!data.StartsWith(CODE_TALKER_SIGNATURE))
      return;

    data = data.Replace(CODE_TALKER_SIGNATURE, string.Empty);
    if (dbg)
    {
      CodeTalkerPlugin.Log.LogDebug($"Heard {ret} from GetLobbyChat. Sender {senderID}, type {messageType}");
      CodeTalkerPlugin.Log.LogDebug($"Full message: {data}");
    }


    PacketBase packet;
    Type inType;

    //We do it this way to make sure we're not blamed for errors
    //that other networked mods may cause

    try
    {
      if (JsonConvert.DeserializeObject<PacketBase>(data, PacketSerializer.JSONOptions) is PacketBase inPacket)
      {
        inType = inPacket.GetType();
        packet = inPacket;
      }
      else
        return;
    }
    catch (JsonReaderException)
    {
      CodeTalkerPlugin.Log.LogWarning($"""
      Malformed JSON in packet!
      Abridged Packet:
        {data[18..108]}

      """);
      return;
    }
    catch (JsonSerializationException ex)
    {
      //Silently ignore this unless debugging
      if (!dbg)
        return;

      CodeTalkerPlugin.Log.LogDebug($"""
      Unable to serialize a packet!
      This may be because our mods differ from the sender's!
      Stack Trace: 
        {ex}

      Abridged Packet:
        {data[18..108]}
      """);
      return;
    }
    catch (Exception ex)
    {
      CodeTalkerPlugin.Log.LogDebug($"""
      Error while handling a packet!
      Stack Trace: 
        {ex}

      Abridged Packet:
        {data[18..108]}
      """);
      return;
    }

    if (packetListeners.TryGetValue(inType, out var listener))
    {
      if (dbg)
        CodeTalkerPlugin.Log.LogDebug($"Sending an event for type {inType.Name}");

      try
      {
        listener.Invoke(new(senderID.m_SteamID), packet);
      }
      catch (Exception ex)
      {
        var plugins = Chainloader.PluginInfos;
        var mod = plugins.Values.Where(mod => mod.Instance?.GetType().Assembly == inType.Assembly).FirstOrDefault();

        //Happy lil ternary
        string modName = mod != null
          ? $"{mod.Metadata.Name} version {mod.Metadata.Version}"
          : inType.Assembly.GetName().Name;

        //Big beefin' raw string literal with interpolation
        CodeTalkerPlugin.Log.LogError($"""
        The following mod encountered an error while responding to a network packet, please do not report this as a CodeTalker error!
          Mod: {modName}
          StackTrace:
        {ex}
        """);
      }
    }

  }
}
