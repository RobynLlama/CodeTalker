using Steamworks;

namespace CodeTalker.Packets;

/// <summary>
/// The packet header created for all received packets.
/// Code talker creates this for you automatically, there is no
/// need to ever use this class manually
/// </summary>
/// <param name="senderID">The steam64 of the packet's origin</param>
public class PacketHeader(ulong senderID)
{
  /// <summary>
  /// The Steam64 of the packet's origin
  /// </summary>
  public readonly ulong SenderID = senderID;

  /// <summary>
  /// Returns <em>TRUE</em> if the packet's origin is identical to the
  /// lobby owner.
  /// </summary>
  public bool SenderIsLobbyOwner =>
    SteamMatchmaking.GetLobbyOwner(new(SteamLobby._current._currentLobbyID)).m_SteamID == SenderID;
}
