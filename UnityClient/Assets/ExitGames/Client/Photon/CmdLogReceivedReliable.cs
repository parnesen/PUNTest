// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CmdLogReceivedReliable
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  internal class CmdLogReceivedReliable : CmdLogItem
  {
    public int TimeSinceLastSend;
    public int TimeSinceLastSendAck;

    public CmdLogReceivedReliable(NCommand command, int timeInt, int rtt, int variance, int timeSinceLastSend, int timeSinceLastSendAck)
      : base(command, timeInt, rtt, variance)
    {
      this.TimeSinceLastSend = timeSinceLastSend;
      this.TimeSinceLastSendAck = timeSinceLastSendAck;
    }

    public override string ToString()
    {
      return string.Format("Read reliable. {0}  TimeSinceLastSend: {1} TimeSinceLastSendAcks: {2}", (object) base.ToString(), (object) this.TimeSinceLastSend, (object) this.TimeSinceLastSendAck);
    }
  }
}
