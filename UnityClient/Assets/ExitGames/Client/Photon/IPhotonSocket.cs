﻿// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.IPhotonSocket
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Net;
using System.Net.Sockets;

namespace ExitGames.Client.Photon
{
  public abstract class IPhotonSocket
  {
    internal PeerBase peerBase;
    public bool PollReceive;

    protected IPhotonPeerListener Listener
    {
      get
      {
        return this.peerBase.Listener;
      }
    }

    public ConnectionProtocol Protocol { get; protected set; }

    public PhotonSocketState State { get; protected set; }

    public string ServerAddress { get; protected set; }

    public int ServerPort { get; protected set; }

    public bool Connected
    {
      get
      {
        return this.State == PhotonSocketState.Connected;
      }
    }

    public int MTU
    {
      get
      {
        return this.peerBase.mtu;
      }
    }

    public IPhotonSocket(PeerBase peerBase)
    {
      if (peerBase == null)
        throw new Exception("Can't init without peer");
      this.peerBase = peerBase;
    }

    public virtual bool Connect()
    {
      if (this.State != PhotonSocketState.Disconnected)
      {
        if (this.peerBase.debugOut >= DebugLevel.ERROR)
          this.peerBase.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: connection in State: " + (object) this.State);
        return false;
      }
      if (this.peerBase == null || this.Protocol != this.peerBase.usedProtocol)
        return false;
      string address;
      ushort port;
      if (!this.TryParseAddress(this.peerBase.ServerAddress, out address, out port))
      {
        if (this.peerBase.debugOut >= DebugLevel.ERROR)
          this.peerBase.Listener.DebugReturn(DebugLevel.ERROR, "Failed parsing address: " + this.peerBase.ServerAddress);
        return false;
      }
      this.ServerAddress = address;
      this.ServerPort = (int) port;
      return true;
    }

    public abstract bool Disconnect();

    public abstract PhotonSocketError Send(byte[] data, int length);

    public abstract PhotonSocketError Receive(out byte[] data);

    public void HandleReceivedDatagram(byte[] inBuffer, int length, bool willBeReused)
    {
      if (this.peerBase.NetworkSimulationSettings.IsSimulationEnabled)
      {
        if (willBeReused)
        {
          byte[] inBufferCopy = new byte[length];
          Buffer.BlockCopy((Array) inBuffer, 0, (Array) inBufferCopy, 0, length);
          this.peerBase.ReceiveNetworkSimulated((PeerBase.MyAction) (() => this.peerBase.ReceiveIncomingCommands(inBufferCopy, length)));
        }
        else
          this.peerBase.ReceiveNetworkSimulated((PeerBase.MyAction) (() => this.peerBase.ReceiveIncomingCommands(inBuffer, length)));
      }
      else
        this.peerBase.ReceiveIncomingCommands(inBuffer, length);
    }

    public bool ReportDebugOfLevel(DebugLevel levelOfMessage)
    {
      return this.peerBase.debugOut >= levelOfMessage;
    }

    public void EnqueueDebugReturn(DebugLevel debugLevel, string message)
    {
      this.peerBase.EnqueueDebugReturn(debugLevel, message);
    }

    protected internal void HandleException(StatusCode statusCode)
    {
      this.State = PhotonSocketState.Disconnecting;
      this.peerBase.EnqueueStatusCallback(statusCode);
      this.peerBase.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.peerBase.Disconnect()));
    }

    /// <summary>
    /// Ports of IPv4 Addresses are of type ushort. They are used as int usually though.
    /// 
    /// </summary>
    protected internal bool TryParseAddress(string addressAndPort, out string address, out ushort port)
    {
      address = string.Empty;
      port = (ushort) 0;
      if (string.IsNullOrEmpty(addressAndPort))
        return false;
      string[] strArray = addressAndPort.Split(':');
      if (strArray.Length != 2)
        return false;
      address = strArray[0];
      return ushort.TryParse(strArray[1], out port);
    }

    protected internal static IPAddress GetIpAddress(string serverIp)
    {
      IPAddress address = (IPAddress) null;
      if (IPAddress.TryParse(serverIp, out address))
        return address;
      foreach (IPAddress ipAddress in Dns.GetHostEntry(serverIp).AddressList)
      {
        if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
          return ipAddress;
      }
      return (IPAddress) null;
    }
  }
}
