using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using CodeTalker.Networking;
using Steamworks;

namespace CodeTalker;

/// <summary>
/// The main Code Talker entry point for BepInEx to load
/// </summary>
[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class CodeTalkerPlugin : BaseUnityPlugin
{
  internal static ManualLogSource Log = null!;
  internal static Callback<LobbyChatMsg_t>? onNetworkMessage;
  internal static ConfigEntry<bool> EnablePacketDebugging = null!;
  private void Awake()
  {

    Log = Logger;
    EnablePacketDebugging = Config.Bind("Debugging", "EnablePacketDebugging", false, "If CodeTalker should dump packet information (this will be on the debug channel, make sure that is enabled in BepInEx.cfg)");

    Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

    onNetworkMessage = Callback<LobbyChatMsg_t>.Create(CodeTalkerNetwork.OnNetworkMessage);
    Log.LogMessage("Created a steam lobby callback");
  }
}
