// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SocketUdpNativeStatic
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  public class SocketUdpNativeStatic : IPhotonSocket
  {
    public SocketUdpNativeStatic(PeerBase peerBase)
      : base(peerBase)
    {
    }

    public override bool Disconnect()
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }

    public override PhotonSocketError Send(byte[] data, int length)
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }

    public override PhotonSocketError Receive(out byte[] data)
    {
      throw new NotImplementedException("This class was compiled in an assembly WITH c# sockets. Another dll must be used for native sockets.");
    }
  }
}
