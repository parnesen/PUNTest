// Decompiled with JetBrains decompiler
// Type: ExitGames.Client.Photon.CmdLogItem
// Assembly: Photon3Unity3D, Version=4.0.0.11, Culture=neutral, PublicKeyToken=null
// MVID: 5CDCDF52-847E-4053-9E9D-E3B4384CF2C6
// Assembly location: D:\altvr\projects\PhotonWebsocket\UnityClient\Assets\Plugins\Photon3Unity3D.dll

namespace ExitGames.Client.Photon
{
  internal class CmdLogItem
  {
    public int TimeInt;
    public int Channel;
    public int SequenceNumber;
    public int Rtt;
    public int Variance;

    public CmdLogItem()
    {
    }

    public CmdLogItem(NCommand command, int timeInt, int rtt, int variance)
    {
      this.Channel = (int) command.commandChannelID;
      this.SequenceNumber = command.reliableSequenceNumber;
      this.TimeInt = timeInt;
      this.Rtt = rtt;
      this.Variance = variance;
    }

    public override string ToString()
    {
      return string.Format("NOW: {0,5}  CH: {1,3} SQ: {2,4} RTT: {3,4} VAR: {4,3}", (object) this.TimeInt, (object) this.Channel, (object) this.SequenceNumber, (object) this.Rtt, (object) this.Variance);
    }
  }
}
