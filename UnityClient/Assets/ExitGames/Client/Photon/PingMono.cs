﻿// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PingMono
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Net.Sockets;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// Uses C# Socket class from System.Net.Sockets (as Unity usually does).
  /// </summary>
  /// 
  /// <remarks>
  /// Incompatible with Windows 8 Store/Phone API.
  /// </remarks>
  public class PingMono : PhotonPing
  {
    private Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    public override bool StartPing(string ip)
    {
      this.Init();
      try
      {
        this.sock.ReceiveTimeout = 5000;
        this.sock.Connect(ip, 5055);
        this.PingBytes[this.PingBytes.Length - 1] = this.PingId;
        this.sock.Send(this.PingBytes);
        this.PingBytes[this.PingBytes.Length - 1] = (byte) ((uint) this.PingId - 1U);
      }
      catch (Exception ex)
      {
        this.sock = (Socket) null;
        Console.WriteLine((object) ex);
      }
      return false;
    }

    public override bool Done()
    {
      if (this.GotResult || this.sock == null)
        return true;
      if (this.sock.Available <= 0)
        return false;
      int num = this.sock.Receive(this.PingBytes, SocketFlags.None);
      if ((int) this.PingBytes[this.PingBytes.Length - 1] != (int) this.PingId || num != this.PingLength)
        this.DebugString += " ReplyMatch is false! ";
      this.Successful = num == this.PingBytes.Length && (int) this.PingBytes[this.PingBytes.Length - 1] == (int) this.PingId;
      this.GotResult = true;
      return true;
    }

    public override void Dispose()
    {
      try
      {
        this.sock.Close();
      }
      catch
      {
      }
      this.sock = (Socket) null;
    }
  }
}
