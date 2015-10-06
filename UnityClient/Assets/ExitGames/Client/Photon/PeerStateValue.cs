// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PeerStateValue
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Value range for a Peer's Connection and initialization state, as returned by the PeerState property.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// While this is not the same as the StatusCode of IPhotonPeerListener.OnStatusChanged(), it directly relates to it.
  ///             In most cases, it makes more sense to build a game's state on top of the OnStatusChanged() as you get changes.
  /// 
  /// </remarks>
  public enum PeerStateValue : byte
  {
    Disconnected = (byte) 0,
    Connecting = (byte) 1,
    Connected = (byte) 3,
    Disconnecting = (byte) 4,
    InitializingApplication = (byte) 10,
  }
}
