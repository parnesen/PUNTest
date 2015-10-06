// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.ConnectionProtocol
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// These are the options that can be used as underlying transport protocol.
  /// 
  /// </summary>
  public enum ConnectionProtocol : byte
  {
    Udp = (byte) 0,
    Tcp = (byte) 1,
    WebSocket = (byte) 4,
    WebSocketSecure = (byte) 5,
  }
}
