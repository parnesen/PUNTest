// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.NetworkSimulationSet
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Threading;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// A set of network simulation settings, enabled (and disabled) by PhotonPeer.IsSimulationEnabled.
  /// 
  /// </summary>
  /// 
  /// <remarks>
  /// For performance reasons, the lag and jitter settings can't be produced exactly.
  ///             In some cases, the resulting lag will be up to 20ms bigger than the lag settings.
  ///             Even if all settings are 0, simulation will be used. Set PhotonPeer.IsSimulationEnabled
  ///             to false to disable it if no longer needed.
  /// 
  ///             All lag, jitter and loss is additional to the current, real network conditions.
  ///             If the network is slow in reality, this will add even more lag.
  ///             The jitter values will affect the lag positive and negative, so the lag settings
  ///             describe the medium lag even with jitter. The jitter influence is: [-jitter..+jitter].
  ///             Packets "lost" due to OutgoingLossPercentage count for BytesOut and LostPackagesOut.
  ///             Packets "lost" due to IncomingLossPercentage count for BytesIn and LostPackagesIn.
  /// 
  /// </remarks>
  public class NetworkSimulationSet
  {
    /// <summary>
    /// internal
    /// </summary>
    private bool isSimulationEnabled = false;
    /// <summary>
    /// internal
    /// </summary>
    private int outgoingLag = 100;
    /// <summary>
    /// internal
    /// </summary>
    private int outgoingJitter = 0;
    /// <summary>
    /// internal
    /// </summary>
    private int outgoingLossPercentage = 1;
    /// <summary>
    /// internal
    /// </summary>
    private int incomingLag = 100;
    /// <summary>
    /// internal
    /// </summary>
    private int incomingJitter = 0;
    /// <summary>
    /// internal
    /// </summary>
    private int incomingLossPercentage = 1;
    public readonly ManualResetEvent NetSimManualResetEvent = new ManualResetEvent(false);
    internal PeerBase peerBase;
    private Thread netSimThread;

    /// <summary>
    /// This setting overrides all other settings and turns simulation on/off. Default: false.
    /// </summary>
    protected internal bool IsSimulationEnabled
    {
      get
      {
        return this.isSimulationEnabled;
      }
      set
      {
        lock (this.NetSimManualResetEvent)
        {
          if (!value)
          {
            lock (this.peerBase.NetSimListIncoming)
            {
              foreach (SimulationItem item_0 in this.peerBase.NetSimListIncoming)
                item_0.ActionToExecute();
              this.peerBase.NetSimListIncoming.Clear();
            }
            lock (this.peerBase.NetSimListOutgoing)
            {
              foreach (SimulationItem item_1 in this.peerBase.NetSimListOutgoing)
                item_1.ActionToExecute();
              this.peerBase.NetSimListOutgoing.Clear();
            }
          }
          this.isSimulationEnabled = value;
          if (this.isSimulationEnabled)
          {
            if (this.netSimThread == null)
            {
              this.netSimThread = new Thread(new ThreadStart(this.peerBase.NetworkSimRun));
              this.netSimThread.IsBackground = true;
              this.netSimThread.Name = "netSim" + (object) SupportClass.GetTickCount();
              this.netSimThread.Start();
            }
            this.NetSimManualResetEvent.Set();
          }
          else
            this.NetSimManualResetEvent.Reset();
        }
      }
    }

    /// <summary>
    /// Outgoing packages delay in ms. Default: 100.
    /// </summary>
    public int OutgoingLag
    {
      get
      {
        return this.outgoingLag;
      }
      set
      {
        this.outgoingLag = value;
      }
    }

    /// <summary>
    /// Randomizes OutgoingLag by [-OutgoingJitter..+OutgoingJitter]. Default: 0.
    /// </summary>
    public int OutgoingJitter
    {
      get
      {
        return this.outgoingJitter;
      }
      set
      {
        this.outgoingJitter = value;
      }
    }

    /// <summary>
    /// Percentage of outgoing packets that should be lost. Between 0..100. Default: 1. TCP ignores this setting.
    /// </summary>
    public int OutgoingLossPercentage
    {
      get
      {
        return this.outgoingLossPercentage;
      }
      set
      {
        this.outgoingLossPercentage = value;
      }
    }

    /// <summary>
    /// Incoming packages delay in ms. Default: 100.
    /// </summary>
    public int IncomingLag
    {
      get
      {
        return this.incomingLag;
      }
      set
      {
        this.incomingLag = value;
      }
    }

    /// <summary>
    /// Randomizes IncomingLag by [-IncomingJitter..+IncomingJitter]. Default: 0.
    /// </summary>
    public int IncomingJitter
    {
      get
      {
        return this.incomingJitter;
      }
      set
      {
        this.incomingJitter = value;
      }
    }

    /// <summary>
    /// Percentage of incoming packets that should be lost. Between 0..100. Default: 1. TCP ignores this setting.
    /// </summary>
    public int IncomingLossPercentage
    {
      get
      {
        return this.incomingLossPercentage;
      }
      set
      {
        this.incomingLossPercentage = value;
      }
    }

    /// <summary>
    /// Counts how many outgoing packages actually got lost. TCP Connections ignore loss and this stays 0.
    /// </summary>
    public int LostPackagesOut { get; internal set; }

    /// <summary>
    /// Counts how many incoming packages actually got lost. TCP Connections ignore loss and this stays 0.
    /// </summary>
    public int LostPackagesIn { get; internal set; }

    public override string ToString()
    {
      return string.Format("NetworkSimulationSet {6}.  Lag in={0} out={1}. Jitter in={2} out={3}. Loss in={4} out={5}.", (object) this.incomingLag, (object) this.outgoingLag, (object) this.incomingJitter, (object) this.outgoingJitter, (object) this.incomingLossPercentage, (object) this.outgoingLossPercentage, (object) (bool) (this.IsSimulationEnabled ? 1 : 0));
    }
  }
}
