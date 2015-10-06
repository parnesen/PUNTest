// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PhotonPing
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;

namespace ExitGames.Client.Photon
{
  public abstract class PhotonPing : IDisposable
  {
    public string DebugString = "";
    protected internal int PingLength = 13;
    protected internal byte[] PingBytes = new byte[13]
    {
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 125,
      (byte) 0
    };
    public bool Successful;
    protected internal bool GotResult;
    protected internal byte PingId;

    public virtual bool StartPing(string ip)
    {
      throw new NotImplementedException();
    }

    public virtual bool Done()
    {
      throw new NotImplementedException();
    }

    public virtual void Dispose()
    {
      throw new NotImplementedException();
    }

    protected internal void Init()
    {
      this.GotResult = false;
      this.Successful = false;
      this.PingId = (byte) (Environment.TickCount % (int) byte.MaxValue);
    }
  }
}
