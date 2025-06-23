using BepInEx;
using BepInEx.Logging;
using CodeTalker.Networking;
using Steamworks;

namespace CodeTalker;

[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class CodeTalkerPlugin : BaseUnityPlugin
{
  internal static ManualLogSource Log = null!;
  internal static Callback<LobbyChatMsg_t>? onNetworkMessage;

  private void Awake()
  {

    Log = Logger;
    Log.LogInfo($"Plugin {LCMPluginInfo.PLUGIN_NAME} version {LCMPluginInfo.PLUGIN_VERSION} is loaded!");

    onNetworkMessage = Callback<LobbyChatMsg_t>.Create(CodeTalkerNetwork.OnNetworkMessage);
    Log.LogMessage("Created a steam lobby callback");
  }
}
