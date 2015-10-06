// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.TPeer
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections.Generic;
using System.IO;

namespace ExitGames.Client.Photon
{
  internal class TPeer : PeerBase
  {
    internal static readonly byte[] tcpFramedMessageHead = new byte[9]
    {
      (byte) 251,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 0,
      (byte) 243,
      (byte) 2
    };
    internal static readonly byte[] tcpMsgHead = new byte[2]
    {
      (byte) 243,
      (byte) 2
    };
    /// <summary>
    /// TCP "Package" header: 7 bytes
    /// </summary>
    internal const int TCP_HEADER_BYTES = 7;
    /// <summary>
    /// TCP "Message" header: 2 bytes
    /// </summary>
    internal const int MSG_HEADER_BYTES = 2;
    /// <summary>
    /// TCP header combined: 9 bytes
    /// </summary>
    public const int ALL_HEADER_BYTES = 9;
    private Queue<byte[]> incomingList;
    internal List<byte[]> outgoingStream;
    private int lastPingResult;
    private byte[] pingRequest;
    internal byte[] messageHeader;
    /// <summary>
    /// Defines if the (TCP) socket implementation needs to do "framing".
    /// </summary>
    /// 
    /// <remarks>
    /// The WebSocket protocol (e.g.) includes framing, so when that is used, we set DoFraming to false.
    /// </remarks>
    protected internal bool DoFraming;

    internal override int QueuedIncomingCommandsCount
    {
      get
      {
        return this.incomingList.Count;
      }
    }

    internal override int QueuedOutgoingCommandsCount
    {
      get
      {
        return this.outgoingCommandsInStream;
      }
    }

    internal TPeer()
    {
      byte[] numArray = new byte[5];
      numArray[0] = (byte) 240;
      this.pingRequest = numArray;
      this.DoFraming = true;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      ++PeerBase.peerCount;
      this.InitOnce();
      this.TrafficPackageHeaderSize = 0;
    }

    internal TPeer(IPhotonPeerListener listener)
      : this()
    {
      this.Listener = listener;
    }

    internal override void InitPeerBase()
    {
      base.InitPeerBase();
      this.incomingList = new Queue<byte[]>(32);
    }

    internal override bool Connect(string serverAddress, string appID)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
      {
        this.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
      this.ServerAddress = serverAddress;
      this.InitPeerBase();
      this.outgoingStream = new List<byte[]>();
      if (appID == null)
        appID = "LoadBalancing";
      for (int index = 0; index < 32; ++index)
        this.INIT_BYTES[index + 9] = index < appID.Length ? (byte) appID[index] : (byte) 0;
      if (this.SocketImplementation != null)
        this.rt = (IPhotonSocket) Activator.CreateInstance(this.SocketImplementation, new object[1]
        {
          (object) this
        });
      else
        this.rt = (IPhotonSocket) new SocketTcp((PeerBase) this);
      if (this.rt == null)
      {
        this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect(). SocketImplementation: " + (object) this.SocketImplementation);
        return false;
      }
      this.messageHeader = this.DoFraming ? TPeer.tcpFramedMessageHead : TPeer.tcpMsgHead;
      if (this.rt.Connect())
      {
        this.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
        this.EnqueueInit();
        this.SendOutgoingCommands();
        return true;
      }
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      return false;
    }

    internal override void Disconnect()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
        return;
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "TPeer.Disconnect()");
      this.StopConnection();
    }

    internal override void StopConnection()
    {
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
      if (this.rt != null)
        this.rt.Disconnect();
      lock (this.incomingList)
        this.incomingList.Clear();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void FetchServerTimestamp()
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + (object) this.peerConnectionState);
        this.Listener.OnStatusChanged(StatusCode.SendError);
      }
      else
      {
        this.SendPing();
        this.serverTimeOffsetIsAvailable = false;
      }
    }

    private void EnqueueInit()
    {
      if (!this.DoFraming)
        return;
      MemoryStream memoryStream = new MemoryStream(0);
      BinaryWriter binaryWriter = new BinaryWriter((Stream) memoryStream);
      byte[] numArray1 = new byte[7];
      numArray1[0] = (byte) 251;
      numArray1[6] = (byte) 1;
      byte[] numArray2 = numArray1;
      int targetOffset = 1;
      Protocol.Serialize(this.INIT_BYTES.Length + numArray2.Length, numArray2, ref targetOffset);
      binaryWriter.Write(numArray2);
      binaryWriter.Write(this.INIT_BYTES);
      byte[] opMessage = memoryStream.ToArray();
      if (this.TrafficStatsEnabled)
      {
        ++this.TrafficStatsOutgoing.TotalPacketCount;
        ++this.TrafficStatsOutgoing.TotalCommandsInPackets;
        this.TrafficStatsOutgoing.CountControlCommand(opMessage.Length);
      }
      this.EnqueueMessageAsPayload(true, opMessage, (byte) 0);
    }

    /// <summary>
    /// Checks the incoming queue and Dispatches received data if possible. Returns if a Dispatch happened or
    ///             not, which shows if more Dispatches might be needed.
    /// 
    /// </summary>
    internal override bool DispatchIncomingCommands()
    {
      while (true)
      {
        PeerBase.MyAction myAction;
        lock (this.ActionQueue)
        {
          if (this.ActionQueue.Count > 0)
            myAction = this.ActionQueue.Dequeue();
          else
            break;
        }
        myAction();
      }
      byte[] inBuff;
      lock (this.incomingList)
      {
        if (this.incomingList.Count <= 0)
          return false;
        inBuff = this.incomingList.Dequeue();
      }
      this.ByteCountCurrentDispatch = inBuff.Length + 3;
      return this.DeserializeMessageAndCallback(inBuff);
    }

    /// <summary>
    /// gathers commands from all (out)queues until udp-packet is full and sends it!
    /// 
    /// </summary>
    internal override bool SendOutgoingCommands()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || !this.rt.Connected)
        return false;
      this.timeInt = SupportClass.GetTickCount() - this.timeBase;
      this.timeLastSendOutgoing = this.timeInt;
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected && SupportClass.GetTickCount() - this.lastPingResult > this.timePingInterval)
        this.SendPing();
      lock (this.outgoingStream)
      {
        foreach (byte[] item_0 in this.outgoingStream)
          this.SendData(item_0);
        this.outgoingStream.Clear();
        this.outgoingCommandsInStream = 0;
      }
      return false;
    }

    /// <summary>
    /// Sends a ping in intervals to keep Connection alive (server will timeout Connection if nothing is sent).
    /// </summary>
    /// 
    /// <returns>
    /// Always false in this case (local queues are ignored. true would be: "call again to send remaining data").
    /// </returns>
    internal override bool SendAcksOnly()
    {
      if (this.rt == null || !this.rt.Connected)
        return false;
      this.timeInt = SupportClass.GetTickCount() - this.timeBase;
      this.timeLastSendOutgoing = this.timeInt;
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected && SupportClass.GetTickCount() - this.lastPingResult > this.timePingInterval)
        this.SendPing();
      return false;
    }

    internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, string.Concat(new object[4]
          {
            (object) "Cannot send op: ",
            (object) opCode,
            (object) "! Not connected. PeerState: ",
            (object) this.peerConnectionState
          }));
        this.Listener.OnStatusChanged(StatusCode.SendError);
        return false;
      }
      if ((int) channelId >= (int) this.ChannelCount)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Cannot send op: Selected channel (" + (object) channelId + ")>= channelCount (" + (string) (object) this.ChannelCount + ").");
        this.Listener.OnStatusChanged(StatusCode.SendError);
        return false;
      }
      byte[] opMessage = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
      return this.EnqueueMessageAsPayload(sendReliable, opMessage, channelId);
    }

    /// <summary>
    /// Returns the UDP Payload starting with Magic Number for binary protocol
    /// </summary>
    internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
    {
      byte[] target;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(this.messageHeader, 0, this.messageHeader.Length);
        Protocol.SerializeOperationRequest(this.SerializeMemStream, opc, parameters, false);
        if (encrypt)
        {
          byte[] local_1_1 = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(this.messageHeader, 0, this.messageHeader.Length);
          this.SerializeMemStream.Write(local_1_1, 0, local_1_1.Length);
        }
        target = this.SerializeMemStream.ToArray();
      }
      if (messageType != PeerBase.EgMessageType.Operation)
        target[this.messageHeader.Length - 1] = (byte) messageType;
      if (encrypt)
        target[this.messageHeader.Length - 1] = (byte) ((uint) target[this.messageHeader.Length - 1] | 128U);
      if (this.DoFraming)
      {
        int targetOffset = 1;
        Protocol.Serialize(target.Length, target, ref targetOffset);
      }
      return target;
    }

    /// <summary>
    /// enqueues serialized operations to be sent as tcp stream / package
    /// </summary>
    internal bool EnqueueMessageAsPayload(bool sendReliable, byte[] opMessage, byte channelId)
    {
      if (opMessage == null)
        return false;
      if (this.DoFraming)
      {
        opMessage[5] = channelId;
        opMessage[6] = sendReliable ? (byte) 1 : (byte) 0;
      }
      lock (this.outgoingStream)
      {
        this.outgoingStream.Add(opMessage);
        ++this.outgoingCommandsInStream;
        if (this.outgoingCommandsInStream % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
      }
      int length = opMessage.Length;
      this.ByteCountLastOperation = length;
      if (this.TrafficStatsEnabled)
      {
        if (sendReliable)
          this.TrafficStatsOutgoing.CountReliableOpCommand(length);
        else
          this.TrafficStatsOutgoing.CountUnreliableOpCommand(length);
        this.TrafficStatsGameLevel.CountOperation(length);
      }
      return true;
    }

    /// <summary>
    /// Sends a ping and modifies this.lastPingResult to avoid another ping for a while.
    /// </summary>
    internal void SendPing()
    {
      this.lastPingResult = SupportClass.GetTickCount();
      if (!this.DoFraming)
      {
        this.EnqueueOperation(new Dictionary<byte, object>()
        {
          {
            (byte) 1,
            (object) SupportClass.GetTickCount()
          }
        }, PhotonCodes.Ping, true, (byte) 0, false, PeerBase.EgMessageType.InternalOperationRequest);
      }
      else
      {
        int targetOffset = 1;
        Protocol.Serialize(SupportClass.GetTickCount(), this.pingRequest, ref targetOffset);
        if (this.TrafficStatsEnabled)
          this.TrafficStatsOutgoing.CountControlCommand(this.pingRequest.Length);
        this.SendData(this.pingRequest);
      }
    }

    internal void SendData(byte[] data)
    {
      try
      {
        this.bytesOut += (long) data.Length;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += this.outgoingCommandsInStream;
        }
        if (this.NetworkSimulationSettings.IsSimulationEnabled)
        {
          int num;
          this.SendNetworkSimulated((PeerBase.MyAction) (() => num = (int) this.rt.Send(data, data.Length)));
        }
        else
        {
          int num1 = (int) this.rt.Send(data, data.Length);
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
        SupportClass.WriteStackTrace(ex);
      }
    }

    /// <summary>
    /// reads incoming tcp-packages to create and queue incoming commands*
    /// </summary>
    internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
    {
      if (inbuff == null)
      {
        if (this.debugOut < DebugLevel.ERROR)
          return;
        this.EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
      }
      else
      {
        this.timestampOfLastReceive = SupportClass.GetTickCount();
        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
        this.timeLastSendOutgoing = this.timeInt;
        this.bytesIn += (long) (inbuff.Length + 7);
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsIncoming.TotalPacketCount;
          ++this.TrafficStatsIncoming.TotalCommandsInPackets;
        }
        if ((int) inbuff[0] == 243 || (int) inbuff[0] == 244)
        {
          lock (this.incomingList)
          {
            this.incomingList.Enqueue(inbuff);
            if (this.incomingList.Count % this.warningSize != 0)
              return;
            this.EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
          }
        }
        else if ((int) inbuff[0] == 240)
        {
          this.TrafficStatsIncoming.CountControlCommand(inbuff.Length);
          this.ReadPingResult(inbuff);
        }
        else
        {
          if (this.debugOut < DebugLevel.ERROR)
            return;
          this.EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + (object) inbuff[0]);
        }
      }
    }

    private void ReadPingResult(byte[] inbuff)
    {
      int num1 = 0;
      int num2 = 0;
      int offset = 1;
      Protocol.Deserialize(out num1, inbuff, ref offset);
      Protocol.Deserialize(out num2, inbuff, ref offset);
      this.lastRoundTripTime = SupportClass.GetTickCount() - num2;
      if (!this.serverTimeOffsetIsAvailable)
        this.roundTripTime = this.lastRoundTripTime;
      this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
      if (this.serverTimeOffsetIsAvailable)
        return;
      this.serverTimeOffset = num1 + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
      this.serverTimeOffsetIsAvailable = true;
    }

    protected internal void ReadPingResult(OperationResponse operationResponse)
    {
      int num = (int) operationResponse.Parameters[(byte) 2];
      this.lastRoundTripTime = SupportClass.GetTickCount() - (int) operationResponse.Parameters[(byte) 1];
      if (!this.serverTimeOffsetIsAvailable)
        this.roundTripTime = this.lastRoundTripTime;
      this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
      if (this.serverTimeOffsetIsAvailable)
        return;
      this.serverTimeOffset = num + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
      this.serverTimeOffsetIsAvailable = true;
    }
  }
}
