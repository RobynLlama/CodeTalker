using System.Collections.Generic;
using Newtonsoft.Json;

namespace CodeTalker.Packets;

internal class PacketWrapper
{
  [JsonProperty]
  public readonly string PacketType;

  [JsonProperty]
  public readonly string PacketPayload;

  [JsonProperty]
  public readonly Dictionary<string, string> MetaInf;

  [JsonConstructor]
  public PacketWrapper(string PacketType, string PacketPayload, Dictionary<string, string>? MetaInf = null)
  {
    this.PacketType = PacketType;
    this.PacketPayload = PacketPayload;
    this.MetaInf = MetaInf ?? [];
  }
}
