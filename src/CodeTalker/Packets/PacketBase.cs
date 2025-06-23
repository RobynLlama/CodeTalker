using Newtonsoft.Json;

namespace CodeTalker.Packets;

public abstract class PacketBase
{
  [JsonProperty]
  public abstract string PacketSourceGUID { get; }
}
