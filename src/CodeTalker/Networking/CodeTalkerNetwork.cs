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

  /// <summary>
  /// This should only be modified for BREAKING changes in packet structure
  /// </summary>
  private const ushort NETWORK_PACKET_VERSION = 2;

  internal static string CODE_TALKER_SIGNATURE = $"!!CODE_TALKER_NETWORKING:PV{NETWORK_PACKET_VERSION}!!";
  private static readonly Dictionary<string, PacketListener> packetListeners = [];

  internal static string GetTypeNameString(Type type) =>
    $"{type.Assembly.GetName().Name},{type.DeclaringType?.Name ?? "NONE"}:{type.Namespace ?? "NONE"}.{type.Name}";

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
    string typeName = GetTypeNameString(inType);

    if (packetListeners.ContainsKey(typeName))
    {
      return false;
    }

    packetListeners.Add(typeName, listener);

    return true;
  }

  /// <summary>
  /// Wraps and sends a message to all clients on the Code Talker network
  /// </summary>
  /// <param name="packet">The packet to send, must be derived from PacketBase</param>
  public static void SendNetworkPacket(PacketBase packet)
  {
    string rawPacket = JsonConvert.SerializeObject(packet, PacketSerializer.JSONOptions);
    PacketWrapper wrapper = new(GetTypeNameString(packet.GetType()), rawPacket);

    var rawWrapper = $"{CODE_TALKER_SIGNATURE}{JsonConvert.SerializeObject(wrapper, PacketSerializer.JSONOptions)}";
    var bytes = Encoding.UTF8.GetBytes(rawWrapper);
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

    PacketWrapper wrapper;
    PacketBase packet;
    Type inType;

    //We do it this way to make sure we're not blamed for errors
    //that other networked mods may cause

    try
    {
      if (JsonConvert.DeserializeObject<PacketWrapper>(data, PacketSerializer.JSONOptions) is PacketWrapper inWrapper)
        wrapper = inWrapper;
      else
        throw new InvalidOperationException("Failed to deserialize a valid packet wrapper");
    }
    catch (Exception ex)
    {
      string aData;

      if (data.Length < 24)
        aData = data;
      else
        aData = data[..24];

      CodeTalkerPlugin.Log.LogError($"""
      Error while receiving a packet!
      Exception: {ex.GetType().Name}
      Abridged Packet:
        {aData}
      """);
      return;
    }

    if (!packetListeners.TryGetValue(wrapper.PacketType, out var listener))
    {
      if (dbg)
        CodeTalkerPlugin.Log.LogDebug($"Skipping packet of type: {wrapper.PacketType} because this client does not have it installed, this is safe!");

      return;
    }

    try
    {
      if (JsonConvert.DeserializeObject<PacketBase>(wrapper.PacketPayload, PacketSerializer.JSONOptions) is PacketBase inPacket)
      {
        inType = inPacket.GetType();
        packet = inPacket;
      }
      else
        return;
    }
    catch (Exception ex)
    {
      CodeTalkerPlugin.Log.LogError($"""
      Error while unwrapping a packet!
      Exception: {ex.GetType().Name}
      Expected Type: {wrapper.PacketType}
      """);
      return;
    }

    if (dbg)
      CodeTalkerPlugin.Log.LogDebug($"Sending an event for type {wrapper.PacketType}");

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
