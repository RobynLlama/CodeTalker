using Newtonsoft.Json;

namespace CodeTalker.Packets;

/// <summary>
/// The base packet type all packets must be derived from
/// </summary>
public abstract class PacketBase
{
  /// <summary>
  /// A string denoting what mod sent this packet. Please set it
  /// based on your mod's GUID to track your own packets or make
  /// inspecting network traffic easier
  /// </summary>
  [JsonProperty]
  public abstract string PacketSourceGUID { get; }
}
