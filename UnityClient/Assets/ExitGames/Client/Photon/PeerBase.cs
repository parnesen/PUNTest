// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.PeerBase
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using Photon.SocketServer.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace ExitGames.Client.Photon
{
  public abstract class PeerBase
  {
    internal static int outgoingStreamBufferSize = 1200;
    protected internal Type SocketImplementation = (Type) null;
    private bool trafficStatsEnabled = false;
    internal bool crcEnabled = false;
    internal DebugLevel debugOut = DebugLevel.ERROR;
    internal readonly Queue<PeerBase.MyAction> ActionQueue = new Queue<PeerBase.MyAction>();
    /// This ID is assigned by the Realtime Server upon Connection.
    ///             The application does not have to care about this, but it is useful in debugging.
    internal short peerID = (short) -1;
    internal int commandBufferSize = 100;
    internal int warningSize = 100;
    internal int sentCountAllowance = 5;
    internal int DisconnectTimeout = 10000;
    internal int timePingInterval = 1000;
    internal byte ChannelCount = (byte) 2;
    internal int limitOfUnreliableCommands = 0;
    private readonly Random lagRandomizer = new Random();
    internal readonly LinkedList<SimulationItem> NetSimListOutgoing = new LinkedList<SimulationItem>();
    internal readonly LinkedList<SimulationItem> NetSimListIncoming = new LinkedList<SimulationItem>();
    private readonly NetworkSimulationSet networkSimulationSettings = new NetworkSimulationSet();
    /// <summary>
    /// Size of CommandLog. Default is 0, no logging.
    /// </summary>
    internal int CommandLogSize = 0;
    internal byte[] INIT_BYTES = new byte[41];
    internal int outgoingCommandsInStream = 0;
    /// <summary>
    /// Maximum Transfer Unit to be used for UDP+TCP
    /// </summary>
    internal int mtu = 1200;
    /// <summary>
    /// (default=2) Rhttp: minimum number of open Connections
    /// </summary>
    internal int rhttpMinConnections = 2;
    /// <summary>
    /// (default=6) Rhttp: maximum number of open Connections, should be &gt; rhttpMinConnections
    /// </summary>
    internal int rhttpMaxConnections = 6;
    protected MemoryStream SerializeMemStream = new MemoryStream();
    public const string ClientVersion = "4.0.0.11";
    internal const int ENET_PEER_PACKET_LOSS_SCALE = 65536;
    internal const int ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 300;
    internal const int ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
    internal IPhotonSocket rt;
    public int ByteCountLastOperation;
    public int ByteCountCurrentDispatch;
    internal NCommand CommandInCurrentDispatch;
    internal int TrafficPackageHeaderSize;
    public TrafficStats TrafficStatsIncoming;
    public TrafficStats TrafficStatsOutgoing;
    public TrafficStatsGameLevel TrafficStatsGameLevel;
    private Stopwatch trafficStatsStopwatch;
    internal ConnectionProtocol usedProtocol;
    internal int packetLossByCrc;
    internal int packetLossByChallenge;
    /// <summary>
    /// This is the (low level) Connection state of the peer. It's internal and based on eNet's states.
    /// 
    /// </summary>
    /// 
    /// <remarks>
    /// Applications can read the "high level" state as PhotonPeer.PeerState, which uses a different enum.
    /// </remarks>
    internal PeerBase.ConnectionStateValue peerConnectionState;
    /// <summary>
    /// The serverTimeOffset is serverTimestamp - localTime. Used to approximate the serverTimestamp with help of localTime
    /// 
    /// </summary>
    internal int serverTimeOffset;
    internal bool serverTimeOffsetIsAvailable;
    internal int roundTripTime;
    internal int roundTripTimeVariance;
    internal int lastRoundTripTime;
    internal int lowestRoundTripTime;
    internal int lastRoundTripTimeVariance;
    internal int highestRoundTripTimeVariance;
    internal int timestampOfLastReceive;
    internal int packetThrottleInterval;
    internal static short peerCount;
    internal long bytesOut;
    internal long bytesIn;
    internal DiffieHellmanCryptoProvider CryptoProvider;
    /// <summary>
    /// Log of sent reliable commands and incoming ACKs.
    /// </summary>
    internal Queue<CmdLogItem> CommandLog;
    /// <summary>
    /// Log of incoming reliable commands, used to track which commands from the server this client got. Part of the PhotonPeer.CommandLogToString() result.
    /// </summary>
    internal Queue<CmdLogItem> InReliableLog;
    internal int timeBase;
    internal int timeInt;
    internal int timeoutInt;
    internal int timeLastAckReceive;
    internal int timeLastSendAck;
    /// <summary>
    /// Set to timeInt, whenever SendOutgoingCommands actually checks outgoing queues to send them. Must be Connected.
    /// </summary>
    internal int timeLastSendOutgoing;
    internal bool ApplicationIsInitialized;
    internal bool isEncryptionAvailable;

    public long TrafficStatsEnabledTime
    {
      get
      {
        return this.trafficStatsStopwatch != null ? this.trafficStatsStopwatch.ElapsedMilliseconds : 0L;
      }
    }

    /// <summary>
    /// Enables or disables collection of statistics.
    ///             Setting this to true, also starts the stopwatch to measure the timespan the stats are collected.
    /// 
    /// </summary>
    public bool TrafficStatsEnabled
    {
      get
      {
        return this.trafficStatsEnabled;
      }
      set
      {
        this.trafficStatsEnabled = value;
        if (value)
        {
          if (this.trafficStatsStopwatch == null)
            this.InitializeTrafficStats();
          this.trafficStatsStopwatch.Start();
        }
        else
        {
          if (this.trafficStatsStopwatch == null)
            return;
          this.trafficStatsStopwatch.Stop();
        }
      }
    }

    public string ServerAddress { get; internal set; }

    internal string HttpUrlParameters { get; set; }

    internal IPhotonPeerListener Listener { get; set; }

    public byte QuickResendAttempts { get; set; }

    /// <summary>
    /// Gets the currently used settings for the built-in network simulation.
    ///             Please check the description of NetworkSimulationSet for more details.
    /// 
    /// </summary>
    public NetworkSimulationSet NetworkSimulationSettings
    {
      get
      {
        return this.networkSimulationSettings;
      }
    }

    /// <summary>
    /// Count of all bytes going out (including headers)
    /// 
    /// </summary>
    internal long BytesOut
    {
      get
      {
        return this.bytesOut;
      }
    }

    /// <summary>
    /// Count of all bytes coming in (including headers)
    /// 
    /// </summary>
    internal long BytesIn
    {
      get
      {
        return this.bytesIn;
      }
    }

    internal abstract int QueuedIncomingCommandsCount { get; }

    internal abstract int QueuedOutgoingCommandsCount { get; }

    public virtual string PeerID
    {
      get
      {
        return ((ushort) this.peerID).ToString();
      }
    }

    protected internal byte[] TcpConnectionPrefix { get; set; }

    internal bool IsSendingOnlyAcks { get; set; }

    /// <summary>
    /// Reduce CommandLog to CommandLogSize. Oldest entries get discarded.
    /// </summary>
    internal void CommandLogResize()
    {
      if (this.CommandLogSize <= 0)
      {
        this.CommandLog = (Queue<CmdLogItem>) null;
        this.InReliableLog = (Queue<CmdLogItem>) null;
      }
      else
      {
        if (this.CommandLog == null || this.InReliableLog == null)
          this.CommandLogInit();
        while (this.CommandLog.Count > 0 && this.CommandLog.Count > this.CommandLogSize)
          this.CommandLog.Dequeue();
        while (this.InReliableLog.Count > 0 && this.InReliableLog.Count > this.CommandLogSize)
          this.InReliableLog.Dequeue();
      }
    }

    /// <summary>
    /// Initializes the CommandLog and InReliableLog according to CommandLogSize. A value of 0 will set both logs to 0.
    /// </summary>
    internal void CommandLogInit()
    {
      if (this.CommandLogSize <= 0)
      {
        this.CommandLog = (Queue<CmdLogItem>) null;
        this.InReliableLog = (Queue<CmdLogItem>) null;
      }
      else if (this.CommandLog == null || this.InReliableLog == null)
      {
        this.CommandLog = new Queue<CmdLogItem>(this.CommandLogSize);
        this.InReliableLog = new Queue<CmdLogItem>(this.CommandLogSize);
      }
      else
      {
        this.CommandLog.Clear();
        this.InReliableLog.Clear();
      }
    }

    /// <summary>
    /// Converts the CommandLog into a readable table-like string with summary.
    /// </summary>
    public string CommandLogToString()
    {
      StringBuilder stringBuilder = new StringBuilder();
      int num = this.usedProtocol != ConnectionProtocol.Udp ? 0 : ((EnetPeer) this).reliableCommandsRepeated;
      stringBuilder.AppendFormat("PeerId: {0} Now: {1} Server: {2} State: {3} Total Resends: {4} Received {5}ms ago.\n", (object) this.PeerID, (object) this.timeInt, (object) this.ServerAddress, (object) this.peerConnectionState, (object) num, (object) (SupportClass.GetTickCount() - this.timestampOfLastReceive));
      if (this.CommandLog == null)
        return stringBuilder.ToString();
      foreach (CmdLogItem cmdLogItem in this.CommandLog)
        stringBuilder.AppendLine(cmdLogItem.ToString());
      stringBuilder.AppendLine("Received Reliable Log: ");
      foreach (CmdLogItem cmdLogItem in this.InReliableLog)
        stringBuilder.AppendLine(cmdLogItem.ToString());
      return stringBuilder.ToString();
    }

    internal void InitOnce()
    {
      this.networkSimulationSettings.peerBase = this;
      this.INIT_BYTES[0] = (byte) 243;
      this.INIT_BYTES[1] = (byte) 0;
      this.INIT_BYTES[2] = (byte) 1;
      this.INIT_BYTES[3] = (byte) 6;
      this.INIT_BYTES[4] = (byte) 1;
      this.INIT_BYTES[5] = (byte) 4;
      this.INIT_BYTES[6] = (byte) 0;
      this.INIT_BYTES[7] = (byte) 0;
      this.INIT_BYTES[8] = (byte) 7;
    }

    /// <summary>
    /// Connect to server and send Init (which inlcudes the appId).
    /// </summary>
    internal abstract bool Connect(string serverAddress, string appID);

    private string GetHttpKeyValueString(Dictionary<string, string> dic)
    {
      StringBuilder stringBuilder = new StringBuilder();
      foreach (KeyValuePair<string, string> keyValuePair in dic)
        stringBuilder.Append(keyValuePair.Key).Append("=").Append(keyValuePair.Value).Append("&");
      return stringBuilder.ToString();
    }

    internal abstract void Disconnect();

    internal abstract void StopConnection();

    internal abstract void FetchServerTimestamp();

    internal bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted)
    {
      return this.EnqueueOperation(parameters, opCode, sendReliable, channelId, encrypted, PeerBase.EgMessageType.Operation);
    }

    internal abstract bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypted, PeerBase.EgMessageType messageType);

    /// <summary>
    /// Checks the incoming queue and Dispatches received data if possible.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// If a Dispatch happened or not, which shows if more Dispatches might be needed.
    /// </returns>
    internal abstract bool DispatchIncomingCommands();

    /// <summary>
    /// Checks outgoing queues for commands to send and puts them on their way.
    ///             This creates one package per go in UDP.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// If commands are not sent, cause they didn't fit into the package that's sent.
    /// </returns>
    internal abstract bool SendOutgoingCommands();

    internal virtual bool SendAcksOnly()
    {
      return false;
    }

    /// <summary>
    /// Returns the UDP Payload starting with Magic Number for binary protocol
    /// </summary>
    internal byte[] SerializeMessageToMessage(object message, bool encrypt, byte[] messageHeader, bool writeLength = true)
    {
      byte[] target;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
        Protocol.SerializeMessage(this.SerializeMemStream, message);
        if (encrypt)
        {
          byte[] local_1_1 = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
          this.SerializeMemStream.Write(local_1_1, 0, local_1_1.Length);
        }
        target = this.SerializeMemStream.ToArray();
      }
      target[messageHeader.Length - 1] = (byte) 8;
      if (encrypt)
        target[messageHeader.Length - 1] = (byte) ((uint) target[messageHeader.Length - 1] | 128U);
      if (writeLength)
      {
        int targetOffset = 1;
        Protocol.Serialize(target.Length, target, ref targetOffset);
      }
      return target;
    }

    /// <summary>
    /// Returns the UDP Payload starting with Magic Number for binary protocol
    /// </summary>
    internal byte[] SerializeRawMessageToMessage(byte[] data, bool encrypt, byte[] messageHeader, bool writeLength = true)
    {
      byte[] target;
      lock (this.SerializeMemStream)
      {
        this.SerializeMemStream.Position = 0L;
        this.SerializeMemStream.SetLength(0L);
        if (!encrypt)
          this.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
        this.SerializeMemStream.Write(data, 0, data.Length);
        if (encrypt)
        {
          byte[] local_1_1 = this.CryptoProvider.Encrypt(this.SerializeMemStream.ToArray());
          this.SerializeMemStream.Position = 0L;
          this.SerializeMemStream.SetLength(0L);
          this.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
          this.SerializeMemStream.Write(local_1_1, 0, local_1_1.Length);
        }
        target = this.SerializeMemStream.ToArray();
      }
      target[messageHeader.Length - 1] = (byte) 9;
      if (encrypt)
        target[messageHeader.Length - 1] = (byte) ((uint) target[messageHeader.Length - 1] | 128U);
      if (writeLength)
      {
        int targetOffset = 1;
        Protocol.Serialize(target.Length, target, ref targetOffset);
      }
      return target;
    }

    internal abstract byte[] SerializeOperationToMessage(byte opCode, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt);

    internal abstract void ReceiveIncomingCommands(byte[] inBuff, int dataLength);

    internal void InitCallback()
    {
      if (this.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
        this.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
      this.ApplicationIsInitialized = true;
      this.FetchServerTimestamp();
      this.Listener.OnStatusChanged(StatusCode.Connect);
    }

    /// <summary>
    /// Internally uses an operation to exchange encryption keys with the server.
    /// 
    /// </summary>
    /// 
    /// <returns>
    /// If the op could be sent.
    /// </returns>
    internal bool ExchangeKeysForEncryption()
    {
      this.isEncryptionAvailable = false;
      this.CryptoProvider = new DiffieHellmanCryptoProvider();
      Dictionary<byte, object> parameters = new Dictionary<byte, object>(1);
      parameters[PhotonCodes.ClientKey] = (object) this.CryptoProvider.PublicKey;
      return this.EnqueueOperation(parameters, PhotonCodes.InitEncryption, true, (byte) 0, false, PeerBase.EgMessageType.InternalOperationRequest);
    }

    internal void DeriveSharedKey(OperationResponse operationResponse)
    {
      if ((int) operationResponse.ReturnCode != 0)
      {
        this.EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. " + operationResponse.ToStringFull());
        this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
      }
      else
      {
        byte[] otherPartyPublicKey = (byte[]) operationResponse[PhotonCodes.ServerKey];
        if (otherPartyPublicKey == null || otherPartyPublicKey.Length == 0)
        {
          this.EnqueueDebugReturn(DebugLevel.ERROR, "Establishing encryption keys failed. Server's public key is null or empty. " + operationResponse.ToStringFull());
          this.EnqueueStatusCallback(StatusCode.EncryptionFailedToEstablish);
        }
        else
        {
          this.CryptoProvider.DeriveSharedKey(otherPartyPublicKey);
          this.isEncryptionAvailable = true;
          this.EnqueueStatusCallback(StatusCode.EncryptionEstablished);
        }
      }
    }

    internal void EnqueueActionForDispatch(PeerBase.MyAction action)
    {
      lock (this.ActionQueue)
        this.ActionQueue.Enqueue(action);
    }

    internal void EnqueueDebugReturn(DebugLevel level, string debugReturn)
    {
      lock (this.ActionQueue)
        this.ActionQueue.Enqueue((PeerBase.MyAction) (() => this.Listener.DebugReturn(level, debugReturn)));
    }

    internal void EnqueueStatusCallback(StatusCode statusValue)
    {
      lock (this.ActionQueue)
        this.ActionQueue.Enqueue((PeerBase.MyAction) (() => this.Listener.OnStatusChanged(statusValue)));
    }

    internal virtual void InitPeerBase()
    {
      this.TrafficStatsIncoming = new TrafficStats(this.TrafficPackageHeaderSize);
      this.TrafficStatsOutgoing = new TrafficStats(this.TrafficPackageHeaderSize);
      this.TrafficStatsGameLevel = new TrafficStatsGameLevel();
      this.ByteCountLastOperation = 0;
      this.ByteCountCurrentDispatch = 0;
      this.bytesIn = 0L;
      this.bytesOut = 0L;
      this.packetLossByCrc = 0;
      this.packetLossByChallenge = 0;
      this.networkSimulationSettings.LostPackagesIn = 0;
      this.networkSimulationSettings.LostPackagesOut = 0;
      lock (this.NetSimListOutgoing)
        this.NetSimListOutgoing.Clear();
      lock (this.NetSimListIncoming)
        this.NetSimListIncoming.Clear();
      this.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
      this.timeBase = SupportClass.GetTickCount();
      this.isEncryptionAvailable = false;
      this.ApplicationIsInitialized = false;
      this.roundTripTime = 300;
      this.roundTripTimeVariance = 0;
      this.packetThrottleInterval = 5000;
      this.serverTimeOffsetIsAvailable = false;
      this.serverTimeOffset = 0;
    }

    internal virtual bool DeserializeMessageAndCallback(byte[] inBuff)
    {
      if (inBuff.Length < 2)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ERROR, "Incoming UDP data too short! " + (object) inBuff.Length);
        return false;
      }
      if ((int) inBuff[0] != 243 && (int) inBuff[0] != 253)
      {
        if (this.debugOut >= DebugLevel.ERROR)
          this.Listener.DebugReturn(DebugLevel.ALL, "No regular operation UDP message: " + (object) inBuff[0]);
        return false;
      }
      byte num1 = (byte) ((uint) inBuff[1] & (uint) sbyte.MaxValue);
      bool flag = ((int) inBuff[1] & 128) > 0;
      MemoryStream memoryStream = (MemoryStream) null;
      if ((int) num1 != 1)
      {
        try
        {
          if (flag)
          {
            inBuff = this.CryptoProvider.Decrypt(inBuff, 2, inBuff.Length - 2);
            memoryStream = new MemoryStream(inBuff);
          }
          else
          {
            memoryStream = new MemoryStream(inBuff);
            memoryStream.Seek(2L, SeekOrigin.Begin);
          }
        }
        catch (Exception ex)
        {
          if (this.debugOut >= DebugLevel.ERROR)
            this.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
          SupportClass.WriteStackTrace(ex);
          return false;
        }
      }
      int num2 = 0;
      switch ((int) num1 - 1)
      {
        case 0:
          this.InitCallback();
          break;
        case 2:
          OperationResponse operationResponse1 = Protocol.DeserializeOperationResponse(memoryStream);
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
            num2 = SupportClass.GetTickCount();
          }
          this.Listener.OnOperationResponse(operationResponse1);
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.TimeForResponseCallback(operationResponse1.OperationCode, SupportClass.GetTickCount() - num2);
            break;
          }
          break;
        case 3:
          EventData eventData = Protocol.DeserializeEventData(memoryStream);
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.CountEvent(this.ByteCountCurrentDispatch);
            num2 = SupportClass.GetTickCount();
          }
          this.Listener.OnEvent(eventData);
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.TimeForEventCallback(eventData.Code, SupportClass.GetTickCount() - num2);
            break;
          }
          break;
        case 6:
          OperationResponse operationResponse2 = Protocol.DeserializeOperationResponse(memoryStream);
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.CountResult(this.ByteCountCurrentDispatch);
            num2 = SupportClass.GetTickCount();
          }
          if ((int) operationResponse2.OperationCode == (int) PhotonCodes.InitEncryption)
            this.DeriveSharedKey(operationResponse2);
          else if ((int) operationResponse2.OperationCode == (int) PhotonCodes.Ping)
          {
            TPeer tpeer = this as TPeer;
            if (tpeer != null)
              tpeer.ReadPingResult(operationResponse2);
            else
              this.EnqueueDebugReturn(DebugLevel.ERROR, "Ping response not used. " + operationResponse2.ToStringFull());
          }
          else
            this.EnqueueDebugReturn(DebugLevel.ERROR, "Received unknown internal operation. " + operationResponse2.ToStringFull());
          if (this.TrafficStatsEnabled)
          {
            this.TrafficStatsGameLevel.TimeForResponseCallback(operationResponse2.OperationCode, SupportClass.GetTickCount() - num2);
            break;
          }
          break;
        default:
          this.EnqueueDebugReturn(DebugLevel.ERROR, "unexpected msgType " + (object) num1);
          break;
      }
      return true;
    }

    internal void SendNetworkSimulated(PeerBase.MyAction sendAction)
    {
      if (!this.NetworkSimulationSettings.IsSimulationEnabled)
        sendAction();
      else if (this.usedProtocol == ConnectionProtocol.Udp && this.NetworkSimulationSettings.OutgoingLossPercentage > 0 && this.lagRandomizer.Next(101) < this.NetworkSimulationSettings.OutgoingLossPercentage)
      {
        ++this.networkSimulationSettings.LostPackagesOut;
      }
      else
      {
        int num1 = this.networkSimulationSettings.OutgoingLag + (this.networkSimulationSettings.OutgoingJitter <= 0 ? 0 : this.lagRandomizer.Next(this.networkSimulationSettings.OutgoingJitter * 2) - this.networkSimulationSettings.OutgoingJitter);
        int num2 = SupportClass.GetTickCount() + num1;
        SimulationItem simulationItem = new SimulationItem()
        {
          ActionToExecute = sendAction,
          TimeToExecute = num2,
          Delay = num1
        };
        lock (this.NetSimListOutgoing)
        {
          if (this.NetSimListOutgoing.Count == 0 || this.usedProtocol == ConnectionProtocol.Tcp)
          {
            this.NetSimListOutgoing.AddLast(simulationItem);
          }
          else
          {
            LinkedListNode<SimulationItem> local_4 = this.NetSimListOutgoing.First;
            while (local_4 != null && local_4.Value.TimeToExecute < num2)
              local_4 = local_4.Next;
            if (local_4 == null)
              this.NetSimListOutgoing.AddLast(simulationItem);
            else
              this.NetSimListOutgoing.AddBefore(local_4, simulationItem);
          }
        }
      }
    }

    internal void ReceiveNetworkSimulated(PeerBase.MyAction receiveAction)
    {
      if (!this.networkSimulationSettings.IsSimulationEnabled)
        receiveAction();
      else if (this.usedProtocol == ConnectionProtocol.Udp && this.networkSimulationSettings.IncomingLossPercentage > 0 && this.lagRandomizer.Next(101) < this.networkSimulationSettings.IncomingLossPercentage)
      {
        ++this.networkSimulationSettings.LostPackagesIn;
      }
      else
      {
        int num1 = this.networkSimulationSettings.IncomingLag + (this.networkSimulationSettings.IncomingJitter <= 0 ? 0 : this.lagRandomizer.Next(this.networkSimulationSettings.IncomingJitter * 2) - this.networkSimulationSettings.IncomingJitter);
        int num2 = SupportClass.GetTickCount() + num1;
        SimulationItem simulationItem = new SimulationItem()
        {
          ActionToExecute = receiveAction,
          TimeToExecute = num2,
          Delay = num1
        };
        lock (this.NetSimListIncoming)
        {
          if (this.NetSimListIncoming.Count == 0 || this.usedProtocol == ConnectionProtocol.Tcp)
          {
            this.NetSimListIncoming.AddLast(simulationItem);
          }
          else
          {
            LinkedListNode<SimulationItem> local_4 = this.NetSimListIncoming.First;
            while (local_4 != null && local_4.Value.TimeToExecute < num2)
              local_4 = local_4.Next;
            if (local_4 == null)
              this.NetSimListIncoming.AddLast(simulationItem);
            else
              this.NetSimListIncoming.AddBefore(local_4, simulationItem);
          }
        }
      }
    }

    /// <summary>
    /// Core of the Network Simulation, which is available in Debug builds.
    ///             Called by a timer in intervals.
    /// 
    /// </summary>
    protected internal void NetworkSimRun()
    {
      while (true)
      {
        bool flag = false;
        lock (this.networkSimulationSettings.NetSimManualResetEvent)
          flag = this.networkSimulationSettings.IsSimulationEnabled;
        if (!flag)
        {
          this.networkSimulationSettings.NetSimManualResetEvent.WaitOne();
        }
        else
        {
          SimulationItem simulationItem;
          lock (this.NetSimListIncoming)
          {
            simulationItem = (SimulationItem) null;
            while (this.NetSimListIncoming.First != null)
            {
              SimulationItem local_1_1 = this.NetSimListIncoming.First.Value;
              if (local_1_1.stopw.ElapsedMilliseconds >= (long) local_1_1.Delay)
              {
                local_1_1.ActionToExecute();
                this.NetSimListIncoming.RemoveFirst();
              }
              else
                break;
            }
          }
          lock (this.NetSimListOutgoing)
          {
            simulationItem = (SimulationItem) null;
            while (this.NetSimListOutgoing.First != null)
            {
              SimulationItem local_1_2 = this.NetSimListOutgoing.First.Value;
              if (local_1_2.stopw.ElapsedMilliseconds >= (long) local_1_2.Delay)
              {
                local_1_2.ActionToExecute();
                this.NetSimListOutgoing.RemoveFirst();
              }
              else
                break;
            }
          }
          Thread.Sleep(0);
        }
      }
    }

    internal void UpdateRoundTripTimeAndVariance(int lastRoundtripTime)
    {
      if (lastRoundtripTime < 0)
        return;
      this.roundTripTimeVariance -= this.roundTripTimeVariance / 4;
      if (lastRoundtripTime >= this.roundTripTime)
      {
        this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
        this.roundTripTimeVariance += (lastRoundtripTime - this.roundTripTime) / 4;
      }
      else
      {
        this.roundTripTime += (lastRoundtripTime - this.roundTripTime) / 8;
        this.roundTripTimeVariance -= (lastRoundtripTime - this.roundTripTime) / 4;
      }
      if (this.roundTripTime < this.lowestRoundTripTime)
        this.lowestRoundTripTime = this.roundTripTime;
      if (this.roundTripTimeVariance <= this.highestRoundTripTimeVariance)
        return;
      this.highestRoundTripTimeVariance = this.roundTripTimeVariance;
    }

    internal void InitializeTrafficStats()
    {
      this.TrafficStatsIncoming = new TrafficStats(this.TrafficPackageHeaderSize);
      this.TrafficStatsOutgoing = new TrafficStats(this.TrafficPackageHeaderSize);
      this.TrafficStatsGameLevel = new TrafficStatsGameLevel();
      this.trafficStatsStopwatch = new Stopwatch();
    }

    internal delegate void MyAction();

    /// <summary>
    /// This is the replacement for the const values used in eNet like: PS_DISCONNECTED, PS_CONNECTED, etc.
    /// 
    /// </summary>
    public enum ConnectionStateValue : byte
    {
      Disconnected = (byte) 0,
      Connecting = (byte) 1,
      Connected = (byte) 3,
      Disconnecting = (byte) 4,
      AcknowledgingDisconnect = (byte) 5,
      Zombie = (byte) 6,
    }

    internal enum EgMessageType : byte
    {
      Init = (byte) 0,
      InitResponse = (byte) 1,
      Operation = (byte) 2,
      OperationResponse = (byte) 3,
      Event = (byte) 4,
      InternalOperationRequest = (byte) 6,
      InternalOperationResponse = (byte) 7,
      Message = (byte) 8,
      RawMessage = (byte) 9,
    }
  }
}
