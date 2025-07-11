using System.Text;
using BepInEx;
using BepInEx.Logging;
using CodeTalker.Networking;
using CodeTalker.PacketExamples;
using CodeTalker.Packets;

namespace CodeTalker;

/*
  This example goes over how to properly register a packet
  listener with CodeTalker. See also: ExamplePacket.cs
*/

[BepInPlugin("dev.mamallama.CodeTalkerExampleListener", "Code Talker Example", "0.0.1")]
public class CodeTalkerExamplePlugin : BaseUnityPlugin
{
  internal static ManualLogSource Log = null!;

  private void Awake()
  {

    Log = Logger;

    // Log our awake here so we can see it in LogOutput.txt file
    Log.LogInfo($"Code talker example is loaded");

    CodeTalkerNetwork.RegisterListener<ExamplePacket>(ReceiveExamplePacket);
    Log.LogMessage("Created a packet listener");
  }

  internal static void ReceiveExamplePacket(PacketHeader header, PacketBase packet)
  {
    if (packet is ExamplePacket example)
    {
      Log.LogInfo($"Packet\n  From: {example.PacketSourceGUID} (isHost: {header.SenderIsLobbyOwner})\n  PayLoad: {example.Payload}");
    }
  }

  internal static void SendPacketTest(string payload)
  {
    CodeTalkerNetwork.SendNetworkPacket(new ExamplePacket(payload));
  }

}
