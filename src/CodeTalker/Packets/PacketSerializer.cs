using Newtonsoft.Json;

namespace CodeTalker.Packets;

public static class PacketSerializer
{
  public static readonly JsonSerializerSettings JSONOptions = new()
  {
    TypeNameHandling = TypeNameHandling.Objects,
    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
    Formatting = Formatting.None,
  };
}
