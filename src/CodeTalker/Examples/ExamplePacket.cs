using CodeTalker.Packets;
using Newtonsoft.Json;

namespace CodeTalker.PacketExamples;

public class ExamplePacket(string payload) : PacketBase
{
  /*
      This example packet contains a simple Payload property,
      but you can include any number of properties of any type.
      However, I strongly recommend sticking to primitive types.

      Remember: Your data is serialized and sent across the network,
      so reference types (like classes) will not point to the same
      object instance on both ends.

      Instead, rely on value types or objects that can be represented
      as values (such as strings, ints, uints, floats, etc) when 
      sending messages.
  */

  public override string PacketSourceGUID => LCMPluginInfo.PLUGIN_GUID;

  [JsonProperty]
  public string Payload { get; set; } = payload;
}
