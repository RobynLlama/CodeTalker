using Steamworks;

namespace CodeTalker.Packets;

public class PacketHeader(ulong senderID)
{
  public readonly ulong SenderID = senderID;

  public bool SenderIsLobbyOwner =>
    SteamMatchmaking.GetLobbyOwner(new(SteamLobby._current._currentLobbyID)).m_SteamID == SenderID;
}
