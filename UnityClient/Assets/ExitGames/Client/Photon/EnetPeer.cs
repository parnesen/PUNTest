// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.EnetPeer
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System;
using System.Collections.Generic;
using System.Text;

namespace ExitGames.Client.Photon
{
  internal class EnetPeer : PeerBase
  {
    internal static readonly byte[] udpHeader0xF3 = new byte[2]
    {
      (byte) 243,
      (byte) 2
    };
    internal static readonly byte[] messageHeader = EnetPeer.udpHeader0xF3;
    /// <summary>
    /// Will contain channel 0xFF and any other.
    /// </summary>
    private Dictionary<byte, EnetChannel> channels = new Dictionary<byte, EnetChannel>();
    /// <summary>
    /// One list for all channels keeps sent commands (for re-sending).
    /// </summary>
    private List<NCommand> sentReliableCommands = new List<NCommand>();
    /// <summary>
    /// One list for all channels keeps acknowledgements.
    /// </summary>
    private Queue<NCommand> outgoingAcknowledgementsList = new Queue<NCommand>();
    internal readonly int windowSize = 128;
    private byte[] initData = (byte[]) null;
    private Queue<int> commandsToRemove = new Queue<int>();
    private const int CRC_LENGTH = 4;
    private byte udpCommandCount;
    private byte[] udpBuffer;
    private int udpBufferIndex;
    internal int challenge;
    internal int reliableCommandsRepeated;
    internal int reliableCommandsSent;
    internal int serverSentTime;
    private EnetChannel[] channelArray;

    internal override int QueuedIncomingCommandsCount
    {
      get
      {
        int num = 0;
        lock (this.channels)
        {
          foreach (EnetChannel item_0 in this.channels.Values)
          {
            num += item_0.incomingReliableCommandsList.Count;
            num += item_0.incomingUnreliableCommandsList.Count;
          }
        }
        return num;
      }
    }

    internal override int QueuedOutgoingCommandsCount
    {
      get
      {
        int num = 0;
        lock (this.channels)
        {
          foreach (EnetChannel item_0 in this.channels.Values)
          {
            num += item_0.outgoingReliableCommandsList.Count;
            num += item_0.outgoingUnreliableCommandsList.Count;
          }
        }
        return num;
      }
    }

    internal EnetPeer()
    {
      ++PeerBase.peerCount;
      this.InitOnce();
      this.TrafficPackageHeaderSize = 12;
    }

    internal EnetPeer(IPhotonPeerListener listener)
      : this()
    {
      this.Listener = listener;
    }

    internal override void InitPeerBase()
    {
      base.InitPeerBase();
      this.peerID = (short) -1;
      this.challenge = SupportClass.ThreadSafeRandom.Next();
      this.udpBuffer = new byte[this.mtu];
      this.reliableCommandsSent = 0;
      this.reliableCommandsRepeated = 0;
      lock (this.channels)
        this.channels = new Dictionary<byte, EnetChannel>();
      lock (this.channels)
      {
        this.channels[byte.MaxValue] = new EnetChannel(byte.MaxValue, this.commandBufferSize);
        for (byte local_0 = (byte) 0; (int) local_0 < (int) this.ChannelCount; ++local_0)
          this.channels[local_0] = new EnetChannel(local_0, this.commandBufferSize);
        this.channelArray = new EnetChannel[(int) this.ChannelCount + 1];
        int local_1 = 0;
        foreach (EnetChannel item_0 in this.channels.Values)
          this.channelArray[local_1++] = item_0;
      }
      lock (this.sentReliableCommands)
        this.sentReliableCommands = new List<NCommand>(this.commandBufferSize);
      lock (this.outgoingAcknowledgementsList)
        this.outgoingAcknowledgementsList = new Queue<NCommand>(this.commandBufferSize);
      this.CommandLogInit();
    }

    internal override bool Connect(string ipport, string appID)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
      {
        this.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting. peerConnectionState: " + (object) this.peerConnectionState);
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
      this.ServerAddress = ipport;
      this.InitPeerBase();
      if (appID == null)
        appID = "LoadBalancing";
      for (int index = 0; index < 32; ++index)
        this.INIT_BYTES[index + 9] = index < appID.Length ? (byte) appID[index] : (byte) 0;
      this.initData = this.INIT_BYTES;
      this.rt = (IPhotonSocket) new SocketUdp((PeerBase) this);
      if (this.rt == null)
      {
        this.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed, because SocketImplementation or socket was null. Set PhotonPeer.SocketImplementation before Connect().");
        return false;
      }
      if (!this.rt.Connect())
        return false;
      if (this.TrafficStatsEnabled)
      {
        this.TrafficStatsOutgoing.ControlCommandBytes += 44;
        ++this.TrafficStatsOutgoing.ControlCommandCount;
      }
      this.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
      this.QueueOutgoingReliableCommand(new NCommand(this, (byte) 2, (byte[]) null, byte.MaxValue));
      return true;
    }

    internal override void Disconnect()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
        return;
      if (this.outgoingAcknowledgementsList != null)
      {
        lock (this.outgoingAcknowledgementsList)
          this.outgoingAcknowledgementsList.Clear();
      }
      if (this.sentReliableCommands != null)
      {
        lock (this.sentReliableCommands)
          this.sentReliableCommands.Clear();
      }
      lock (this.channels)
      {
        foreach (EnetChannel item_0 in this.channels.Values)
          item_0.clearAll();
      }
      bool simulationEnabled = this.NetworkSimulationSettings.IsSimulationEnabled;
      this.NetworkSimulationSettings.IsSimulationEnabled = false;
      NCommand command = new NCommand(this, (byte) 4, (byte[]) null, byte.MaxValue);
      this.QueueOutgoingReliableCommand(command);
      this.SendOutgoingCommands();
      if (this.TrafficStatsEnabled)
        this.TrafficStatsOutgoing.CountControlCommand(command.Size);
      this.rt.Disconnect();
      this.NetworkSimulationSettings.IsSimulationEnabled = simulationEnabled;
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void StopConnection()
    {
      if (this.rt != null)
        this.rt.Disconnect();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      if (this.Listener == null)
        return;
      this.Listener.OnStatusChanged(StatusCode.Disconnect);
    }

    internal override void FetchServerTimestamp()
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut < DebugLevel.INFO)
          return;
        this.EnqueueDebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + (object) this.peerConnectionState);
      }
      else
        this.CreateAndEnqueueCommand((byte) 12, new byte[0], byte.MaxValue);
    }

    /// <summary>
    /// Checks the incoming queue and Dispatches received data if possible.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// If a Dispatch happened or not, which shows if more Dispatches might be needed.
    /// </returns>
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
      NCommand ncommand = (NCommand) null;
      lock (this.channels)
      {
        for (int local_2 = 0; local_2 < this.channelArray.Length; ++local_2)
        {
          EnetChannel local_3 = this.channelArray[local_2];
          if (local_3.incomingUnreliableCommandsList.Count > 0)
          {
            int local_4 = int.MaxValue;
            foreach (int item_0 in local_3.incomingUnreliableCommandsList.Keys)
            {
              NCommand local_6 = local_3.incomingUnreliableCommandsList[item_0];
              if (item_0 < local_3.incomingUnreliableSequenceNumber || local_6.reliableSequenceNumber < local_3.incomingReliableSequenceNumber)
                this.commandsToRemove.Enqueue(item_0);
              else if (this.limitOfUnreliableCommands > 0 && local_3.incomingUnreliableCommandsList.Count > this.limitOfUnreliableCommands)
                this.commandsToRemove.Enqueue(item_0);
              else if (item_0 < local_4 && local_6.reliableSequenceNumber <= local_3.incomingReliableSequenceNumber)
                local_4 = item_0;
            }
            while (this.commandsToRemove.Count > 0)
              local_3.incomingUnreliableCommandsList.Remove(this.commandsToRemove.Dequeue());
            if (local_4 < int.MaxValue)
              ncommand = local_3.incomingUnreliableCommandsList[local_4];
            if (ncommand != null)
            {
              local_3.incomingUnreliableCommandsList.Remove(ncommand.unreliableSequenceNumber);
              local_3.incomingUnreliableSequenceNumber = ncommand.unreliableSequenceNumber;
              break;
            }
          }
          if (ncommand == null && local_3.incomingReliableCommandsList.Count > 0)
          {
            local_3.incomingReliableCommandsList.TryGetValue(local_3.incomingReliableSequenceNumber + 1, out ncommand);
            if (ncommand != null)
            {
              if ((int) ncommand.commandType != 8)
              {
                local_3.incomingReliableSequenceNumber = ncommand.reliableSequenceNumber;
                local_3.incomingReliableCommandsList.Remove(ncommand.reliableSequenceNumber);
                break;
              }
              if (ncommand.fragmentsRemaining > 0)
              {
                ncommand = (NCommand) null;
                break;
              }
              byte[] local_8 = new byte[ncommand.totalLength];
              for (int local_9 = ncommand.startSequenceNumber; local_9 < ncommand.startSequenceNumber + ncommand.fragmentCount; ++local_9)
              {
                if (!local_3.ContainsReliableSequenceNumber(local_9))
                  throw new Exception("command.fragmentsRemaining was 0, but not all fragments are found to be combined!");
                NCommand local_7 = local_3.FetchReliableSequenceNumber(local_9);
                Buffer.BlockCopy((Array) local_7.Payload, 0, (Array) local_8, local_7.fragmentOffset, local_7.Payload.Length);
                local_3.incomingReliableCommandsList.Remove(local_7.reliableSequenceNumber);
              }
              if (this.debugOut >= DebugLevel.ALL)
                this.Listener.DebugReturn(DebugLevel.ALL, "assembled fragmented payload from " + (object) ncommand.fragmentCount + " parts. Dispatching now.");
              ncommand.Payload = local_8;
              ncommand.Size = 12 * ncommand.fragmentCount + ncommand.totalLength;
              local_3.incomingReliableSequenceNumber = ncommand.reliableSequenceNumber + ncommand.fragmentCount - 1;
              break;
            }
          }
        }
      }
      if (ncommand != null && ncommand.Payload != null)
      {
        this.ByteCountCurrentDispatch = ncommand.Size;
        this.CommandInCurrentDispatch = ncommand;
        if (this.DeserializeMessageAndCallback(ncommand.Payload))
        {
          this.CommandInCurrentDispatch = (NCommand) null;
          return true;
        }
        this.CommandInCurrentDispatch = (NCommand) null;
      }
      return false;
    }

    /// <summary>
    /// gathers acks until udp-packet is full and sends it!
    /// 
    /// </summary>
    internal override bool SendAcksOnly()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || (this.rt == null || !this.rt.Connected))
        return false;
      lock (this.udpBuffer)
      {
        int local_0 = 0;
        this.udpBufferIndex = 12;
        if (this.crcEnabled)
          this.udpBufferIndex += 4;
        this.udpCommandCount = (byte) 0;
        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
        lock (this.outgoingAcknowledgementsList)
        {
          if (this.outgoingAcknowledgementsList.Count > 0)
          {
            local_0 = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
            this.timeLastSendAck = this.timeInt;
          }
        }
        if (this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0)
        {
          lock (this.sentReliableCommands)
          {
            foreach (NCommand item_0 in this.sentReliableCommands)
            {
              if (item_0 != null && item_0.roundTripTimeout != 0 && this.timeInt - item_0.commandSentTime > item_0.roundTripTimeout)
              {
                item_0.commandSentCount = (byte) 1;
                item_0.roundTripTimeout = 0;
                item_0.timeoutTime = int.MaxValue;
                item_0.commandSentTime = this.timeInt;
              }
            }
          }
        }
        if ((int) this.udpCommandCount <= 0)
          return false;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += (int) this.udpCommandCount;
        }
        this.SendData(this.udpBuffer, this.udpBufferIndex);
        return local_0 > 0;
      }
    }

    /// <summary>
    /// gathers commands from all (out)queues until udp-packet is full and sends it!
    /// 
    /// </summary>
    internal override bool SendOutgoingCommands()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || !this.rt.Connected)
        return false;
      lock (this.udpBuffer)
      {
        int local_0 = 0;
        this.udpBufferIndex = 12;
        if (this.crcEnabled)
          this.udpBufferIndex += 4;
        this.udpCommandCount = (byte) 0;
        this.timeInt = SupportClass.GetTickCount() - this.timeBase;
        this.timeLastSendOutgoing = this.timeInt;
        lock (this.outgoingAcknowledgementsList)
        {
          if (this.outgoingAcknowledgementsList.Count > 0)
          {
            local_0 = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
            this.timeLastSendAck = this.timeInt;
          }
        }
        if (!this.IsSendingOnlyAcks && this.timeInt > this.timeoutInt && this.sentReliableCommands.Count > 0)
        {
          lock (this.sentReliableCommands)
          {
            Queue<NCommand> local_1 = new Queue<NCommand>();
            foreach (NCommand item_0 in this.sentReliableCommands)
            {
              if (item_0 != null && this.timeInt - item_0.commandSentTime > item_0.roundTripTimeout)
              {
                if ((int) item_0.commandSentCount > this.sentCountAllowance || this.timeInt > item_0.timeoutTime)
                {
                  if (this.debugOut >= DebugLevel.WARNING)
                    this.Listener.DebugReturn(DebugLevel.WARNING, "Timeout-disconnect! Command: " + (object) item_0 + " now: " + (string) (object) this.timeInt + " challenge: " + Convert.ToString(this.challenge, 16));
                  if (this.CommandLog != null)
                  {
                    this.CommandLog.Enqueue((CmdLogItem) new CmdLogSentReliable(item_0, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, true));
                    this.CommandLogResize();
                  }
                  this.peerConnectionState = PeerBase.ConnectionStateValue.Zombie;
                  this.Listener.OnStatusChanged(StatusCode.TimeoutDisconnect);
                  this.Disconnect();
                  return false;
                }
                local_1.Enqueue(item_0);
              }
            }
            while (local_1.Count > 0)
            {
              NCommand local_2_1 = local_1.Dequeue();
              this.QueueOutgoingReliableCommand(local_2_1);
              this.sentReliableCommands.Remove(local_2_1);
              ++this.reliableCommandsRepeated;
              if (this.debugOut >= DebugLevel.INFO)
                this.Listener.DebugReturn(DebugLevel.INFO, string.Format("Resending: {0}. times out after: {1} sent: {3} now: {2} rtt/var: {4}/{5} last recv: {6}", (object) local_2_1, (object) local_2_1.roundTripTimeout, (object) this.timeInt, (object) local_2_1.commandSentTime, (object) this.roundTripTime, (object) this.roundTripTimeVariance, (object) (SupportClass.GetTickCount() - this.timestampOfLastReceive)));
            }
          }
        }
        if (!this.IsSendingOnlyAcks && this.peerConnectionState == PeerBase.ConnectionStateValue.Connected && (this.timePingInterval > 0 && this.sentReliableCommands.Count == 0) && (this.timeInt - this.timeLastAckReceive > this.timePingInterval && !this.AreReliableCommandsInTransit()) && this.udpBufferIndex + 12 < this.udpBuffer.Length)
        {
          NCommand local_2_2 = new NCommand(this, (byte) 5, (byte[]) null, byte.MaxValue);
          this.QueueOutgoingReliableCommand(local_2_2);
          if (this.TrafficStatsEnabled)
            this.TrafficStatsOutgoing.CountControlCommand(local_2_2.Size);
        }
        if (!this.IsSendingOnlyAcks)
        {
          lock (this.channels)
          {
            for (int local_3 = 0; local_3 < this.channelArray.Length; ++local_3)
            {
              EnetChannel local_4 = this.channelArray[local_3];
              local_0 += this.SerializeToBuffer(local_4.outgoingReliableCommandsList);
              local_0 += this.SerializeToBuffer(local_4.outgoingUnreliableCommandsList);
            }
          }
        }
        if ((int) this.udpCommandCount <= 0)
          return false;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsOutgoing.TotalPacketCount;
          this.TrafficStatsOutgoing.TotalCommandsInPackets += (int) this.udpCommandCount;
        }
        this.SendData(this.udpBuffer, this.udpBufferIndex);
        return local_0 > 0;
      }
    }

    /// <summary>
    /// Checks if any channel has a outgoing reliable command.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// True if any channel has a outgoing reliable command. False otherwise.
    /// </returns>
    private bool AreReliableCommandsInTransit()
    {
      lock (this.channels)
      {
        foreach (EnetChannel item_0 in this.channels.Values)
        {
          if (item_0.outgoingReliableCommandsList.Count > 0)
            return true;
        }
      }
      return false;
    }

    /// <summary>
    /// Checks Connected state and channel before operation is serialized and enqueued for sending.
    /// 
    /// </summary>
    /// <param name="parameters">operation parameters</param><param name="opCode">code of operation</param><param name="sendReliable">send as reliable command</param><param name="channelId">channel (sequence) for command</param><param name="encrypt">encrypt or not</param><param name="messageType">usually EgMessageType.Operation</param>
    /// <returns>
    /// if operation could be enqueued
    /// </returns>
    internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
    {
      if (this.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, string.Concat(new object[4]
          {
            (object) "Cannot send op: ",
            (object) opCode,
            (object) " Not connected. PeerState: ",
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
      byte[] payload = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
      return this.CreateAndEnqueueCommand(sendReliable ? (byte) 6 : (byte) 7, payload, channelId);
    }

    /// <summary>
    /// reliable-udp-level function to send some byte[] to the server via un/reliable command
    /// </summary>
    /// 
    /// <remarks>
    /// only called when a custom operation should be send
    /// </remarks>
    /// <param name="commandType">(enet) command type</param><param name="payload">data to carry (operation)</param><param name="channelNumber">channel in which to send</param>
    /// <returns>
    /// the invocation ID for this operation (the payload)
    /// </returns>
    internal bool CreateAndEnqueueCommand(byte commandType, byte[] payload, byte channelNumber)
    {
      if (payload == null)
        return false;
      EnetChannel enetChannel = this.channels[channelNumber];
      this.ByteCountLastOperation = 0;
      int count = this.mtu - 12 - 36;
      if (payload.Length > count)
      {
        int num1 = (payload.Length + count - 1) / count;
        int num2 = enetChannel.outgoingReliableSequenceNumber + 1;
        int num3 = 0;
        int srcOffset = 0;
        while (srcOffset < payload.Length)
        {
          if (payload.Length - srcOffset < count)
            count = payload.Length - srcOffset;
          byte[] payload1 = new byte[count];
          Buffer.BlockCopy((Array) payload, srcOffset, (Array) payload1, 0, count);
          NCommand command = new NCommand(this, (byte) 8, payload1, enetChannel.ChannelNumber);
          command.fragmentNumber = num3;
          command.startSequenceNumber = num2;
          command.fragmentCount = num1;
          command.totalLength = payload.Length;
          command.fragmentOffset = srcOffset;
          this.QueueOutgoingReliableCommand(command);
          this.ByteCountLastOperation += command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountFragmentOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
          ++num3;
          srcOffset += count;
        }
      }
      else
      {
        NCommand command = new NCommand(this, commandType, payload, enetChannel.ChannelNumber);
        if ((int) command.commandFlags == 1)
        {
          this.QueueOutgoingReliableCommand(command);
          this.ByteCountLastOperation = command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountReliableOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
        }
        else
        {
          this.QueueOutgoingUnreliableCommand(command);
          this.ByteCountLastOperation = command.Size;
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsOutgoing.CountUnreliableOpCommand(command.Size);
            this.TrafficStatsGameLevel.CountOperation(command.Size);
          }
        }
      }
      return true;
    }

    /// <summary>
    /// Returns the UDP Payload starting with Magic Number for binary protocol
    /// </summary>
    internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
    {
      byte[] numArray;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
        Protocol.SerializeOperationRequest(this.SerializeMemStream, opc, parameters, false);
        if (encrypt)
        {
          byte[] local_1_1 = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(EnetPeer.messageHeader, 0, EnetPeer.messageHeader.Length);
          this.SerializeMemStream.Write(local_1_1, 0, local_1_1.Length);
        }
        numArray = this.SerializeMemStream.ToArray();
      }
      if (messageType != PeerBase.EgMessageType.Operation)
        numArray[EnetPeer.messageHeader.Length - 1] = (byte) messageType;
      if (encrypt)
        numArray[EnetPeer.messageHeader.Length - 1] = (byte) ((uint) numArray[EnetPeer.messageHeader.Length - 1] | 128U);
      return numArray;
    }

    internal int SerializeToBuffer(Queue<NCommand> commandList)
    {
      while (commandList.Count > 0)
      {
        NCommand command = commandList.Peek();
        if (command == null)
        {
          commandList.Dequeue();
        }
        else
        {
          if (this.udpBufferIndex + command.Size > this.udpBuffer.Length)
          {
            if (this.debugOut >= DebugLevel.INFO)
            {
              this.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[4]
              {
                (object) "UDP package is full. Commands in Package: ",
                (object) this.udpCommandCount,
                (object) ". Commands left in queue: ",
                (object) commandList.Count
              }));
              break;
            }
            break;
          }
          Buffer.BlockCopy((Array) command.Serialize(), 0, (Array) this.udpBuffer, this.udpBufferIndex, command.Size);
          this.udpBufferIndex += command.Size;
          ++this.udpCommandCount;
          if (((int) command.commandFlags & 1) > 0)
          {
            this.QueueSentCommand(command);
            if (this.CommandLog != null)
            {
              this.CommandLog.Enqueue((CmdLogItem) new CmdLogSentReliable(command, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, false));
              this.CommandLogResize();
            }
          }
          commandList.Dequeue();
        }
      }
      return commandList.Count;
    }

    internal void SendData(byte[] data, int length)
    {
      try
      {
        int targetOffset1 = 0;
        Protocol.Serialize(this.peerID, data, ref targetOffset1);
        data[2] = this.crcEnabled ? (byte) 204 : (byte) 0;
        data[3] = this.udpCommandCount;
        int targetOffset2 = 4;
        Protocol.Serialize(this.timeInt, data, ref targetOffset2);
        Protocol.Serialize(this.challenge, data, ref targetOffset2);
        if (this.crcEnabled)
        {
          Protocol.Serialize(0, data, ref targetOffset2);
          uint num = SupportClass.CalculateCrc(data, length);
          targetOffset2 -= 4;
          Protocol.Serialize((int) num, data, ref targetOffset2);
        }
        this.bytesOut += (long) length;
        if (this.NetworkSimulationSettings.IsSimulationEnabled)
        {
          byte[] dataCopy = new byte[length];
          Buffer.BlockCopy((Array) data, 0, (Array) dataCopy, 0, length);
          int num;
          this.SendNetworkSimulated((PeerBase.MyAction) (() => num = (int) this.rt.Send(dataCopy, length)));
        }
        else
        {
          int num1 = (int) this.rt.Send(data, length);
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
        SupportClass.WriteStackTrace(ex);
      }
    }

    internal void QueueSentCommand(NCommand command)
    {
      command.commandSentTime = this.timeInt;
      ++command.commandSentCount;
      if (command.roundTripTimeout == 0)
      {
        command.roundTripTimeout = this.roundTripTime + 4 * this.roundTripTimeVariance;
        command.timeoutTime = this.timeInt + this.DisconnectTimeout;
      }
      else if ((int) command.commandSentCount > (int) this.QuickResendAttempts + 1)
        command.roundTripTimeout *= 2;
      lock (this.sentReliableCommands)
      {
        if (this.sentReliableCommands.Count == 0)
        {
          int local_0 = command.commandSentTime + command.roundTripTimeout;
          if (local_0 < this.timeoutInt)
            this.timeoutInt = local_0;
        }
        ++this.reliableCommandsSent;
        this.sentReliableCommands.Add(command);
      }
      if (this.sentReliableCommands.Count < this.warningSize || this.sentReliableCommands.Count % this.warningSize != 0)
        return;
      this.Listener.OnStatusChanged(StatusCode.QueueSentWarning);
    }

    internal void QueueOutgoingReliableCommand(NCommand command)
    {
      EnetChannel enetChannel = this.channels[command.commandChannelID];
      lock (enetChannel)
      {
        Queue<NCommand> local_1 = enetChannel.outgoingReliableCommandsList;
        if (local_1.Count >= this.warningSize && local_1.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
        if (command.reliableSequenceNumber == 0)
          command.reliableSequenceNumber = ++enetChannel.outgoingReliableSequenceNumber;
        local_1.Enqueue(command);
      }
    }

    internal void QueueOutgoingUnreliableCommand(NCommand command)
    {
      Queue<NCommand> queue = this.channels[command.commandChannelID].outgoingUnreliableCommandsList;
      if (queue.Count >= this.warningSize && queue.Count % this.warningSize == 0)
        this.Listener.OnStatusChanged(StatusCode.QueueOutgoingUnreliableWarning);
      EnetChannel enetChannel = this.channels[command.commandChannelID];
      command.reliableSequenceNumber = enetChannel.outgoingReliableSequenceNumber;
      command.unreliableSequenceNumber = ++enetChannel.outgoingUnreliableSequenceNumber;
      queue.Enqueue(command);
    }

    internal void QueueOutgoingAcknowledgement(NCommand command)
    {
      lock (this.outgoingAcknowledgementsList)
      {
        if (this.outgoingAcknowledgementsList.Count >= this.warningSize && this.outgoingAcknowledgementsList.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueOutgoingAcksWarning);
        this.outgoingAcknowledgementsList.Enqueue(command);
      }
    }

    /// <summary>
    /// reads incoming udp-packages to create and queue incoming commands*
    /// </summary>
    internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLength)
    {
      this.timestampOfLastReceive = SupportClass.GetTickCount();
      try
      {
        int offset = 0;
        short num1;
        Protocol.Deserialize(out num1, inBuff, ref offset);
        byte[] numArray1 = inBuff;
        int index1 = offset;
        int num2 = 1;
        int num3 = index1 + num2;
        byte num4 = numArray1[index1];
        byte[] numArray2 = inBuff;
        int index2 = num3;
        int num5 = 1;
        int num6 = index2 + num5;
        byte num7 = numArray2[index2];
        Protocol.Deserialize(out this.serverSentTime, inBuff, ref num6);
        int num8;
        Protocol.Deserialize(out num8, inBuff, ref num6);
        if ((int) num4 == 204)
        {
          int num9;
          Protocol.Deserialize(out num9, inBuff, ref num6);
          this.bytesIn += 4L;
          num6 -= 4;
          Protocol.Serialize(0, inBuff, ref num6);
          uint num10 = SupportClass.CalculateCrc(inBuff, dataLength);
          if (num9 != (int) num10)
          {
            ++this.packetLossByCrc;
            if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.debugOut < DebugLevel.INFO)
              return;
            this.EnqueueDebugReturn(DebugLevel.INFO, string.Format("Ignored package due to wrong CRC. Incoming:  {0:X} Local: {1:X}", (object) (uint) num9, (object) num10));
            return;
          }
        }
        this.bytesIn += 12L;
        if (this.TrafficStatsEnabled)
        {
          ++this.TrafficStatsIncoming.TotalPacketCount;
          this.TrafficStatsIncoming.TotalCommandsInPackets += (int) num7;
        }
        if ((int) num7 > this.commandBufferSize || (int) num7 <= 0)
          this.EnqueueDebugReturn(DebugLevel.ERROR, string.Concat(new object[4]
          {
            (object) "too many/few incoming commands in package: ",
            (object) num7,
            (object) " > ",
            (object) this.commandBufferSize
          }));
        if (num8 != this.challenge)
        {
          ++this.packetLossByChallenge;
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected || this.debugOut < DebugLevel.ALL)
            return;
          this.EnqueueDebugReturn(DebugLevel.ALL, "Info: Ignoring received package due to wrong challenge. Challenge in-package!=local:" + (object) num8 + "!=" + (string) (object) this.challenge + " Commands in it: " + (string) (object) num7);
        }
        else
        {
          this.timeInt = SupportClass.GetTickCount() - this.timeBase;
          for (int index3 = 0; index3 < (int) num7; ++index3)
          {
            NCommand readCommand = new NCommand(this, inBuff, ref num6);
            if ((int) readCommand.commandType != 1)
            {
              this.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.ExecuteCommand(readCommand)));
            }
            else
            {
              this.TrafficStatsIncoming.TimestampOfLastAck = SupportClass.GetTickCount();
              this.ExecuteCommand(readCommand);
            }
            if (((int) readCommand.commandFlags & 1) > 0)
            {
              if (this.InReliableLog != null)
              {
                this.InReliableLog.Enqueue((CmdLogItem) new CmdLogReceivedReliable(readCommand, this.timeInt, this.roundTripTime, this.roundTripTimeVariance, this.timeInt - this.timeLastSendOutgoing, this.timeInt - this.timeLastSendAck));
                this.CommandLogResize();
              }
              NCommand ack = NCommand.CreateAck(this, readCommand, this.serverSentTime);
              this.QueueOutgoingAcknowledgement(ack);
              if (this.TrafficStatsEnabled)
              {
                this.TrafficStatsIncoming.TimestampOfLastReliableCommand = SupportClass.GetTickCount();
                this.TrafficStatsOutgoing.CountControlCommand(ack.Size);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.EnqueueDebugReturn(DebugLevel.ERROR, string.Format("Exception while reading commands from incoming data: {0}", (object) ex));
        SupportClass.WriteStackTrace(ex);
      }
    }

    internal bool ExecuteCommand(NCommand command)
    {
      bool flag = true;
      switch (command.commandType)
      {
        case (byte) 1:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          this.timeLastAckReceive = this.timeInt;
          this.lastRoundTripTime = this.timeInt - command.ackReceivedSentTime;
          NCommand ncommand = this.RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, (int) command.commandChannelID);
          if (this.CommandLog != null)
          {
            this.CommandLog.Enqueue((CmdLogItem) new CmdLogReceivedAck(command, this.timeInt, this.roundTripTime, this.roundTripTimeVariance));
            this.CommandLogResize();
          }
          if (ncommand != null)
          {
            if ((int) ncommand.commandType == 12)
            {
              if (this.lastRoundTripTime <= this.roundTripTime)
              {
                this.serverTimeOffset = this.serverSentTime + (this.lastRoundTripTime >> 1) - SupportClass.GetTickCount();
                this.serverTimeOffsetIsAvailable = true;
              }
              else
                this.FetchServerTimestamp();
            }
            else
            {
              this.UpdateRoundTripTimeAndVariance(this.lastRoundTripTime);
              if ((int) ncommand.commandType == 4 && this.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting)
              {
                if (this.debugOut >= DebugLevel.INFO)
                  this.EnqueueDebugReturn(DebugLevel.INFO, "Received disconnect ACK by server");
                this.EnqueueActionForDispatch((PeerBase.MyAction) (() => this.rt.Disconnect()));
              }
              else if ((int) ncommand.commandType == 2)
                this.roundTripTime = this.lastRoundTripTime;
            }
            break;
          }
          break;
        case (byte) 2:
        case (byte) 5:
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
            break;
          }
          break;
        case (byte) 3:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
          {
            command = new NCommand(this, (byte) 6, this.initData, (byte) 0);
            this.QueueOutgoingReliableCommand(command);
            if (this.TrafficStatsEnabled)
              this.TrafficStatsOutgoing.CountControlCommand(command.Size);
            this.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
            break;
          }
          break;
        case (byte) 4:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountControlCommand(command.Size);
          StatusCode statusCode = StatusCode.DisconnectByServer;
          if ((int) command.reservedByte == 1)
            statusCode = StatusCode.DisconnectByServerLogic;
          else if ((int) command.reservedByte == 3)
            statusCode = StatusCode.DisconnectByServerUserLimit;
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, "Server " + (object) this.ServerAddress + " sent disconnect. PeerId: " + (string) (object) (ushort) this.peerID + " RTT/Variance:" + (string) (object) this.roundTripTime + "/" + (string) (object) this.roundTripTimeVariance + " reason byte: " + (string) (object) command.reservedByte);
          this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
          this.Listener.OnStatusChanged(statusCode);
          this.rt.Disconnect();
          this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
          this.Listener.OnStatusChanged(StatusCode.Disconnect);
          break;
        case (byte) 6:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            flag = this.QueueIncomingCommand(command);
            break;
          }
          break;
        case (byte) 7:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            flag = this.QueueIncomingCommand(command);
            break;
          }
          break;
        case (byte) 8:
          if (this.TrafficStatsEnabled)
            this.TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
          if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
          {
            if (command.fragmentNumber > command.fragmentCount || command.fragmentOffset >= command.totalLength || command.fragmentOffset + command.Payload.Length > command.totalLength)
            {
              if (this.debugOut >= DebugLevel.ERROR)
              {
                this.Listener.DebugReturn(DebugLevel.ERROR, "Received fragment has bad size: " + (object) command);
                break;
              }
              break;
            }
            flag = this.QueueIncomingCommand(command);
            if (flag)
            {
              EnetChannel enetChannel = this.channels[command.commandChannelID];
              if (command.reliableSequenceNumber == command.startSequenceNumber)
              {
                --command.fragmentsRemaining;
                int num = command.startSequenceNumber + 1;
                while (command.fragmentsRemaining > 0 && num < command.startSequenceNumber + command.fragmentCount)
                {
                  if (enetChannel.ContainsReliableSequenceNumber(num++))
                    --command.fragmentsRemaining;
                }
              }
              else if (enetChannel.ContainsReliableSequenceNumber(command.startSequenceNumber))
                --enetChannel.FetchReliableSequenceNumber(command.startSequenceNumber).fragmentsRemaining;
            }
            break;
          }
          break;
      }
      return flag;
    }

    /// <summary>
    /// queues incoming commands in the correct order as either unreliable, reliable or unsequenced. return value determines if the command is queued / done.
    /// </summary>
    internal bool QueueIncomingCommand(NCommand command)
    {
      EnetChannel enetChannel;
      this.channels.TryGetValue(command.commandChannelID, out enetChannel);
      if (enetChannel == null)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Received command for non-existing channel: " + (object) command.commandChannelID);
        return false;
      }
      if (this.debugOut >= DebugLevel.ALL)
        this.Listener.DebugReturn(DebugLevel.ALL, "queueIncomingCommand() " + (object) command + " channel seq# r/u: " + (string) (object) enetChannel.incomingReliableSequenceNumber + "/" + (string) (object) enetChannel.incomingUnreliableSequenceNumber);
      if ((int) command.commandFlags == 1)
      {
        if (command.reliableSequenceNumber <= enetChannel.incomingReliableSequenceNumber)
        {
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[4]
            {
              (object) "incoming command ",
              (object) command,
              (object) " is old (not saving it). Dispatched incomingReliableSequenceNumber: ",
              (object) enetChannel.incomingReliableSequenceNumber
            }));
          return false;
        }
        if (enetChannel.ContainsReliableSequenceNumber(command.reliableSequenceNumber))
        {
          if (this.debugOut >= DebugLevel.INFO)
            this.Listener.DebugReturn(DebugLevel.INFO, "Info: command was received before! Old/New: " + (object) enetChannel.FetchReliableSequenceNumber(command.reliableSequenceNumber) + "/" + (string) (object) command + " inReliableSeq#: " + (string) (object) enetChannel.incomingReliableSequenceNumber);
          return false;
        }
        if (enetChannel.incomingReliableCommandsList.Count >= this.warningSize && enetChannel.incomingReliableCommandsList.Count % this.warningSize == 0)
          this.Listener.OnStatusChanged(StatusCode.QueueIncomingReliableWarning);
        enetChannel.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
        return true;
      }
      if ((int) command.commandFlags != 0)
        return false;
      if (command.reliableSequenceNumber < enetChannel.incomingReliableSequenceNumber)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
        return true;
      }
      if (command.unreliableSequenceNumber <= enetChannel.incomingUnreliableSequenceNumber)
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
        return true;
      }
      if (enetChannel.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber))
      {
        if (this.debugOut >= DebugLevel.INFO)
          this.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[4]
          {
            (object) "command was received before! Old/New: ",
            (object) enetChannel.incomingUnreliableCommandsList[command.unreliableSequenceNumber],
            (object) "/",
            (object) command
          }));
        return false;
      }
      if (enetChannel.incomingUnreliableCommandsList.Count >= this.warningSize && enetChannel.incomingUnreliableCommandsList.Count % this.warningSize == 0)
        this.Listener.OnStatusChanged(StatusCode.QueueIncomingUnreliableWarning);
      enetChannel.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
      return true;
    }

    /// <summary>
    /// removes commands which are acknowledged*
    /// </summary>
    internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel)
    {
      NCommand ncommand = (NCommand) null;
      lock (this.sentReliableCommands)
      {
        foreach (NCommand item_0 in this.sentReliableCommands)
        {
          if (item_0 != null && item_0.reliableSequenceNumber == ackReceivedReliableSequenceNumber && (int) item_0.commandChannelID == ackReceivedChannel)
          {
            ncommand = item_0;
            break;
          }
        }
        if (ncommand != null)
        {
          this.sentReliableCommands.Remove(ncommand);
          if (this.sentReliableCommands.Count > 0)
            this.timeoutInt = this.timeInt + 25;
        }
        else if (this.debugOut >= DebugLevel.ALL && this.peerConnectionState != PeerBase.ConnectionStateValue.Connected && this.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting)
          this.EnqueueDebugReturn(DebugLevel.ALL, string.Format("No sent command for ACK (Ch: {0} Sq#: {1}). PeerState: {2}.", (object) ackReceivedReliableSequenceNumber, (object) ackReceivedChannel, (object) this.peerConnectionState));
      }
      return ncommand;
    }

    internal string CommandListToString(NCommand[] list)
    {
      if (this.debugOut < DebugLevel.ALL)
        return string.Empty;
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < list.Length; ++index)
      {
        stringBuilder.Append((string) (object) index + (object) "=");
        stringBuilder.Append((object) list[index]);
        stringBuilder.Append(" # ");
      }
      return stringBuilder.ToString();
    }
  }
}
