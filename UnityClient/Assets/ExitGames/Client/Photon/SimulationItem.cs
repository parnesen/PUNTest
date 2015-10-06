// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.SimulationItem
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

using System.Diagnostics;

namespace ExitGames.Client.Photon
{
  /// <summary>
  /// A simulation item is an action that can be queued to simulate network lag.
  /// 
  /// </summary>
  internal class SimulationItem
  {
    /// <summary>
    /// With this, the actual delay can be measured, compared to the intended lag.
    /// </summary>
    internal readonly Stopwatch stopw;
    /// <summary>
    /// Timestamp after which this item must be executed.
    /// </summary>
    public int TimeToExecute;
    /// <summary>
    /// Action to execute when the lag-time passed.
    /// </summary>
    public PeerBase.MyAction ActionToExecute;

    public int Delay { get; internal set; }

    /// <summary>
    /// Starts a new Stopwatch
    /// </summary>
    public SimulationItem()
    {
      this.stopw = new Stopwatch();
      this.stopw.Start();
    }
  }
}
